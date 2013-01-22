using HutongGames.PlayMaker;

namespace Game.Actions {
	[ActionCategory("Game")]
	[Tooltip("Watch for specific event based on given state.")]
	public class SpriteUnitCtrlWatchAnimEvent : FSMActionComponentBase<UnitSpriteController> {
		[UIHint(UIHint.Variable)]
		public FsmInt storeInt;
		
		[UIHint(UIHint.Variable)]
		public FsmString storeString;
		
		[UIHint(UIHint.Variable)]
		public FsmFloat storeFloat;
		
		public FsmEvent toEvent;
		
		[Tooltip("The state to watch for")]
		public UnitSpriteState spriteState = UnitSpriteState.Move;
		
		public override void Reset ()
		{
			base.Reset();
			
			storeInt = null;
			storeString = null;
			storeFloat = null;
			
			checkChildren = true;
			toEvent = null;
			spriteState = UnitSpriteState.Move;
		}
		
		// Code that runs on entering the state.
		public override void OnEnter()
		{
			base.OnEnter();
			
			UnitSpriteController c = mComp;
			if(c != null && c.HasState(spriteState)) {
				c.stateEventCallback += OnStateAnimEvent;
			}
		}
		
		public override void OnExit ()
		{
			base.OnExit ();
			
			UnitSpriteController c = mComp;
			if(c != null) {
				c.stateEventCallback -= OnStateAnimEvent;
			}
		}
		
		void OnStateAnimEvent(UnitSpriteState aState, UnitSpriteController.Dir dir, UnitSpriteController.EventData data) {
			if(aState == spriteState) {
				storeInt.Value = data.valI;
				storeString.Value = data.valS;
				storeFloat.Value = data.valF;
				
				Fsm.Event(toEvent);
			}
		}
	}
}