using UnityEngine;
using HutongGames.PlayMaker;

namespace Game.Actions {
	[ActionCategory("Game")]
	[Tooltip("Check listener's target to see if it is available.")]
	public class EnemyTargetNearest : FSMActionComponentBase<EnemyActionController> {
		[Tooltip("If we are able to set a target.")]
		public FsmEvent isSet;
		
		[Tooltip("If we didn't set a target.")]
		public FsmEvent isNotSet;
		
		public bool ignorePriority;
		public bool everyFrame;
		public float delay;
		
		private float mPrevTime;
		
		public override void Reset()
		{
			base.Reset();
			
			isSet = null;
			isNotSet = null;
			
			ignorePriority = false;
			everyFrame = false;
			delay = 0.0f;
		}
		
		public override void OnEnter()
		{
			base.OnEnter();
			
			if(mComp != null) {
				if(everyFrame) {
					mPrevTime = Time.time;
				}
				else {
					DoGetTarget();
					Finish();
				}
			}
			else {
				Finish();
			}
		}
		
		public override void OnLateUpdate ()
		{
			if(Time.time - mPrevTime >= delay) {
				mPrevTime = Time.time;
				DoGetTarget();
			}
		}
		
		void DoGetTarget() {
			if(mComp.SetTargetToNearest(ignorePriority))
				Fsm.Event(isSet);
			else
				Fsm.Event(isNotSet);
		}
		
		/*public override string ErrorCheck ()
		{
			if(everyFrame &&
				FsmEvent.IsNullOrEmpty(isSet) &&
				FsmEvent.IsNullOrEmpty(isNotSet)) {
				return "Action sends no events!";
			}
			
			return "";
		}*/
	}
}
