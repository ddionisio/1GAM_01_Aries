using UnityEngine;
using System.Collections;

//attach with a rigid body
public class ActionListener : MonoBehaviour {
	private ActionTarget mCurActionTarget = null;
	private ActionTarget mDefaultActionTarget = null;
	private bool mLockAction = false;
	
	///<summary> lock action, preventing action being set </summary>
	public bool lockAction {
		get { return mLockAction; }
		
		set {
			mLockAction = value;
		}
	}
	
	public ActionTarget.Priority currentPriority {
		get { return mCurActionTarget != null ? mCurActionTarget.priority : ActionTarget.Priority.Low; }
	}
	
	//use this to manually set target (e.g. attacking with specific units)
	public ActionTarget currentTarget {
		get { return mCurActionTarget; }
		
		set {
			if(!mLockAction) {
				SetTarget(value);
			}
		}
	}
	
	//default target is when current stops, the default starts again and becomes current
	//also when current is null and you set default, it starts the action
	public ActionTarget defaultTarget {
		get { return mDefaultActionTarget; }
		
		set {
			if(value != null) {
				if(mDefaultActionTarget != value) {
					//check if there's already a current action
					//if there's a previous default, then clean that up
					//otherwise, set this to also be current
					if(mCurActionTarget != null) {
						if(mDefaultActionTarget != null) {
							mDefaultActionTarget.RemoveListener(this);
						}
					}
					else {
						ApplyToCurTarget(value);
					}
					
					mDefaultActionTarget = value;
				}
			}
			else {
				//clear out default, if it is also current, stop it as well
				if(mDefaultActionTarget != null) {
					if(mDefaultActionTarget == mCurActionTarget) {
						StopAction(ActionTarget.Priority.Highest, false);
					}
					else {
						mDefaultActionTarget.RemoveListener(this);
					}
					
					mDefaultActionTarget = null;
				}
			}
		}
	}
			
	//true if stopped or there was no action target
	public bool StopAction(ActionTarget.Priority priority, bool resumeDefault) {
		if(mCurActionTarget == null) {
			return true;
		}
		else if(mCurActionTarget.priority <= priority) {
			mCurActionTarget.RemoveListener(this);
			
			OnActionFinish();
			
			mCurActionTarget = null;
			
			if(resumeDefault) {
				ApplyToCurTarget(mDefaultActionTarget);
			}
			
			return true;
		}
		
		return false;
	}
	
	protected virtual void OnActionEnter() {
	}
	
	protected virtual void OnActionExit() {
	}
	
	protected virtual void OnActionFinish() {
	}
	
	void OnDestroy() {
		defaultTarget = null;
		currentTarget = null;
	}
	
	void OnTriggerEnter(Collider other) {
		if(!mLockAction) {
			SetTarget(other.GetComponent<ActionTarget>());
		}
	}
	
	void OnTriggerExit(Collider other) {
		ActionTarget target = other.GetComponent<ActionTarget>();
		if(target != null && target == mCurActionTarget) {
			if(target.stopOnExit) {
				StopAction(ActionTarget.Priority.Highest, true);
			}
			else {
				OnActionExit();
			}
		}
	}
	
	private void SetTarget(ActionTarget target) {
		if(target != null) {
			if(target != mCurActionTarget && target.vacancy) {
				//check if we currently have a target, then determine priority
				if(StopAction(target.priority, false)) {
					ApplyToCurTarget(target);
				}
			}
		}
		else {
			StopAction(ActionTarget.Priority.Highest, true);
		}
	}
	
	private void ApplyToCurTarget(ActionTarget target) {
		target.AddListener(this);
		
		mCurActionTarget = target;
		
		OnActionEnter();
	}
}
