using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;

[RequireComponent(typeof(FlockFilter))]
public class FlockUnit : MotionBase {
	public enum State {
		Idle, //no cohesion, move, alignment
		Move, //move toward moveTarget, will use seek if blocked
		Waypoint, //go through generated waypoint
		Wander, //cohesion and alignment
		//Disperse //anti-cohesion and no alignment
	}
			
	public FlockSensor sensor;
	
	public float maxForce = 120.0f; //N
	
	public float pathRadius;
	public float wallRadius;
	
	public float separateDistance = 2.0f;
	public float avoidDistance = 0.0f;
	
	public float separateFactor = 1.5f;
	public float alignFactor = 1.0f;
	public float cohesionFactor = 1.0f;
	public float moveToFactor = 1.0f;
	public float catchUpFactor = 2.0f; //when we have a move target and there are no other flocks around
	public float pathFactor = 1.0f;
	public float wallFactor = 1.0f;
	public float avoidFactor = 1.0f;
	
	public float updateDelay = 1.0f;
	
	public float seekDelay = 1.0f;
	
	public float catchUpMinDistance; //min distance to use catchup factor
	
	public float wanderDelay;
	
	[System.NonSerializedAttribute] public bool wanderEnabled = false; //Set unit to wander if there's no move target
	[System.NonSerializedAttribute] public bool groupMoveEnabled = true; //false = no cohesion and alignment
	[System.NonSerializedAttribute] public bool catchUpEnabled = true; //false = don't use catch up factor
	
	private FlockFilter mFilter = null;
	
	private Transform mMoveTarget = null;
	private float mMoveTargetDist = 0;
	private Vector2 mMoveTargetDir = Vector2.right;
	
	private float mCurUpdateDelay = 0;
	private float mCurSeekDelay = 0;
	private float mWanderStartTime = 0;
	
	private Transform mTrans;
	
	private Seeker mSeek; //use for when our target is blocked
	private Path mSeekPath = null;
	private int mSeekCurPath = -1;
	private bool mSeekStarted = false;
	
	private bool mWallCheck = false;
	private RaycastHit mWallHit;
	
	private float mRadius;
	private float mPathRadiusSqr;
	
	private State mState = State.Move;
	
	public FlockType type {
		get { return mFilter.type; }
		set { mFilter.type = value; }
	}
	
	public Transform moveTarget {
		get { return mMoveTarget; }
		
		set {
			if(mMoveTarget != value) {
				mMoveTarget = value;
				
				SeekPathStop();
			}
		}
	}
	
	public float moveTargetDistance {
		get { return mMoveTargetDist; }
	}
	
	/// <summary>
	/// Direction from unit towards move target. (Note: this is updated based on seek delay or update delay)
	/// </summary>
	public Vector2 moveTargetDir {
		get { return mMoveTargetDir; }
	}
	
	void OnDestroy() {
		mSeek.pathCallback -= OnSeekPathComplete;
	}
		
	protected override void Awake() {
		base.Awake();
		
		mFilter = GetComponent<FlockFilter>();
		
		mTrans = transform;
		
		mSeek = GetComponent<Seeker>();
		mSeek.pathCallback += OnSeekPathComplete;
		
		SphereCollider sc = GetComponent<SphereCollider>();
		mRadius = sc != null ? sc.radius : 0.0f;
		
		mPathRadiusSqr = pathRadius*pathRadius;
	}
	
