using HutongGames.PlayMaker;

namespace Game.Actions {
	[ActionCategory("Game")]
	[Tooltip("Stop current action of ActionListener (setting priority to highest essentially clears it, use this to cancel entity's current activity")]
	public class ActionListenerLock : FSMActionComponentBase<ActionListener> {
		
		public FsmBool val;
		
		public override void Reset ()
		{
			base.Reset();
			
			val = false;
		}
		
		// Code that runs on entering the state.
		public override void OnEnter()
		{
			base.OnEnter();
			
			ActionListener l = mComp;
			if(l != null)
				l.lockAction = val.Value;
			
			Finish();
		}
	}
}
