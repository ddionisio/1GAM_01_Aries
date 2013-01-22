using HutongGames.PlayMaker;

namespace Game.Actions {
	[ActionCategory("Game")]
	[Tooltip("Stops the action, this will clear out any listeners currently set on the target")]
	public class ActionTargetStop : FSMActionComponentBase<ActionTarget> {
		
		// Code that runs on entering the state.
		public override void OnEnter()
		{
			base.OnEnter();
			
			ActionTarget t = mComp;
			if(t != null)
				t.StopAction();
			
			Finish();
		}
	}
}
