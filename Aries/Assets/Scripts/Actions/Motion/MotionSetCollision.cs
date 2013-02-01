using UnityEngine;
using HutongGames.PlayMaker;

namespace Game.Actions {
	[ActionCategory("Game")]
	[Tooltip("Enable/disable collider.")]
	public class MotionSetCollision : FsmStateAction
	{
		[Tooltip("The Game Object to work with.")]
		public FsmOwnerDefault owner;
		
		public bool enable;
		
		public override void Reset() {
			base.Reset();
			
			enable = true;
		}
		
		// Code that runs on entering the state.
		public override void OnEnter()
		{
			base.OnEnter();
			
			GameObject go = Fsm.GetOwnerDefaultTarget(owner);
			
			if(go != null) {
				if(go.collider != null)
					go.collider.enabled = enable;
			}
			
			Finish();
		}
	}
}