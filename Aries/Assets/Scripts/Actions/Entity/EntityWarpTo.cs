using UnityEngine;
using HutongGames.PlayMaker;

namespace Game.Actions {
	[ActionCategory("Game")]
	[Tooltip("Warp to given location (use EntityWarp).")]
	public class EntityWarpTo : FSMActionComponentBase<EntityWarp>
	{
		public FsmGameObject target;
		
		[Tooltip("If target is none, this is where to warp to.")]
		public FsmVector2 location;
		
		public FsmEvent success;
		public FsmEvent fail;
		
		
		public override void Reset ()
		{
			base.Reset ();
			
			target = null;
			location = null;
			success = null;
			fail = null;
		}
		
		// Code that runs on entering the state.
		public override void OnEnter()
		{
			base.OnEnter();
			
			if(mComp != null) {
				mComp.onFinishCallback += WarpFinish;
				
				if(target.Value != null) {
					mComp.WarpTo(target.Value.transform.position);
				}
				else {
					mComp.WarpTo(location.Value);
				}
			}
		}
		
		public override void OnExit ()
		{
			base.OnExit ();
			
			if(mComp != null) {
				mComp.onFinishCallback -= WarpFinish;
			}
		}
		
		void WarpFinish(bool aSuccess) {
			if(aSuccess)
				Fsm.Event(success);
			else
				Fsm.Event(fail);
			
			Finish();
		}
	}
}