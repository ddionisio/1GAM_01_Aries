using HutongGames.PlayMaker;

namespace Game.Actions {
	[ActionCategory("Game")]
	[Tooltip("Change unit sprite controller's state")]
	public class SpriteUnitCtrlSetState : FsmStateAction {
		[Tooltip("The state to change to")]
		public UnitSpriteState spriteState = UnitSpriteState.Move;
		
		private UnitSpriteController controller;
		
		public override void Init(FsmState aState)
		{
			base.Init(aState);
			
			if(controller == null)
				controller = aState.Fsm.Owner.GetComponentInChildren<UnitSpriteController>();
		}
				
		public override void Reset ()
		{
			spriteState = UnitSpriteState.Move;
		}
		
		// Code that runs on entering the state.
		public override void OnEnter()
		{
			if(controller.HasState(spriteState))
				controller.state = spriteState;
			
			Finish();
		}
	}
}