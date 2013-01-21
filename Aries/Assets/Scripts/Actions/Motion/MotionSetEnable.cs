using UnityEngine;
using HutongGames.PlayMaker;

namespace Game.Actions {
	[ActionCategory("Game")]
	[Tooltip("Disable the motion component.")]
	public class MotionSetEnable : FSMActionComponentBase<MotionBase>
	{
		public FsmBool val;
		
		public override void Reset() {
			base.Reset();
			
			val = true;
		}
		
		// Code that runs on entering the state.
		public override void OnEnter()
		{
			base.OnEnter();
			
			if(mComp != null)
				mComp.enabled = val.Value;
			
			Finish();
		}
	}
}