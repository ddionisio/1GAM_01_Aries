using HutongGames.PlayMaker;

namespace Game.Actions {
	[ActionCategory("Game")]
	[Tooltip("Change unit sprite controller's state")]
	public class SpriteUnitCtrlSetState : FSMActionComponentBase<UnitSpriteController> {
		[Tooltip("The state to change to")]
		public UnitSpriteState spriteState = UnitSpriteState.Move;
		
		public override void Reset ()
		{
			base.Reset();
			
			checkChildren = true;
			spriteState = UnitSpriteState.Move;
		}
		
		// Code that runs on entering the state.
		public override void OnEnter()
		{
			base.OnEnter();
			
			UnitSpriteController c = mComp;
			if(c != null && c.HasState(spriteState))
				c.state = spriteState;
			
			Finish();
		}
	}
}