using UnityEngine;
using System.Collections;

//attach with a rigid body, broadcasts action
public class ActionListener : MonoBehaviour {
	public const string FuncActionEnter = "OnActionEnter"; //void OnActionEnter(ActionTarget target)
	public const string FuncActionExit = "OnActionExit"; //void OnActionExit(ActionTarget target)
	public const string FuncActionFinish = "OnActionFinish"; //void OnActionFinish(ActionTarget target)
	
	private ActionTarget mCurActionTarget = null;
	
	public ActionTarget currentTarget {
		get { return mCurActionTarget; }
		
		set {
			if(value != null) {
				SetTarget(value);
			}
			else {
				StopAction(ActionTarget.Priority.Highest);
			}
		}
	}
	
	//true if stopped or there was no action target
	public bool StopAction(ActionTarget.Priority priority) {
		if(mCurActionTarget == null) {
			return true;
		}
		else if(mCurActionTarget.priority <= priority) {
			mCurActionTarget.RemoveListener(this);
			
			BroadcastMessage(FuncActionFinish, this, SendMessageOptions.DontRequireReceiver);
									
			mCurActionTarget = null;
			
			return true;
		}
		
		return false;
	}
	
	void OnDestroy() {
		currentTarget = null;
	}
	
	void OnTriggerEnter(Collider other) {
		SetTarget(other.GetComponent<ActionTarget>());
	}
	
	void OnTriggerExit(Collider other) {
		ActionTarget target = other.GetComponent<ActionTarget>();
		if(target != null && target == mCurActionTarget) {
			if(target.stopOnExit) {
				StopAction(ActionTarget.Priority.Highest);
			}
			else {
				BroadcastMessage(FuncActionExit, this, SendMessageOptions.DontRequireReceiver);
			}
		}
	}
	
	private void SetTarget(ActionTarget target) {
		if(target != null && target.vacancy) {			
			//check if we currently have a target, then determine priority
			if(StopAction(target.priority)) {
				target.AddListener(this);
				
				mCurActionTarget = target;
				
				BroadcastMessage(FuncActionEnter, this, SendMessageOptions.DontRequireReceiver);
			}
		}
	}
}
