using UnityEngine;
using System.Collections;

//collider is used for following, assume it's trigger
public class FlockActionController : ActionListener {
	
	public FlockUnit flockUnit;
	
	public float stopDelay = 1.0f;
	
	public float resumeDistance = 2.0f;
	
	public UnitSpriteController spriteControl;
	
	private bool mStopActive = false;
	private float mCurStopDelay = 0.0f;
	private Vector3 mLastFollowPos = Vector3.zero;
	private float mResumeDistanceSqr;
	
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
		mResumeDistanceSqr = resumeDistance*resumeDistance;
	}
	
	void Update() {
		if(mStopActive) {
			mCurStopDelay += Time.deltaTime;
			if(mCurStopDelay >= stopDelay) {
				mCurStopDelay = 0;
				
				//stop after some delay while following
				if(flockUnit.moveTarget != null) {
					flockUnit.moveTarget = null;
				}
				else if(currentTarget.type == ActionType.Follow) {
					if((currentTarget.target.position - mLastFollowPos).sqrMagnitude >= mResumeDistanceSqr) {
						ResumeFollow(null, true);
					}
				}
			}
		}
	}
	
	void ResumeFollow(Collider targetCheck, bool stopActive) {
		if(targetCheck == null || targetCheck == currentTarget.collider) {
			flockUnit.moveTarget = currentTarget.target;
			mLastFollowPos = flockUnit.moveTarget.position;
			mStopActive = stopActive;
		}
	}
	
	//using flock's sensor to determine when to stop moving while following
	void OnTriggerEnter(Collider other) {
		if(!mStopActive) {
			mStopActive = 
				currentTarget.type == ActionType.Follow
				&& currentTarget.collider == other;
		}
	}
	
	void OnTriggerExit(Collider other) {
		if(mStopActive) {
			//go back to following
			ResumeFollow(other, false);
		}
	}
}
