using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;

public class FlockUnit : MotionBase {
	public FlockType type;
	
	public FlockSensor sensor;
	
	public float maxForce = 120.0f; //N
	
	public float pathRadius;
	public float wallRadius;
	
	public float separateDistance = 2.0f;
	
	public float separateFactor = 1.5f;
	public float alignFactor = 1.0f;
	public float cohesionFactor = 1.0f;
	public float moveToFactor = 1.0f;
	public float catchUpFactor = 2.0f; //when we have a move target and there are no other flocks around
	public float pathFactor = 1.0f;
	public float wallFactor = 1.0f;
	
	public float updateDelay = 1.0f;
	
	public float seekDelay = 1.0f;
	
	private Transform mMoveTarget = null;
	
	private float mCurUpdateDelay = 0;
	private float mCurSeekDelay = 0;
	
	private Transform mTrans;
	
	private Seeker mSeek; //use for when our target is blocked
	private Path mSeekPath = null;
	private int mSeekCurPath = -1;
	private bool mSeekStarted = false;
	
	private bool mWallCheck = false;
	private RaycastHit mWallHit;
	
	private float mRadius;
	
	public Transform moveTarget {
		get { return mMoveTarget; }
		
		set {
			mMoveTarget = value;
			if(mMoveTarget == null) {
				SeekPathStop();
			}
		}
	}
			
	void OnDestroy() {
		mSeek.pathCallback -= OnSeekPathComplete;
	}
		
	protected override void Awake() {
		base.Awake();
		
		mTrans = transform;
		
		if(sensor != null) {
			sensor.typeFilter = type;
		}
		
		mSeek = GetComponent<Seeker>();
		mSeek.pathCallback += OnSeekPathComplete;
		
		SphereCollider sc = GetComponent<SphereCollider>();
		mRadius = sc != null ? sc.radius : 0.0f;
	}
	
	// Use this for initialization
	void Start () {
	
	}
	
	void Update() {
		Vector3 pos = transform.position;
		
		//check current pathing
		if(moveTarget != null) {
			if(mSeekStarted) {
				if(mSeekPath != null) {
					mCurSeekDelay += Time.deltaTime;
					if(mCurSeekDelay >= seekDelay) {
						Vector3 dest = moveTarget.position;
						
						if(dest != mSeekPath.vectorPath[mSeekPath.vectorPath.Count-1]
							|| !CheckTargetBlock(pos, dest, mRadius)) {
							SeekPathStop();
						}
						else {
							mCurSeekDelay = 0.0f;
						}
					}
					else {
						//check if we need to move to next waypoint
						Vector3 wp = mSeekPath.vectorPath[mSeekCurPath];
						
						if(Vector3.Distance(pos, wp) < pathRadius) {
							//no longer need to follow path
							int nextPath = mSeekCurPath+1;
							if(nextPath == mSeekPath.vectorPath.Count) {
								SeekPathStop();
							}
							else {
								mSeekCurPath = nextPath;
							}
						}
					}
				}
			}
			else {
				mCurSeekDelay += Time.deltaTime;
				if(mCurSeekDelay >= seekDelay) {
					//check if target is blocked
					Vector3 dest = moveTarget.position;
					if(CheckTargetBlock(pos, dest, mRadius)) {
						SeekPathStart(pos, dest);
					}
					else {
						mCurSeekDelay = 0.0f;
					}
				}
			}
		}
		
		//check wall
		mWallCheck = Physics.SphereCast(pos, wallRadius, dir, out mWallHit, 0.1f, Layers.layerMaskWall);
	}
	
	// Update is called once per frame
	protected override void FixedUpdate () {
		mCurUpdateDelay += Time.fixedDeltaTime;
		if(mCurUpdateDelay >= updateDelay) {
			mCurUpdateDelay = 0;
			
			Vector2 sumForce = Vector2.zero;
				
			Vector2 sep, align, coh;
			ComputeMovement(out sep, out align, out coh);
			
			//seek path
			if(mSeekStarted) {
				sumForce += sep;
				
				if(mSeekPath != null) {
					Vector2 target = mSeekPath.vectorPath[mSeekCurPath];
					Vector2 pos = transform.position;
					Vector2 desired = target - pos;
					desired.Normalize();
					sumForce += desired*(maxForce*pathFactor);
					//sumForce += Seek(target, pathFactor);
				}
			}
			else {
				sumForce += sep + align + coh;
				
				if(moveToFactor != 0.0f && moveTarget != null) {
					//move to destination
					Vector2 targetPos = moveTarget.position;
					
					//catch up?
					float factor = sensor == null || sensor.units.Count == 0 ? catchUpFactor : moveToFactor;
					
					Vector2 seek = Seek(targetPos, factor);
					
					sumForce += seek;
				}
			}
			
			body.AddForce(sumForce.x, sumForce.y, 0.0f);
		}
		
		if(mWallCheck) {
			Vector2 wall = Wall();
			body.AddForce(wall.x, wall.y, 0.0f);
		}
		
		base.FixedUpdate();
	}
	
	void OnDrawGizmosSelected() {
		Gizmos.color = Color.gray;
		Gizmos.DrawWireSphere(transform.position, pathRadius);
		
		Gizmos.color *= 1.25f;
		Gizmos.DrawWireSphere(transform.position, wallRadius);
	}
	
	void OnSeekPathComplete(Path p) {
		mSeekPath = p;
		mSeekCurPath = 0;
	}
	
	private bool CheckTargetBlock(Vector3 pos, Vector3 dest, float radius) {
		Vector3 dir = dest - pos;
		float dist = dir.magnitude;
		if(dist > 0.0f) {
			Ray ray = new Ray(pos, dir/dist);
			return Physics.SphereCast(ray, pathRadius, dist, Layers.layerMaskWall);
		}
		else {
			return false;
		}
	}
	
	private void SeekPathStart(Vector3 start, Vector3 dest) {
		mSeek.StartPath(start, dest);
		mSeekPath = null;
		mCurSeekDelay = 0.0f;
		mSeekStarted = true;
	}
	
	private void SeekPathStop() {
		mSeekPath = null;
		mCurSeekDelay = 0.0f;
		mSeekStarted = false;
		mSeekCurPath = -1;
	}
		
	//use if mWallCheck is true
	private Vector2 Wall() {
		return M8.Math.Steer(body.velocity, mWallHit.normal*maxSpeed, maxForce, wallFactor);
	}
	
	private Vector2 Seek(Vector2 target, float factor) {
		Vector2 pos = mTrans.localPosition;
		
		Vector2 desired = target - pos;
		desired.Normalize();
		
		return M8.Math.Steer(body.velocity, desired*maxSpeed, maxForce, factor);
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
					separate = M8.Math.Steer(body.velocity, separate*maxSpeed, maxForce, separateFactor);
				}
			}
			
			float fCount = (float)sensor.units.Count;
			
			//calculate align
			align /= fCount;
			align.Normalize();
			align = M8.Math.Steer(body.velocity, align*maxSpeed, maxForce, alignFactor);
			
			//calculate cohesion
			cohesion /= fCount;
			cohesion = Seek(cohesion, cohesionFactor);
		}
	}
}
