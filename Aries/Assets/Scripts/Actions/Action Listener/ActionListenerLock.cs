using HutongGames.PlayMaker;

namespace Game.Actions {
	[ActionCategory("Game")]
	[Tooltip("Stop current action of ActionListener (setting priority to highest essentially clears it, use this to cancel entity's current activity")]
	public class ActionListenerLock : FsmStateAction {
		[RequiredField]
		[UIHint(UIHint.FsmGameObject)]
		public ActionListener listener;
		
		public FsmBool val;
		
		public override void Reset ()
		{
			listener = null;
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
