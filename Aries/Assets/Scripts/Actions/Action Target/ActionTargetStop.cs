using HutongGames.PlayMaker;

namespace Game.Actions {
	[ActionCategory("Game")]
	[Tooltip("Stops the action, this will clear out any listeners currently set on the target")]
	public class ActionTargetStop : FsmStateAction {
		[RequiredField]
		[UIHint(UIHint.FsmGameObject)]
		public ActionTarget actionTarget;
		
		public override void Reset ()
		{
			actionTarget = null;
		}
		
		// Code that runs on entering the state.
		public override void OnEnter()
		{
			actionTarget.StopAction();
			
			Finish();
		}
	}
}
