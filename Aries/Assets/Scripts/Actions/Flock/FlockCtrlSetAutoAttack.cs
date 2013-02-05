using UnityEngine;
using HutongGames.PlayMaker;

namespace Game.Actions {
	[ActionCategory("Game")]
	[Tooltip("Set the auto attack sensor on/off.")]
	public class FlockCtrlSetAutoAttack : FSMActionComponentBase<FlockActionController> {
		
		public FsmBool enable;
					
		public override void Reset()
		{
			base.Reset();
			
			enable = null;
		}
		
		public override void OnEnter()
		{
			base.OnEnter();
			
			if(mComp != null) {
				mComp.autoAttack = enable.Value;
			}
			
			Finish();
		}
	}
}
