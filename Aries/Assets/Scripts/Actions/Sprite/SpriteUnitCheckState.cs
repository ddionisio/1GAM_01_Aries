using HutongGames.PlayMaker;

namespace Game.Actions {
	[ActionCategory("Game")]
	[Tooltip("Check a variable to spriteState")]
	public class SpriteUnitCheckState : FsmStateAction {
		[RequiredField]
		[UIHint(UIHint.Variable)]
		[Tooltip("The variable to check with.")]
		public FsmInt val;
		
		[Tooltip("The state to check against.")]
		public UnitSpriteState spriteState = UnitSpriteState.Move;
		
		public FsmEvent isTrue;
		public FsmEvent isFalse;
		
		public override void Reset ()
		{
			val = 0;
			isTrue = null;
			isFalse = null;
		}
		
		// Code that runs on entering the state.
		public override void OnEnter()
		{
			if((UnitSpriteState)val.Value == spriteState)
				Fsm.Event(isTrue);
			else
				Fsm.Event(isFalse);
			
			Finish();
		}
	}
}