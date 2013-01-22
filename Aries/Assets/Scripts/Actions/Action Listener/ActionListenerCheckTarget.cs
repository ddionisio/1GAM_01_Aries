using UnityEngine;
using HutongGames.PlayMaker;

namespace Game.Actions {
	[ActionCategory("Game")]
	[Tooltip("Check listener's target to see if it is available.")]
	public class ActionListenerCheckTarget : FSMActionComponentBase<ActionListener> {
		public FsmEvent isTrue;
		public FsmEvent isFalse;
		
		[Tooltip("Optional: If target is set to its assigned default.")]
		public FsmEvent isDefault;
		
		public bool everyFrame;
		
		public override void Reset()
		{
			base.Reset();
			
			isTrue = null;
			isFalse = null;
			isDefault = null;
			everyFrame = false;
		}
		
		public override void OnEnter()
		{
			base.OnEnter();
			
			DoCheck();
			if(!everyFrame)
				Finish();
		}
		
		public override void OnUpdate ()
		{
			DoCheck();
		}
		
		void DoCheck() {
			ActionListener l = mComp;
			if(l != null && l.currentTarget != null) {
				if(!FsmEvent.IsNullOrEmpty(isDefault) && l.currentTarget == l.defaultTarget) {
					Fsm.Event(isDefault);
				}
				else {
					Fsm.Event(isTrue);
				}
			}
			else {
				Fsm.Event(isFalse);
			}
		}
		
		public override string ErrorCheck()
		{
			if (everyFrame &&
				FsmEvent.IsNullOrEmpty(isDefault) &&
				FsmEvent.IsNullOrEmpty(isTrue) &&
				FsmEvent.IsNullOrEmpty(isFalse))
				return "Action sends no events!";
			return "";
		}
	}
}
