using UnityEngine;
using System.Collections;

//collider is used for following, assume it's trigger
public class FlockActionController : ActionListener {
	
	public FlockUnit flockUnit;
	
	public float followStopDelay = 1.0f;
	public float followStopRadius = 2.0f;
	public float followResumeRadius = 1.0f;
	
	private bool mStopActive = false;
	private float mCurStopDelay = 0.0f;
	private Vector3 mLastFollowPos = Vector3.zero;
	
	private float mFollowStopRadiusSqr;
	private float mFollowResumeRadiusSqr;
	
	protected override void OnActionEnter() {
		mStopActive = false;
		mCurStopDelay = 0.0f;
		
		switch(currentTarget.type) {
		case ActionType.Disperse:
			break;
			
		case ActionType.Follow:
			flockUnit.moveTarget = currentTarget.target;
			
			mLastFollowPos = flockUnit.moveTarget.position;
			break;
			
		default:
			flockUnit.moveTarget = currentTarget.target;
			break;
		}
	}
	
	protected override void OnActionExit() {
	}
	
	protected override void OnActionFinish() {
		//do something
		mStopActive = false;
		mCurStopDelay = 0.0f;
		
		flockUnit.moveTarget = null;
	}
	
	void Awake() {
		mFollowStopRadiusSqr = followStopRadius*followStopRadius;
		mFollowResumeRadiusSqr = followResumeRadius*followResumeRadius;
	}
	
	void Update() {
		if(currentTarget != null) {
			if(currentTarget.type == ActionType.Follow) {
				mCurStopDelay += Time.deltaTime;
				if(mCurStopDelay >= followStopDelay) {
					mCurStopDelay = 0;
					
					if(mStopActive) {
						//check if target's position delta exceeds resume radius
						if((currentTarget.target.position - mLastFollowPos).sqrMagnitude >= mFollowResumeRadiusSqr) {
							flockUnit.moveTarget = currentTarget.target;
							mStopActive = false;
						}
						//stop after some delay while following
						else if(flockUnit.moveTarget != null) {
							flockUnit.moveTarget = null;
						}
					}
					//within stopping distance, stop!
					else if((transform.position - currentTarget.target.position).sqrMagnitude < mFollowStopRadiusSqr) {
						mLastFollowPos = currentTarget.target.position;
						mStopActive = true;
					}
				}
			}
		}
	}
	
	void ResumeFollow(Collider targetCheck, bool stopActive) {
		if(targetCheck == null || targetCheck == currentTargetCollider) {
			Transform t = currentTarget.target;
			flockUnit.moveTarget = t;
			mLastFollowPos = t.position;
			mStopActive = stopActive;
		}
	}
}
