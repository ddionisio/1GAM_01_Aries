using UnityEngine;
using HutongGames.PlayMaker;

namespace Game.Actions {
	[ActionCategory("Game")]
	[Tooltip("Jump to target.")]
	public class JumpToTarget : FSMActionComponentBase<Jump>
	{
		public FsmEvent finish;
		
		public override void Reset() {
			base.Reset();
			
			finish = null;
		}
		
		// Code that runs on entering the state.
		public override void OnEnter()
		{
			base.OnEnter();
			
			Jump m = mComp;
			if(m != null) {
				m.jumpFinishCallback += JumpFinish;
				
				//we are already jumping, wait for it to finish
				if(!m.isJumping) {
					if(!m.JumpToTarget()) {
						//failed to jump for some reason?
						JumpFinish();
					}
				}
			}
			else {
				JumpFinish();
			}
		}
		
		public override void OnExit ()
		{
			base.OnExit ();
			
			Jump m = mComp;
			if(m != null) {
				m.jumpFinishCallback -= JumpFinish;
			}
		}
		
		void JumpFinish() {
			Fsm.Event(finish);
			Finish();
		}
	}
}