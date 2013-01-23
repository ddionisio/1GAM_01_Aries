using UnityEngine;
using HutongGames.PlayMaker;

namespace Game.Actions {
	[ActionCategory("Game")]
	[Tooltip("Move waypoint to the next.")]
	public class WaypointNext : FsmStateAction
	{
		[RequiredField]
		[UIHint(UIHint.Variable)]
		[Tooltip("The waypoint data.")]
		public FsmObject wp;
		
		public FsmEvent isDone;
		
		public bool loop = false;
		
		public override void Reset() {
			base.Reset();
			
			wp = null;
			isDone = null;
			loop = false;
		}
		
		// Code that runs on entering the state.
		public override void OnEnter()
		{
			base.OnEnter();
									
			if(wp.Value != null) {
				WaypointData wpData = (WaypointData)wp.Value;
				
				if(wpData.curInd >= wpData.waypoints.Count) {
					if(loop)
						wpData.curInd = 0;
					
					Fsm.Event(isDone);
				}
				else {
					wpData.curInd++;
					if(wpData.curInd == wpData.waypoints.Count) {
						if(loop)
							wpData.curInd = 0;
						
						Fsm.Event(isDone);
					}
				}
			}
			
			Finish();
		}
	}
}