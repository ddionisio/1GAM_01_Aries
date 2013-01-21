using HutongGames.PlayMaker;

namespace Game.Actions {
	[ActionCategory("Game")]
	[Tooltip("Check a variable to spriteState")]
	public class SpriteUnitCheckEvent : FsmStateAction {
		[RequiredField]
		[UIHint(UIHint.Variable)]
		[Tooltip("The variable to check with.")]
		public FsmInt val;
		
		[Tooltip("The state to check against.")]
		public UnitSpriteEvent spriteEvent = UnitSpriteEvent.None;
		
		public FsmEvent isTrue;
		public FsmEvent isFalse;
		
		public override void Reset ()
		{
			val = null;
			isTrue = null;
			isFalse = null;
		}
		
		// Code that runs on entering the state.
		public override void OnEnter()
		{
			if((UnitSpriteEvent)val.Value == spriteEvent)
				Fsm.Event(isTrue);
			else
				Fsm.Event(isFalse);
			
			Finish();
		}
	}
}