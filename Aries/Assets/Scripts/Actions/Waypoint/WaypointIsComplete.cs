using UnityEngine;
using HutongGames.PlayMaker;

namespace Game.Actions {
	[ActionCategory("Game")]
	[Tooltip("Check if waypoint is done. Use this after WaypointNext")]
	public class WaypointIsComplete : FsmStateAction
	{
		[RequiredField]
		[UIHint(UIHint.Variable)]
		[Tooltip("The waypoint data.")]
		public FsmObject wp;
		
		public FsmEvent isTrue;
		public FsmEvent isFalse;
				
		public override void Reset() {
			base.Reset();
			
			wp = null;
			isTrue = null;
			isFalse = null;
		}
		
		// Code that runs on entering the state.
		public override void OnEnter()
		{
			base.OnEnter();
									
			if(wp.Value != null) {
				WaypointData wpData = (WaypointData)wp.Value;
				
				if(wpData.curInd >= wpData.waypoints.Count)
					Fsm.Event(isTrue);
				else
					Fsm.Event(isFalse);
			}
			
			Finish();
		}
		
		public override string ErrorCheck()
		{
			if (FsmEvent.IsNullOrEmpty(isTrue) &&
				FsmEvent.IsNullOrEmpty(isFalse))
				return "Action sends no events!";
			return "";
		}
	}
}