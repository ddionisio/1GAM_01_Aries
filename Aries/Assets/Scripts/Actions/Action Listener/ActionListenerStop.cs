using HutongGames.PlayMaker;

namespace Game.Actions {
	[ActionCategory("Game")]
	[Tooltip("Stop current action of ActionListener (setting priority to highest essentially clears it, use this to cancel entity's current activity")]
	public class ActionListenerStop : FSMActionComponentBase<ActionListener> {
		
		public ActionTarget.Priority priority = ActionTarget.Priority.Highest;
		public bool resumeDefault = true;
		
		[Tooltip("If we are able to stop the action, then go to this state")]
		public FsmEvent stopSucceed;
		
		[Tooltip("If we fail to stop the action, then go to this state. Usually this is when our priority is lower.")]
		public FsmEvent stopFailed;
		
		public bool everyFrame = false;
		
		public override void Reset ()
		{
			base.Reset();
			
			priority = ActionTarget.Priority.Highest;
			resumeDefault = true;
			everyFrame = false;
			stopSucceed = FsmEvent.Finished;
			stopFailed = FsmEvent.Finished;
		}
		
		// Code that runs on entering the state.
		public override void OnEnter()
		{
			base.OnEnter();
			
			if(mComp != null) {
				DoStop();
				
				if(!everyFrame) {
					Finish();
				}
			}
		}
		
		public override void OnLateUpdate() {
			DoStop();
		}
		
		void DoStop() {
			if(mComp.StopAction(priority, resumeDefault)) {
				//check if we reverted to default, if there's no default, then call success
				//going back to default guarantees our FSM state will change via EntityActionEnter
				if(resumeDefault || mComp.currentTarget == null || mComp.currentTarget != mComp.defaultTarget)
					Fsm.Event(stopSucceed);
			}
			else {
				Fsm.Event(stopFailed);
			}
		}
	}
}
