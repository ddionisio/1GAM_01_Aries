using UnityEngine;
using System.Collections;
using HutongGames.PlayMaker;

namespace Game.Actions {
	[ActionCategory("Game")]
	[Tooltip("Cause damage to given target.")]
	public class StatDoDamage : FSMActionComponentBase<StatBase>
	{
		[RequiredField]
		[UIHint(UIHint.FsmGameObject)]
		public FsmGameObject target;
				
		public override void Reset()
		{
			base.Reset();
			
			target = null;
		}
		
		public override void OnEnter()
		{
			base.OnEnter();
			
			if(mComp != null && target.Value != null) {
				StatBase targetStat = target.Value.GetComponent<StatBase>();
				if(targetStat != null) {
                    targetStat.DamageBy(mComp);
				}
			}
			
			Finish();
		}
	}
}