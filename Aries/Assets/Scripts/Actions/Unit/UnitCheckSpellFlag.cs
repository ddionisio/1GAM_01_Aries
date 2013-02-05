using UnityEngine;
using HutongGames.PlayMaker;

namespace Game.Actions {
	[ActionCategory("Game")]
	[Tooltip("Check if unit is under a spell of specific condition.")]
	public class UnitCheckSpellFlag : FSMActionComponentBase<UnitEntity>
	{
		public SpellFlag flag;
		
		public FsmEvent isTrue;
		public FsmEvent isFalse;
		
		public bool everyFrame;
				
		public override void Reset() {
			base.Reset();
			
			flag = (SpellFlag)0;
			
			isTrue = null;
			isFalse = null;
			
			everyFrame = false;
		}
		
		// Code that runs on entering the state.
		public override void OnEnter()
		{
			base.OnEnter();
			
			if(mComp != null) {
				if(!everyFrame) {
					Fsm.Event(mComp.SpellCheckFlags(flag) ? isTrue : isFalse);
					Finish();
				}
			}
			else {
				Finish();
			}
		}
		
		public override void OnUpdate ()
		{
			if(mComp != null) {
				Fsm.Event(mComp.SpellCheckFlags(flag) ? isTrue : isFalse);
			}
			else {
				Finish();
			}
		}
		
		public override string ErrorCheck()
		{
			if (everyFrame &&
				FsmEvent.IsNullOrEmpty(isTrue) &&
				FsmEvent.IsNullOrEmpty(isFalse))
				return "Action sends no events!";
			return "";
		}
	}
}