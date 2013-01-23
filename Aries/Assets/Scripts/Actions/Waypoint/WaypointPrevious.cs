using UnityEngine;
using HutongGames.PlayMaker;

namespace Game.Actions {
	[ActionCategory("Game")]
	[Tooltip("Move waypoint backwards.")]
	public class WaypointPrevious : FsmStateAction
	{
		[RequiredField]
		[UIHint(UIHint.Variable)]
		[Tooltip("The waypoint data.")]
		public FsmObject wp;
		
		public FsmEvent isFirst;
		
		public bool loop = false;
		
		public override void Reset() {
			base.Reset();
			
			wp = null;
			isFirst = null;
			loop = false;
		}
		
		// Code that runs on entering the state.
		public override void OnEnter()
		{
			base.OnEnter();
									
			if(wp.Value != null) {
				WaypointData wpData = (WaypointData)wp.Value;
				
				if(wpData.curInd <= 0) {
					if(loop)
						wpData.curInd = wpData.waypoints.Count-1;
					else {
						wpData.curInd = 0;
					}
					
					Fsm.Event(isFirst);
				}
				else {
					wpData.curInd--;
					if(wpData.curInd < 0) {
						if(loop)
							wpData.curInd = wpData.waypoints.Count-1;
						else
							wpData.curInd = 0;
						
						Fsm.Event(isFirst);
					}
				}
			}
			
			Finish();
		}
	}
}