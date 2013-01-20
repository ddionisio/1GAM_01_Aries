using HutongGames.PlayMaker;

namespace Game.Actions {
	public class ActionListenerCheckTarget : FsmStateAction {
		[RequiredField]
		[Tooltip("Check ActionListener's current target state")]
		[UIHint(UIHint.FsmGameObject)]
		public ActionListener listener;
		
		[Tooltip("Check to see if there is no current target, then set event")]
		public FsmEvent noTarget;
		
		[Tooltip("Check to see if current target is set to default, then set event")]
		public FsmEvent targetIsDefault;
		
		public class Info {
			public ActionType type = ActionType.Follow;
			public FsmEvent toEvent;
		}
		
		[Tooltip("Types to check if there is a current target")]
		public Info[] actionChecks;
		
		public bool everyFrame;
		
		public override void Reset()
		{
			listener = null;
			noTarget = null;
			targetIsDefault = null;
			actionChecks = null;
			everyFrame = false;
		}
		
		public override void OnEnter()
		{
			DoCompare();
			
			if (!everyFrame)
				Finish();
		}
		
		public override void OnLateUpdate()
		{
			DoCompare();
		}
		
		void DoCompare()
		{
			ActionTarget target = listener.currentTarget;
			
			if(target == null) {
				Fsm.Event(noTarget);
				return;
			}
			
			if(target == listener.defaultTarget) {
				Fsm.Event(targetIsDefault);
				return;
			}
			
			if(actionChecks != null) {
				foreach(Info actionCheck in actionChecks) {
					if(target.type == actionCheck.type) {
						Fsm.Event(actionCheck.toEvent);
						break;
					}
				}
			}
		}
		
		public bool ValidateActionChecks() {
			foreach(Info actionCheck in actionChecks) {
				if(!FsmEvent.IsNullOrEmpty(actionCheck.toEvent)) {
					return true;
				}
			}
			
			return false;
		}
	
		public override string ErrorCheck()
		{
			if (FsmEvent.IsNullOrEmpty(noTarget) &&
				(actionChecks == null ||
				actionChecks.Length == 0 ||
				!ValidateActionChecks()))
				return "Action sends no events!";
			return "";
		}
	}
}
