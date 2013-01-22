using UnityEngine;
using System.Collections;
using HutongGames.PlayMaker;

namespace Game.Actions {
	[ActionCategory("Game")]
	[Tooltip("Get the damage.")]
	public class StatGetDamage : FSMActionComponentBase<StatBase>
	{
		[RequiredField]
		[UIHint(UIHint.Variable)]
		public FsmFloat toVar;
		
		public override void Reset()
		{
			base.Reset();
			
			toVar = null;
		}
		
		public override void OnEnter()
		{
			base.OnEnter();
			
			StatBase s = mComp;
			if(s != null) {
				toVar.Value = s.damage;
			}
			
			Finish();
		}
	}
}