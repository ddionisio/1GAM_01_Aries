using HutongGames.PlayMaker;

namespace Game.Actions {
	[ActionCategory("Game")]
	[Tooltip("Stop current action of ActionListener (setting priority to highest essentially clears it, use this to cancel entity's current activity")]
	public class ActionListenerStop : FSMActionComponentBase<ActionListener> {
		
		public ActionTarget.Priority priority = ActionTarget.Priority.Highest;
		public bool resumeDefault = true;
		
		public override void Reset ()
		{
			base.Reset();
			
			priority = ActionTarget.Priority.Highest;
			resumeDefault = true;
		}
		
		// Code that runs on entering the state.
		public override void OnEnter()
		{
			base.OnEnter();
			
			DoStop();
			Finish();
		}
		
		void DoStop() {
			if(mComp != null)
				mComp.StopAction(priority, resumeDefault);
		}
	}
}
