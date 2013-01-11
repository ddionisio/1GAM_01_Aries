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
		StopAction(ActionTarget.Priority.Highest);
	}
	
	void OnTriggerEnter(Collider other) {
		ActionTarget target = other.GetComponent<ActionTarget>();
		if(target != null && target.vacancy) {			
			//check if we currently have a target, then determine priority
			if(StopAction(target.priority)) {
				target.AddListener(this);
				
				mCurActionTarget = target;
				
				BroadcastMessage(FuncActionEnter, this, SendMessageOptions.DontRequireReceiver);
			}
		}
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
}
