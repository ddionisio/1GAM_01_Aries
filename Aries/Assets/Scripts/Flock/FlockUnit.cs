using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FlockUnit : MonoBehaviour {
	public FlockType type;
	
	public FlockSensor sensor;
			
	public float maxForce = 120.0f; //N
	public float maxSpeed = 20.0f; //meters/sec
	
	public float separateDistance = 2.0f;
	
	public float separateFactor = 1.5f;
	public float alignFactor = 1.0f;
	public float cohesionFactor = 1.0f;
	public float moveToFactor = 1.0f;
	public float wallFactor = 1.0f;
	
	public float updateDelay = 1.0f;
	
	public float wallCheckRadius = 5.0f;
	
	[System.NonSerialized] public Transform moveTarget = null;
	
	private float mCurUpdateDelay = 0;
	
	private Vector2 mDir = Vector2.right;
	private Rigidbody mBody;
	private Transform mTrans;
	
	private RaycastHit mWallCheckHit;
	private bool mWallCheck;
	
	private HashSet<FlockAnti> mAntis = new HashSet<FlockAnti>();
	
	public Vector2 dir {
		get { return mDir; }
	}
	
	public Rigidbody body {
		get { return mBody; }
	}
		
	void Awake() {
		if(sensor != null) {
			sensor.typeFilter = type;
		}
		
		mBody = rigidbody;
		mTrans = transform;
	}
	
	// Use this for initialization
	void Start () {
	
	}
	
	void Update() {
		mWallCheck = Physics.SphereCast(transform.position, wallCheckRadius, mDir, out mWallCheckHit, 0.1f, Layers.layerMaskWall);
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		mCurUpdateDelay += Time.fixedDeltaTime;
		if(mCurUpdateDelay >= updateDelay) {
			mCurUpdateDelay = 0;
			
			Vector2 sumForce = Vector2.zero;
			
			Vector2 sep, align, coh;
			
			ComputeMovement(out sep, out align, out coh);
									
			sumForce += sep + align + coh;
			
			//move to destination
			if(!mWallCheck && moveToFactor != 0.0f && moveTarget != null) {
				Vector2 targetPos = moveTarget.position;
				
				float factor = moveToFactor;
				if(sensor == null || sensor.units.Count == 0) {
					factor *= 2.0f; //catch up
				}
				
				Vector2 seek = Seek(targetPos, factor);
				
				sumForce += seek;
			}
			
			if(mAntis.Count > 0) {
				Vector2 away = Anti();
				
				sumForce += away;
			}
			
			mBody.AddForce(sumForce.x, sumForce.y, 0.0f);
		}
		
		//wall check (TODO: just use some cheap pathfinding)
		if(mWallCheck && wallFactor != 0.0f) {
			Vector2 wallOff = Wall();
			mBody.AddForce(wallOff.x, wallOff.y, 0.0f);
		}
						
		//get direction and limit speed
		Vector2 vel = mBody.velocity;
		float velD = vel.magnitude;
		
		if(velD > 0) {
			mDir = vel/velD;
			
			if(velD > maxSpeed) {
				mBody.velocity = mDir*maxSpeed;
			}
		}
	}
		
	void OnTriggerEnter(Collider t) {
		if(t.gameObject.layer == Layers.layerFlockAnti) {
			FlockAnti anti = t.GetComponent<FlockAnti>();
			if(anti.typeFilter == FlockType.All || anti.typeFilter == type) {
				mAntis.Add(anti);
			}
		}
	}
	
	void OnTriggerExit(Collider t) {
		if(t.gameObject.layer == Layers.layerFlockAnti) {
			FlockAnti anti = t.GetComponent<FlockAnti>();
			if(anti.typeFilter == FlockType.All || anti.typeFilter == type) {
				mAntis.Remove(anti);
			}
		}
	}
	
	void OnDrawGizmosSelected() {
		Gizmos.color = Color.gray;
		
		Gizmos.DrawWireSphere(transform.position, wallCheckRadius);
	}
	
	private Vector2 CalculateSteerForce(Vector2 dir, float factor) {
		Vector2 velocity = mBody.velocity;
		
		Vector2 steer = dir*maxSpeed - velocity;
		
		return M8.Math.Limit(steer, maxForce)*factor;
	}
	
	//use if mWallCheck is true
	private Vector2 Wall() {
		Vector2 dir = M8.Math.Reflect(mDir, mWallCheckHit.normal);
				
		return dir*(maxForce*wallFactor);
	}
	
	//use if mAntis.Count > 0
	private Vector2 Anti() {
		Vector2 pos = mTrans.localPosition;
		
		Vector2 forces = Vector2.zero;
		
		Vector2 away = Vector2.zero;
		
		float awayFactor = 0.0f;
		
		int numAway = 0;
				
		foreach(FlockAnti anti in mAntis) {
			Vector2 antiPos = anti.transform.position;
			
			switch(anti.type) {
			case FlockAnti.Type.Away:
				away += (pos - antiPos).normalized;
				awayFactor += anti.factor;
				numAway++;
				break;
				
			case FlockAnti.Type.Force:
				forces += (pos - antiPos).normalized*anti.force;
				break;
				
			case FlockAnti.Type.Dir:
				away += anti.dir;
				awayFactor += anti.factor;
				numAway++;
				break;
				
			case FlockAnti.Type.DirForce:
				forces += anti.dir*anti.force;
				break;
			}
		}
		
		if(numAway > 0) {
			float fCount = (float)numAway;
			
			away /= fCount;
			awayFactor /= fCount;
			
			float dist = away.magnitude;
			if(dist > 0) {
				away /= dist;
				away = CalculateSteerForce(away, awayFactor);
			}
		}
		
		return away + forces;
	}
		
	private Vector2 Seek(Vector2 target, float factor) {
		Vector2 pos = mTrans.localPosition;
		
		Vector2 desired = target - pos;
		desired.Normalize();
		
		return CalculateSteerForce(desired, factor);
	}
	
	private void ComputeMovement(out Vector2 separate, out Vector2 align, out Vector2 cohesion) {
		separate = Vector2.zero;
		align = Vector2.zero;
		cohesion = Vector2.zero;
		
		if(sensor != null && sensor.units.Count > 0) {
			Vector2 pos = mTrans.localPosition;
			
			Vector2 dPos;
			float dist;
			
			int numSeparate = 0;
			
			foreach(FlockUnit unit in sensor.units) {
				Vector2 otherPos = unit.transform.localPosition;
				Vector2 otherVel = unit.body.velocity;
				
				//separate
				dPos = pos - otherPos;
				dist = dPos.magnitude;
				if(dist < separateDistance) {
					dPos /= dist;
					separate += dPos;
					numSeparate++;
				}
				
				//align speed
				align += otherVel;
				
				//cohesion
				cohesion += otherPos;
			}
			
			//calculate separate
			if(numSeparate > 0) {
				separate /= (float)numSeparate;
				
				dist = separate.magnitude;
				if(dist > 0) {
					separate /= dist;
					separate = CalculateSteerForce(separate, separateFactor);
				}
			}
			
			float fCount = (float)sensor.units.Count;
			
			//calculate align
			align /= fCount;
			align.Normalize();
			align = CalculateSteerForce(align.normalized, alignFactor);
			
			//calculate cohesion
			cohesion /= fCount;
			cohesion = Seek(cohesion, cohesionFactor);
		}
	}
}
