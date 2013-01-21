using HutongGames.PlayMaker;

namespace Game.Actions {
	[ActionCategory("Game")]
	[Tooltip("Check unit sprite controller's state")]
	public class SpriteUnitCtrlCheckState : FSMActionComponentBase<UnitSpriteController> {
		[Tooltip("The state to change to")]
		public UnitSpriteState spriteState = UnitSpriteState.Move;
		
		public FsmEvent isTrue;
		public FsmEvent isFalse;
		
		public override void Reset ()
		{
			base.Reset();
			checkChildren = true;
			
			spriteState = UnitSpriteState.Move;
			isTrue = null;
			isFalse = null;
		}
		
		// Code that runs on entering the state.
		public override void OnEnter()
		{
			base.OnEnter();
			
			if(mComp != null) {
				if(mComp.state == spriteState)
					Fsm.Event(isTrue);
				else
					Fsm.Event(isFalse);
			}
			
			Finish();
		}
	}
}