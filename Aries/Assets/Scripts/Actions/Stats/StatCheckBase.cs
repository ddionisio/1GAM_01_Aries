using UnityEngine;
using System.Collections;
using HutongGames.PlayMaker;

namespace Game.Actions {
	public abstract class StatCheckBase<T> : FsmStateAction where T : StatBase
	{
		[RequiredField]
		public FsmFloat val;
		
		[RequiredField]
		public FsmFloat tolerance;
		
		public FsmEvent equal;
		public FsmEvent lessThan;
		public FsmEvent greaterThan;
		
		public bool everyFrame;
		
		protected T stats;
		
		public override void Init(FsmState aState)
		{
			base.Init(aState);
			
			if(stats == null)
				stats = aState.Fsm.Owner.GetComponentInChildren<T>();
		}
		
		public override void Reset()
		{
			val = 0.0f;
			tolerance = 0.0f;
			equal = null;
			lessThan = null;
			greaterThan = null;
			everyFrame = false;
		}
		
		public override void OnEnter()
		{
			DoCompare();
			
			if (!everyFrame)
				Finish();
		}
		
		public override void OnLateUpdate()
		{
			DoCompare();
		}
		
		protected abstract float GetStat();
		
		void DoCompare()
		{
			float stat = GetStat();
			
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