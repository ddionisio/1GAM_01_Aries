using UnityEngine;
using HutongGames.PlayMaker;

namespace Game.Actions {
	[ActionCategory("Game")]
	[Tooltip("Get a waypoint from given WaypointData and put it in a Vector2")]
	public class WaypointGetCurrent : FsmStateAction
	{
		[RequiredField]
		[UIHint(UIHint.Variable)]
		[Tooltip("The waypoint data.")]
		public FsmObject wp;
		
		[RequiredField]
		[UIHint(UIHint.Variable)]
		public FsmVector2 to;
		
		public override void Reset() {
			base.Reset();
			
			wp = null;
			to = null;
		}
		
		// Code that runs on entering the state.
		public override void OnEnter()
		{
			base.OnEnter();
									
			if(wp.Value != null) {
				WaypointData wpData = (WaypointData)wp.Value;
				to.Value = wpData.waypoints[wpData.curInd].position;
			}
			
			Finish();
		}
	}
}