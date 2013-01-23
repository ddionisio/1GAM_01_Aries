using HutongGames.PlayMaker;

namespace Game.Actions {
	[ActionCategory("Game")]
	[Tooltip("Stops the action, this will clear out any listeners currently set on the target")]
	public class ActionTargetCheckVacancy : FSMActionComponentBase<ActionTarget> {
		public FsmEvent isTrue;
		
		[Tooltip("This means it is full.")]
		public FsmEvent isFalse;
		
		public bool everyFrame;
		
		public override void Reset ()
		{
			base.Reset ();
			
			isTrue = null;
			isFalse = null;
			
			everyFrame = false;
		}
		
		// Code that runs on entering the state.
		public override void OnEnter()
		{
			base.OnEnter();
									
			DoCheck();
			
			if(!everyFrame)
				Finish();
		}
		
		public override void OnUpdate ()
		{
			DoCheck();
		}
		
		public void DoCheck() {
			ActionTarget t = mComp;
			if(t != null) {
				if(t.vacancy)
					Fsm.Event(isTrue);
				else
					Fsm.Event(isFalse);
			}
			else
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