using UnityEngine;
using System.Collections;
using HutongGames.PlayMaker;

namespace Game.Actions {
	[ActionCategory("Game")]
	[Tooltip("Validate range based on current action of target.")]
	public class ActionListenerCheckRange : FSMActionComponentBase<ActionListener> {
		public FsmEvent isTrue;
		public FsmEvent isFalse;
		
		public bool everyFrame;
		public float delay;
		
		private float mLastTime;
		
		public override void Reset ()
		{
			base.Reset ();
			
			isTrue = null;
			isFalse = null;
			everyFrame = false;
			delay = 0.0f;
		}
		
		public override void OnEnter ()
		{
			base.OnEnter ();
			
			if(!everyFrame) {
				DoCheck();
				Finish();
			}
			else {
				mLastTime = Time.time;
			}
		}
		
		public override void OnLateUpdate ()
		{
			if(Time.time - mLastTime >= delay) {
				mLastTime = Time.time;
				DoCheck();
			}
		}
		
		void DoCheck() {
			ActionListener c = mComp;
			if(c != null && c.CheckRange()) {
				Fsm.Event(isTrue);
			}
			else {
				Fsm.Event(isFalse);
			}
		}
		
		public override string ErrorCheck()
		{
			if (FsmEvent.IsNullOrEmpty(isTrue) &&
				FsmEvent.IsNullOrEmpty(isFalse))
				return "Action sends no events!";
			return "";
		}
	}
}