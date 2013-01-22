using UnityEngine;
using System.Collections;
using HutongGames.PlayMaker;

namespace Game.Actions {
	[ActionCategory("Game")]
	[Tooltip("Subtract health by given value.")]
	public class StatSubtractHealth : FSMActionComponentBase<StatBase>
	{
		public FsmFloat val;
		
		public override void Reset()
		{
			base.Reset();
			
			val = 0.0f;
		}
		
		public override void OnEnter()
		{
			base.OnEnter();
			
			StatBase s = mComp;
			if(s != null) {
				s.curHP -= val.Value;
			}
			
			Finish();
		}
	}
}