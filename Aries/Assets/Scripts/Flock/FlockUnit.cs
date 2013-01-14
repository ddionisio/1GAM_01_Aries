using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;

public class FlockUnit : MotionBase {
	public enum State {
		Idle, //no cohesion, move, alignment
		Move, //move toward moveTarget, will use seek if blocked
		Waypoint, //go through generated waypoint
		//Wander, //cohesion and alignment
		//Disperse //anti-cohesion and no alignment
	}
	
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
	
	private State mState = State.Move;
	
	public Transform moveTarget {
		get { return mMoveTarget; }
		
		set {
			if(mMoveTarget != value) {
				mMoveTarget = value;
				
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
	
	void Update() {
		Vector3 pos = transform.position;
		
		//check current pathing
		if(moveTarget != null) {
			if(mSeekStarted) {
				if(mSeekPath != null) {
					mCurSeekDelay += Time.deltaTime;
					if(mCurSeekDelay >= seekDelay) {
						Vector3 dest = moveTarget.position;
						
						//check to see if destination has changed or no longer blocked
						if(dest != mSeekPath.vectorPath[mSeekPath.vectorPath.Count-1]
							|| !CheckTargetBlock(pos, dest, mRadius)) {
							SeekPathStop();
						}
						else {
							mCurSeekDelay = 0.0f;
						}
					}
					else {
						
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
			
			switch(mState) {
			case State.Waypoint:
				Vector2 desired = mSeekPath.vectorPath[mSeekCurPath] - transform.position;
				float distance = desired.magnitude;
				
				//check if we need to move to next waypoint
				if(distance < pathRadius) {
					//path complete?
					int nextPath = mSeekCurPath+1;
					if(nextPath == mSeekPath.vectorPath.Count) {
						SeekPathStop();
					}
					else {
						mSeekCurPath = nextPath;
					}
				}
				else {
					//continue moving to wp
					desired /= distance;
					
					sumForce = ComputeSeparate() + desired*(maxForce*pathFactor);
				}
				break;
				
			case State.Move:
				if(moveTarget == null) {
					mState = State.Idle;
				}
				else {
					//move to destination
					
					//catch up?
					float factor = sensor == null || sensor.units.Count == 0 ? catchUpFactor : moveToFactor;
					
					sumForce = ComputeMovement() + Seek(moveTarget.position, factor);
				}
				break;
				
			default:
				sumForce = ComputeSeparate();
				break;
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
		if(p == null || p.vectorPath.Count == 0) {
			SeekPathStop();
		}
		else {
			mSeekPath = p;
			mSeekCurPath = 0;
			
			mState = State.Waypoint;
		}
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
		
		mState = State.Idle;
	}
	
	private void SeekPathStop() {
		mSeekPath = null;
		mCurSeekDelay = 0.0f;
		mSeekStarted = false;
		mSeekCurPath = -1;
		
		mState = mMoveTarget == null ? State.Idle : State.Move;
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
	
	//use for idle, waypoint, etc.
	private Vector2 ComputeSeparate() {
		Vector2 separate = Vector2.zero;
		
		if(sensor != null && sensor.units.Count > 0) {
			Vector2 pos = mTrans.localPosition;
			
			Vector2 dPos;
			float dist;
			
			int numSeparate = 0;
			
			foreach(FlockUnit unit in sensor.units) {
				Vector2 otherPos = unit.transform.localPosition;
				
				//separate
				dPos = pos - otherPos;
				dist = dPos.magnitude;
				if(dist < separateDistance) {
					dPos /= dist;
					separate += dPos;
					numSeparate++;
				}
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
		}
		
		return separate;
	}
	
	private Vector2 ComputeMovement() {
		Vector2 forceRet;
		
		if(sensor != null && sensor.units.Count > 0) {
			Vector2 separate = Vector2.zero;
			Vector2 align = Vector2.zero;
			Vector2 cohesion = Vector2.zero;
			
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
			
			forceRet = separate + align + cohesion;
		}
		else {
			forceRet = Vector2.zero;
		}
		
		return forceRet;
	}
}
