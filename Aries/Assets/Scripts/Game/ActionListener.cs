using UnityEngine;
using System.Collections;

//attach with a rigid body, broadcasts action
public class ActionListener : MonoBehaviour {
	public const string FuncActionEnter = "OnActionEnter"; //void OnActionEnter(ActionTarget target)
	public const string FuncActionExit = "OnActionExit"; //void OnActionExit(ActionTarget target)
	
	private ActionTarget mCurActionTarget = null;
	
	public void StopAction() {
		if(mCurActionTarget != null) {
			mCurActionTarget.RemoveListener(this);
			
			BroadcastMessage(FuncActionExit, mCurActionTarget, SendMessageOptions.DontRequireReceiver);
									
			mCurActionTarget = null;
		}
	}
	
	void OnTriggerEnter(Collider other) {
		ActionTarget target = other.GetComponent<ActionTarget>();
		if(target != null && target.vacancy) {
			bool doIt = true;
			
			//check if we currently have a target, then determine priority
			if(mCurActionTarget != null) {
				if(mCurActionTarget.action.priority <= target.action.priority) {
					StopAction();
				}
				else {
					doIt = false;
				}
			}
			
			if(doIt) {
				target.AddListener(this);
				
				mCurActionTarget = target;
				
				BroadcastMessage(FuncActionEnter, target, SendMessageOptions.DontRequireReceiver);
			}
		}
	}
	
	void OnTriggerExit(Collider other) {
		ActionTarget target = other.GetComponent<ActionTarget>();
		if(target != null && target == mCurActionTarget) {
			StopAction();
		}
	}
}
