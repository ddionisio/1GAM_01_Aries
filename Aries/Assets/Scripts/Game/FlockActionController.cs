using UnityEngine;
using System.Collections;

public class FlockActionController : MonoBehaviour {
	private FlockUnit mFlockUnit;
	
	void Awake() {
		mFlockUnit = GetComponent<FlockUnit>();
	}
	
	void OnActionEnter(ActionTarget target) {
		switch(target.action.type) {
		case ActionInfo.Type.Disperse:
			break;
			
		default:
			mFlockUnit.moveTarget = target.target;
			break;
		}
	}
	
	void OnActionExit(ActionTarget target) {
		switch(target.action.type) {
		case ActionInfo.Type.Move:
			break;
			
		default:
			mFlockUnit.moveTarget = null;
			break;
		}
	}
}
