using UnityEngine;
using System.Collections;
using HutongGames.PlayMaker;

namespace Game.Actions {
	[ActionCategory("Game")]
	[Tooltip("Check if unit can attack target at current distance.")]
	public class UnitCheckRangeAttack : FSMActionComponentBase<UnitEntity> {
		public FsmEvent isTrue;
		public FsmEvent isFalse;
		
		public bool everyFrame;
		
		public override void Reset ()
		{
			base.Reset ();
			
			isTrue = null;
			isFalse = null;
			everyFrame = false;
		}
		
		public override void OnEnter ()
		{
			base.OnEnter ();
			
			if(mComp != null) {
				DoCheck();
				
				if(!everyFrame)
					Finish();
			}
			else {
				Finish();
			}
		}
		
		public override void OnLateUpdate ()
		{
			DoCheck();
		}
		
		void DoCheck() {
			if(mComp.CheckRangeAttack())
				Fsm.Event(isTrue);
			else
				Fsm.Event(isFalse);
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