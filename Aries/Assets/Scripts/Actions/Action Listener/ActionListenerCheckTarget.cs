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
		
		public override void Reset()
		{
			base.Reset();
			
			isTrue = null;
			isFalse = null;
			isDefault = null;
		}
		
		public override void OnEnter()
		{
			base.OnEnter();
			
			if(mComp != null) {
				if(mComp.currentTarget != null) {
					if(!FsmEvent.IsNullOrEmpty(isDefault) && mComp.currentTarget == mComp.defaultTarget) {
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
			
			Finish();
		}
	}
}
