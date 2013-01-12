using UnityEngine;
using System.Collections;

//attach with a rigid body, broadcasts action
public class ActionListener : MonoBehaviour {
	public const string FuncActionEnter = "OnActionEnter"; //void OnActionEnter(ActionTarget target)
	public const string FuncActionExit = "OnActionExit"; //void OnActionExit(ActionTarget target)
	public const string FuncActionFinish = "OnActionFinish"; //void OnActionFinish(ActionTarget target)
	
	private ActionTarget mCurActionTarget = null;
	private ActionTarget mDefaultActionTarget = null;
	
	public ActionTarget currentTarget {
		get { return mCurActionTarget; }
		
		set {
			SetTarget(value);
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
			
			BroadcastMessage(FuncActionFinish, this, SendMessageOptions.DontRequireReceiver);
			
			mCurActionTarget = null;
			
			if(resumeDefault) {
				ApplyToCurTarget(mDefaultActionTarget);
			}
			
			return true;
		}
		
		return false;
	}
	
	void OnDestroy() {
		defaultTarget = null;
		currentTarget = null;
	}
	
	void OnTriggerEnter(Collider other) {
		SetTarget(other.GetComponent<ActionTarget>());
	}
	
	void OnTriggerExit(Collider other) {
		ActionTarget target = other.GetComponent<ActionTarget>();
		if(target != null && target == mCurActionTarget) {
			if(target.stopOnExit) {
				StopAction(ActionTarget.Priority.Highest, true);
			}
			else {
				BroadcastMessage(FuncActionExit, this, SendMessageOptions.DontRequireReceiver);
			}
		}
	}
	
	private void SetTarget(ActionTarget target) {
		if(target != null) {
			if(target.vacancy) {
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
		
		BroadcastMessage(FuncActionEnter, this, SendMessageOptions.DontRequireReceiver);
	}
}
