using UnityEngine;
using HutongGames.PlayMaker;

namespace Game.Actions {
	[ActionCategory("Game")]
	[Tooltip("Get a waypoint from the waypoint manager and stuff it in an FsmObject. Do this sparingly, ie. on start or cache the ones you need.")]
	public class WaypointGetByName : FsmStateAction
	{
		[RequiredField]
		public FsmString waypoint;
		
		[RequiredField]
		[UIHint(UIHint.Variable)]
		[Tooltip("Store the object WaypointData. You can check afterwards for null if it exists.")]
		public FsmObject toObject;
		
		public override void Reset() {
			base.Reset();
			
			waypoint = null;
			toObject = null;
		}
		
		// Code that runs on entering the state.
		public override void OnEnter()
		{
			base.OnEnter();
			
			WaypointData dat = WaypointManager.instance.CreateWaypointData(waypoint.Value);
			
			if(dat == null)
				LogWarning("Waypoint: "+waypoint+" was not found!");
			
			toObject.Value = (Object)dat;
			
			Finish();
		}
	}
}