using UnityEngine;
using System.Collections;
using HutongGames.PlayMaker;

namespace Game.Actions {
	[ActionCategory("Game")]
	[Tooltip("Check to see if we can hurt given target.")]
	public class StatCanDamage : FSMActionComponentBase<StatBase>
	{
		[RequiredField]
		[UIHint(UIHint.FsmGameObject)]
		public FsmGameObject target;
		
		public FsmEvent isTrue;
		public FsmEvent isFalse;
		
		public override void Reset()
		{
			base.Reset();
			
			target = null;
			isTrue = null;
			isFalse = null;
		}
		
		public override void OnEnter()
		{
			base.OnEnter();
			
			if(target.Value != null) {
				StatBase targetStat = target.Value.GetComponent<StatBase>();
				if(targetStat != null && mComp.CanDamage(targetStat)) {
					Fsm.Event(isTrue);
				}
				else {
					Fsm.Event(isFalse);
				}
			}
			else {
				Fsm.Event(isFalse);
			}
			
			Finish();
		}
	}
}