	void Update() {
		Vector3 pos = transform.position;
		
		//check current pathing
		if(moveTarget != null) {
			if(mSeekStarted) {
				if(mSeekPath != null) {
					mCurSeekDelay += Time.deltaTime;
					if(mCurSeekDelay >= seekDelay) {
						//check if target is blocked, also update move dir
						Vector3 dest = moveTarget.position;
						mMoveTargetDir = (dest - pos);
						mMoveTargetDist = mMoveTargetDir.magnitude;
						mMoveTargetDir /= mMoveTargetDist;
						
						//check to see if destination has changed or no longer blocked
						if(dest != mSeekPath.vectorPath[mSeekPath.vectorPath.Count-1]
							|| !CheckTargetBlock(pos, mMoveTargetDir, mMoveTargetDist, mRadius)) {
							SeekPathStop();
						}
						else {
							mCurSeekDelay = 0.0f;
						}
					}
				}
			}
			else {
				mCurSeekDelay += Time.deltaTime;
				if(mCurSeekDelay >= seekDelay) {
					//check if target is blocked, also update move dir
					Vector3 dest = moveTarget.position;
					mMoveTargetDir = (dest - pos);
					mMoveTargetDist = mMoveTargetDir.magnitude;
					mMoveTargetDir /= mMoveTargetDist;
					
					if(CheckTargetBlock(pos, mMoveTargetDir, mMoveTargetDist, mRadius)) {
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
		if(mState == State.Waypoint) {
			//need to keep checking per frame if we have reached a waypoint
			Vector2 desired = mSeekPath.vectorPath[mSeekCurPath] - transform.position;
			
			//check if we need to move to next waypoint
			if(desired.sqrMagnitude < mPathRadiusSqr) {
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
				mCurUpdateDelay += Time.fixedDeltaTime;
				if(mCurUpdateDelay >= updateDelay) {
					mCurUpdateDelay = 0;
					
					//continue moving to wp
					desired.Normalize();
					Vector2 sumForce = ComputeSeparate() + desired*(maxForce*pathFactor);
					body.AddForce(sumForce.x, sumForce.y, 0.0f);
				}
			}
		}
		else {
			mCurUpdateDelay += Time.fixedDeltaTime;
			if(mCurUpdateDelay >= updateDelay) {
				mCurUpdateDelay = 0;
				
				Vector2 sumForce = Vector2.zero;
				
				switch(mState) {
				case State.Move:
					if(moveTarget == null) {
						ApplyState(wanderEnabled ? State.Wander : State.Idle);
					}
					else {
						//move to destination
						Vector2 pos = mTrans.position;
						Vector2 dest = moveTarget.position;
						Vector2 _dir = dest - pos;
						mMoveTargetDist = _dir.magnitude;
						
						//catch up?
						float factor = catchUpEnabled 
							&& (!groupMoveEnabled || sensor == null || sensor.units.Count == 0) 
							&& mMoveTargetDist > catchUpMinDistance ? 
								catchUpFactor : moveToFactor;
						
						sumForce = groupMoveEnabled ? ComputeMovement() : ComputeSeparate();
						
						if(mMoveTargetDist > 0) {
							_dir /= mMoveTargetDist;
							
							mMoveTargetDir = _dir;
							
							sumForce += M8.Math.Steer(body.velocity, _dir*maxSpeed, maxForce, factor);
						}
					}
					break;
					
				case State.Wander:
					if(Time.fixedTime - mWanderStartTime >= wanderDelay) {
						WanderRefresh();
					}
					
					sumForce = ComputeSeparate() + M8.Math.Steer(body.velocity, mMoveTargetDir*maxSpeed, maxForce, moveToFactor);
					break;
					
				default:
					sumForce = ComputeSeparate();
					break;
				}
				
				body.AddForce(sumForce.x, sumForce.y, 0.0f);
			}
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
		
		Gizmos.color = Color.yellow;
		Gizmos.color *= 0.75f;
		Gizmos.DrawWireSphere(transform.position, avoidDistance);
	}
	
	void OnSeekPathComplete(Path p) {
		if(p == null || p.vectorPath.Count == 0) {
			SeekPathStop();
		}
		else {
			mSeekPath = p;
			mSeekCurPath = 0;
			
			ApplyState(State.Waypoint);
		}
	}
	
	private bool CheckTargetBlock(Vector3 pos, Vector3 dir, float dist, float radius) {
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
		
		ApplyState(State.Idle);
	}
	
	private void SeekPathStop() {
		mSeekPath = null;
		mCurSeekDelay = 0.0f;
		mSeekStarted = false;
		mSeekCurPath = -1;
		
		ApplyState(mMoveTarget == null ? wanderEnabled ? State.Wander : State.Idle : State.Move);
	}
	
	private void ApplyState(State toState) {
		
		mState = toState;
		
		switch(mState) {
		case State.Wander:
			WanderRefresh();
			break;
		}
	}
	
	private void WanderRefresh() {
		mWanderStartTime = Time.fixedTime;
		
		mMoveTargetDir = Random.onUnitSphere;
	}
		
	//use if mWallCheck is true
	private Vector2 Wall() {
		return M8.Math.Steer(body.velocity, mWallHit.normal*maxSpeed, maxForce, wallFactor);
	}
	
	private Vector2 Seek(Vector2 target, float factor) {
		Vector2 pos = mTrans.position;
		
		Vector2 desired = target - pos;
		desired.Normalize();
		
		return M8.Math.Steer(body.velocity, desired*maxSpeed, maxForce, factor);
	}
	
	//use for idle, waypoint, etc.
	private Vector2 ComputeSeparate() {
		Vector2 forceRet = Vector2.zero;
		
		if(sensor != null && sensor.units.Count > 0) {
			Vector2 separate = Vector2.zero;
			Vector2 avoid = Vector2.zero;
			
			Vector2 pos = mTrans.position;
			
			Vector2 dPos;
			float dist;
			
			int numSeparate = 0;
			int numAvoid = 0;
			
			foreach(FlockFilter unit in sensor.units) {
				Vector2 otherPos = unit.transform.position;
				
				dPos = pos - otherPos;
				dist = dPos.magnitude;
				
				if(mFilter.CheckAvoid(unit.type)) {
					//avoid
					if(dist < avoidDistance) {
						dPos /= dist;
						avoid += dPos;
						numAvoid++;
					}
				}
				else {
					//separate	
					if(dist < separateDistance) {
						dPos /= dist;
						separate += dPos;
						numSeparate++;
					}
				}
			}
			
			//calculate separate
			if(numSeparate > 0) {
				separate /= (float)numSeparate;
				
				dist = separate.magnitude;
				if(dist > 0) {
					separate /= dist;
					forceRet += M8.Math.Steer(body.velocity, separate*maxSpeed, maxForce, separateFactor);
				}
			}
			
			//calculate avoid
			if(numAvoid > 0) {
				avoid /= (float)numAvoid;
				
				dist = avoid.magnitude;
				if(dist > 0) {
					avoid /= dist;
					forceRet += M8.Math.Steer(body.velocity, avoid*maxSpeed, maxForce, avoidFactor);
				}
			}
		}
		
		return forceRet;
	}
	
	private Vector2 ComputeMovement() {
		Vector2 forceRet = Vector2.zero;
		
		if(sensor != null && sensor.units.Count > 0) {
			Vector2 separate = Vector2.zero;
			Vector2 align = Vector2.zero;
			Vector2 cohesion = Vector2.zero;
			Vector2 avoid = Vector2.zero;
			
			Vector2 pos = mTrans.position;
			
			Vector2 dPos;
			float dist;
			
			int numFollow = 0;
			int numSeparate = 0;
			int numAvoid = 0;
			
			foreach(FlockFilter unit in sensor.units) {
				Vector2 otherPos = unit.transform.position;
				
				dPos = pos - otherPos;
				dist = dPos.magnitude;
				
				if(mFilter.CheckAvoid(unit.type)) {
					//avoid
					if(dist < avoidDistance) {
						dPos /= dist;
						avoid += dPos;
						numAvoid++;
					}
				}
				else if(mFilter.type == unit.type) {
					//separate	
					if(dist < separateDistance) {
						dPos /= dist;
						separate += dPos;
						numSeparate++;
					}
					
					//only follow if it has a legit body
					if(unit.isLegit) {
						Rigidbody otherBody = unit.rigidbody;
						if(otherBody != null && !otherBody.isKinematic) {
							//align speed
							Vector2 vel = otherBody.velocity;
							align += vel;
							
							//cohesion
							cohesion += otherPos;
						
							numFollow++;
						}
					}
				}
			}
			
			//calculate separate
			if(numSeparate > 0) {
				separate /= (float)numSeparate;
				
				dist = separate.magnitude;
				if(dist > 0) {
					separate /= dist;
					forceRet += M8.Math.Steer(body.velocity, separate*maxSpeed, maxForce, separateFactor);
					
				}
			}
			
			//calculate avoid
			if(numAvoid > 0) {
				avoid /= (float)numAvoid;
				
				dist = avoid.magnitude;
				if(dist > 0) {
					avoid /= dist;
					forceRet += M8.Math.Steer(body.velocity, avoid*maxSpeed, maxForce, avoidFactor);
				}
			}
			
			if(numFollow > 0) {
				float fCount = (float)numFollow;
				
				//calculate align
				align /= fCount;
				align.Normalize();
				align = M8.Math.Steer(body.velocity, align*maxSpeed, maxForce, alignFactor);
				
				//calculate cohesion
				cohesion /= fCount;
				cohesion = Seek(cohesion, cohesionFactor);
				
				forceRet += align + cohesion;
			}
		}
		
		return forceRet;
	}
}
