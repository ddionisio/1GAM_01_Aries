using HutongGames.PlayMaker;

namespace Game.Actions {
	[ActionCategory("Game")]
	[Tooltip("Stops the action, this will clear out any listeners currently set on the target")]
	public class ActionTargetCheckType : FSMActionComponentBase<ActionTarget> {
		
		public ActionType type;
		
		public FsmEvent isTrue;
		public FsmEvent isFalse;
		
		public override void Reset ()
		{
			base.Reset ();
			
			type = ActionType.Follow;
			isTrue = null;
			isFalse = null;
		}
		
		// Code that runs on entering the state.
		public override void OnEnter()
		{
			base.OnEnter();
			
			if(mComp != null) {
				if(mComp.type == type)
					Fsm.Event(isTrue);
				else
					Fsm.Event(isFalse);
			}
			
			Finish();
		}
	}
}
