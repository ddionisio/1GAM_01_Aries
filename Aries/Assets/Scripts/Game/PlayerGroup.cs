using UnityEngine;
using System.Collections;

public class PlayerGroup : FlockGroup {
	//assume player is in scene and this is set correctly
	public PlayerController playerController;
	
	protected override void OnAddUnit(FlockUnit unit) {
		//setup default sheep follow target
		FlockActionController actionControl = unit.GetComponent<FlockActionController>();
		if(actionControl != null) {
			actionControl.noActionFollow = playerController.transform;
			
			//if for some reason there's an action listener, then just stop it
			//follow player right away
			if(actionControl.curListener != null) {
				actionControl.curListener.StopAction(ActionTarget.Priority.High);
			}
			else {
				unit.restrictMove = false;
				unit.moveTarget = playerController.transform;
			}
		}
	}
}
