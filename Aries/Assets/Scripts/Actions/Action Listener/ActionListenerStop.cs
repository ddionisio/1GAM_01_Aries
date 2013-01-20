using HutongGames.PlayMaker;

namespace Game.Actions {
	[ActionCategory("Game")]
	[Tooltip("Stop current action of ActionListener (setting priority to highest essentially clears it, use this to cancel entity's current activity")]
	public class ActionListenerStop : FsmStateAction {
		
		public ActionTarget.Priority priority = ActionTarget.Priority.Highest;
		public bool resumeDefault = true;
		
		[Tooltip("If we are able to stop the action, then go to this state")]
		public FsmEvent stopSucceed = FsmEvent.Finished;
		
		[Tooltip("If we fail to stop the action, then go to this state. Usually this is when our priority is lower.")]
		public FsmEvent stopFailed = FsmEvent.Finished;
		
		public bool everyFrame = false;
		
		private ActionListener listener;
		
		public override void Init (FsmState state)
		{
			base.Init (state);
			
			if(listener == null)
				listener = state.Fsm.Owner.GetComponentInChildren<ActionListener>();
		}
		
		public override void Reset ()
		{
			priority = ActionTarget.Priority.Highest;
			resumeDefault = true;
			everyFrame = false;
			stopSucceed = FsmEvent.Finished;
			stopFailed = FsmEvent.Finished;
		}
		
		// Code that runs on entering the state.
		public override void OnEnter()
		{
			//listener.StopAction
			DoStop();
			
			if(!everyFrame) {
				Finish();
			}
		}
		
		public override void OnLateUpdate() {
			DoStop();
		}
		
		void DoStop() {
			if(listener.StopAction(priority, resumeDefault)) {
				Fsm.Event(stopSucceed);
			}
			else {
				Fsm.Event(stopFailed);
			}
		}
	}
}