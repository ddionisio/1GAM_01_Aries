using HutongGames.PlayMaker;

namespace Game.Actions {
	[ActionCategory("Game")]
	[Tooltip("Change unit sprite controller's state")]
	public class SpriteUnitCtrlSetState : FsmStateAction {
		[RequiredField]
		[Tooltip("The unit sprite controller to set state")]
		[UIHint(UIHint.FsmGameObject)]
		public UnitSpriteController controller;
		
		[Tooltip("The state to change to")]
		public UnitSpriteState state = UnitSpriteState.Move;
		
		public override void Reset ()
		{
			controller = null;
			state = UnitSpriteState.Move;
		}
		
		// Code that runs on entering the state.
		public override void OnEnter()
		{
			if(controller.HasState(state))
				controller.state = state;
			
			Finish();
		}
	}
}