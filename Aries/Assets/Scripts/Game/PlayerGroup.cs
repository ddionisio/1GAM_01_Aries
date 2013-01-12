using UnityEngine;
using System.Collections;

public class PlayerGroup : FlockGroup {
	//assume player is in scene and this is set correctly
	public PlayerController playerController;
	
	protected override void OnAddUnit(FlockUnit unit) {
		//setup default sheep follow target
		ActionListener actionListen = unit.GetComponent<ActionListener>();
		if(actionListen != null) {
			actionListen.defaultTarget = playerController.followAction;
		}
	}
}
