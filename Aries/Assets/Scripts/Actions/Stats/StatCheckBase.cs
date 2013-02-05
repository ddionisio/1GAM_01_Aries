using UnityEngine;
using System.Collections;
using HutongGames.PlayMaker;

namespace Game.Actions {
	public abstract class StatCheckBase<T> : FSMActionComponentBase<T> where T : StatBase
	{
		[RequiredField]
		public FsmFloat val;
		
		[RequiredField]
		public FsmFloat tolerance;
		
		public FsmEvent equal;
		public FsmEvent lessThan;
		public FsmEvent greaterThan;
		
		public bool everyFrame;
		
		public override void Reset()
		{
			base.Reset();
			
			val = 0.0f;
			tolerance = 0.0f;
			equal = null;
			lessThan = null;
			greaterThan = null;
			everyFrame = false;
		}
		
		public override void OnEnter()
		{
			base.OnEnter();
			
			DoCompare();
				
			if (!everyFrame)
				Finish();
		}
		
		public override void OnLateUpdate()
		{
			DoCompare();
		}
		
		protected abstract float GetStat(T s);
		
		void DoCompare()
		{
			if(mComp != null) {
				float stat = GetStat(mComp);
				
				if (Mathf.Abs(stat - val.Value) <= tolerance.Value)
				{
					Fsm.Event(equal);
					return;
				}
		
				if (stat < val.Value)
				{
					Fsm.Event(lessThan);
					return;
				}
		
				if (stat > val.Value)
				{
					Fsm.Event(greaterThan);
				}
			}
			else {
				Finish();
			}
		}
	
		public override string ErrorCheck()
		{
			if (FsmEvent.IsNullOrEmpty(equal) &&
				FsmEvent.IsNullOrEmpty(lessThan) &&
				FsmEvent.IsNullOrEmpty(greaterThan))
				return "Action sends no events!";
			return "";
		}
	}
}