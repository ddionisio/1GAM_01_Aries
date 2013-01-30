using UnityEngine;
using HutongGames.PlayMaker;

namespace Game.Actions {
	[ActionCategory("Game")]
	[Tooltip("Do some blinking. Make sure there are EntitySpriteControllers")]
	public class EntityBlink : FSMActionComponentBase<EntityBase>
	{
		public FsmEvent finish;
		public FsmFloat delay;
		
		public override void Reset ()
		{
			base.Reset ();
			
			finish = null;
			delay = 0.0f;
		}
		// Code that runs on entering the state.
		public override void OnEnter()
		{
			base.OnEnter();
			
			EntityBase e = mComp;
			if(e != null) {
				e.setBlinkCallback += OnBlinkFinish;
				
				e.Blink(delay.Value);
				
				if(FsmEvent.IsNullOrEmpty(finish))
					Finish();
			}
			else {
				Finish();
			}
		}
		
		public override void OnExit ()
		{
			base.OnExit ();
			
			EntityBase e = mComp;
			if(e != null) {
				e.setBlinkCallback -= OnBlinkFinish;
			}
		}
		
		void OnBlinkFinish(EntityBase ent, bool b) {
			if(!b)
				Fsm.Event(finish);
		}
	}
}