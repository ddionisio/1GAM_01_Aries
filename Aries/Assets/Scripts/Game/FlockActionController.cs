using UnityEngine;
using System.Collections;

//collider is used for following, assume it's trigger
public class FlockActionController : MonoBehaviour {
	
	public FlockUnit flockUnit;
	
	public float stopDelay = 1.0f;
	
	public float resumeDistance = 2.0f;
	
	private Transform mNoActionFollow = null;
	private ActionListener mCurListener = null;
	
	private bool mStopActive = false;
	private float mCurStopDelay = 0.0f;
	private Vector3 mLastFollowPos = Vector3.zero;
	private float mResumeDistanceSqr;
	
	public ActionListener curListener {
		get { return mCurListener; }
	}
	
	public Transform noActionFollow {
		get { return mNoActionFollow; }
		set { mNoActionFollow = value; mLastFollowPos = mNoActionFollow.position; }
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
					flockUnit.restrictMove = true;
					flockUnit.moveTarget = null;
				}
				else if(mCurListener != null && mCurListener.currentTarget.type == ActionTarget.Type.Follow) {
					if((mCurListener.currentTarget.target.position - mLastFollowPos).sqrMagnitude >= mResumeDistanceSqr) {
						ResumeFollow(null, true);
					}
				}
				else if(mNoActionFollow != null 
					&& (mNoActionFollow.position - mLastFollowPos).sqrMagnitude >= mResumeDistanceSqr) {
					ResumeFollow(null, true);
				}
			}
		}
	}
	
	void ResumeFollow(Transform targetCheck, bool stopActive) {
		if(mCurListener != null && mCurListener.currentTarget.type == ActionTarget.Type.Follow) {
			if(targetCheck == null || targetCheck == mCurListener.currentTarget.target) {
				flockUnit.restrictMove = false;
				flockUnit.moveTarget = mCurListener.currentTarget.target;
				mLastFollowPos = flockUnit.moveTarget.position;
				mStopActive = stopActive;
			}
		}
		else if(mNoActionFollow != null) {
			if(targetCheck == null || targetCheck == mNoActionFollow) {
				flockUnit.restrictMove = false;
				flockUnit.moveTarget = mNoActionFollow;
				mLastFollowPos = mNoActionFollow.position;
				mStopActive = stopActive;
			}
		}
	}
	
	//using flock's sensor to determine when to stop moving while following
	void OnTriggerEnter(Collider other) {
		if(!mStopActive) {
			mStopActive =
				((mCurListener != null && mCurListener.currentTarget.type == ActionTarget.Type.Follow)
					|| mNoActionFollow != null) && other.transform == flockUnit.moveTarget;
		}
	}
	
	void OnTriggerExit(Collider other) {
		if(mStopActive) {
			//go back to following
			ResumeFollow(other.transform, false);
		}
	}
	
	void OnActionEnter(ActionListener listener) {
		mStopActive = false;
		mCurStopDelay = 0.0f;
		
		mCurListener = listener;
		
		switch(mCurListener.currentTarget.type) {
		case ActionTarget.Type.Disperse:
			break;
			
		case ActionTarget.Type.Follow:
			flockUnit.restrictMove = false;
			flockUnit.moveTarget = mCurListener.currentTarget.target;
			
			mLastFollowPos = flockUnit.moveTarget.position;
			break;
			
		default:
			flockUnit.restrictMove = false;
			flockUnit.moveTarget = mCurListener.currentTarget.target;
			break;
		}
	}
	
	void OnActionExit(ActionListener listener) {
	}
	
	void OnActionFinish(ActionListener listener) {
		//do something
		mStopActive = false;
		mCurStopDelay = 0.0f;
		
		//default follow
		if(mNoActionFollow != null) {
			flockUnit.restrictMove = false;
			flockUnit.moveTarget = mNoActionFollow;
			mLastFollowPos = mNoActionFollow.position;
		}
		else { //just stop
			flockUnit.restrictMove = true;
		}
		
		mCurListener = null;
	}
}
