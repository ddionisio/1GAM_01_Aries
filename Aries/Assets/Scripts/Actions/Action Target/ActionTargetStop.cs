using HutongGames.PlayMaker;

namespace Game.Actions {
	[ActionCategory("Game")]
	[Tooltip("Stops the action, this will clear out any listeners currently set on the target")]
	public class ActionTargetStop : FsmStateAction {
		private ActionTarget actionTarget;
		
		public override void Init (FsmState state)
		{
			base.Init (state);
			
			if(actionTarget == null)
				actionTarget = state.Fsm.Owner.GetComponentInChildren<ActionTarget>();
		}
		
		// Code that runs on entering the state.
		public override void OnEnter()
		{
			actionTarget.StopAction();
			
			Finish();
		}
	}
}
