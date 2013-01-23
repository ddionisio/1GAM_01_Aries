using UnityEngine;
using HutongGames.PlayMaker;

namespace Game.Actions {
	[ActionCategory("Game")]
	[Tooltip("Reset waypoint to beginning.")]
	public class WaypointReset : FsmStateAction
	{
		[RequiredField]
		[UIHint(UIHint.Variable)]
		[Tooltip("The waypoint data.")]
		public FsmObject wp;
				
		public override void Reset() {
			base.Reset();
			
			wp = null;
		}
		
		// Code that runs on entering the state.
		public override void OnEnter()
		{
			base.OnEnter();
									
			if(wp.Value != null) {
				WaypointData wpData = (WaypointData)wp.Value;
				wpData.curInd = 0;
			}
			
			Finish();
		}
	}
}