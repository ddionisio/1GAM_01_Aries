using HutongGames.PlayMaker;

namespace Game.Actions {
	[ActionCategory("Game")]
	[Tooltip("Stop current action of ActionListener (setting priority to highest essentially clears it, use this to cancel entity's current activity")]
	public class ActionListenerLock : FsmStateAction {
		
		public FsmBool val;
		
		private ActionListener listener;
		
		public override void Init (FsmState state)
		{
			base.Init (state);
			
			if(listener == null)
				listener = state.Fsm.Owner.GetComponentInChildren<ActionListener>();
		}
		
		public override void Reset ()
		{
			val = false;
		}
		
		// Code that runs on entering the state.
		public override void OnEnter()
		{
			listener.lockAction = val.Value;
			
			Finish();
		}
	}
}
