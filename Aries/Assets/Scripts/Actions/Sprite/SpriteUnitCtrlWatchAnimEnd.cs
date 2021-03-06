using HutongGames.PlayMaker;

namespace Game.Actions {
	[ActionCategory("Game")]
	[Tooltip("Watch for specific state to end.")]
	public class SpriteUnitCtrlWatchAnimEnd : FSMActionComponentBase<UnitSpriteController> {
		public FsmEvent toEvent;
		
		[Tooltip("The state to watch for")]
		public UnitSpriteState spriteState = UnitSpriteState.Move;
		
		public override void Reset ()
		{
			base.Reset();
			
			checkChildren = true;
			toEvent = null;
			spriteState = UnitSpriteState.Move;
		}
		
		// Code that runs on entering the state.
		public override void OnEnter()
		{
			base.OnEnter();
			
			if(mComp != null && mComp.HasState(spriteState)) {
				mComp.stateFinishCallback += OnStateAnimComplete;
			}
		}
		
		public override void OnExit ()
		{
			base.OnExit ();
			
			if(mComp != null) {
				mComp.stateFinishCallback -= OnStateAnimComplete;
			}
		}
		
		void OnStateAnimComplete(UnitSpriteState aState, UnitSpriteController.Dir dir) {
			if(aState == spriteState) {
				//Fsm.EventData.IntData = frame.eventInt;
				Fsm.Event(toEvent);
			}
		}
	}
}