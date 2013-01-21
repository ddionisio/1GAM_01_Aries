using UnityEngine;
using System.Collections;

//collider is used for following, assume it's trigger
public class FlockActionController : ActionListener {
	
	public FlockUnit flockUnit;
	
	public float followStopDelay = 1.0f;
	public float followStopRadius = 2.0f;
	public float followStopSpeed = 0.01f;
	
	private float mCurStopDelay = 0.0f;
	
	private MotionBase mTargetMotion = null;
	
	protected override void OnActionEnter() {
		mCurStopDelay = 0.0f;
		mTargetMotion = currentTarget.GetComponent<MotionBase>();
		
		switch(currentTarget.type) {
		case ActionType.Disperse:
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
		mCurStopDelay = 0.0f;
		
		flockUnit.moveTarget = null;
		
		mTargetMotion = null;
	}
	
	void Update() {
		if(currentTarget != null && mTargetMotion != null) {
			mCurStopDelay += Time.deltaTime;
			if(mCurStopDelay >= followStopDelay) {
				mCurStopDelay = 0.0f;
				
				if(mTargetMotion.curSpeed < followStopSpeed) {
					if(flockUnit.moveTarget != null && flockUnit.moveTargetDistance <= followStopRadius)
						flockUnit.moveTarget = null;
				}
				else {
					flockUnit.moveTarget = currentTarget.target;
				}
			}
		}
	}
}
