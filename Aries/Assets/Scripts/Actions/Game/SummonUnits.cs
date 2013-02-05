using UnityEngine;
using HutongGames.PlayMaker;

namespace Game.Actions {
	[ActionCategory("Game")]
	[Tooltip("Summon number of units of type with SummonController of owner.")]
	public class SummonUnits : FSMActionComponentBase<SummonController>
	{
		public UnitType type;
		
		public FsmInt amount;
		
		public FsmEvent finish;
				
		public override void Reset() {
			base.Reset();
			
			type = UnitType.NumTypes;
			amount = 0;
			finish = null;
		}
		
		// Code that runs on entering the state.
		public override void OnEnter()
		{
			base.OnEnter();
			
			if(mComp != null) {
				mComp.summonedCallback += SummonSpawned;
				
				if(type == UnitType.NumTypes || amount.Value == 0) {
					LogWarning("Invalid params for summoning units!");
					Finish();
				}
			}
			else {
				Finish();
			}
		}
		
		public override void OnExit ()
		{
			base.OnExit ();
			
			if(mComp != null) {
				mComp.summonedCallback -= SummonSpawned;
			}
		}
		
		void SummonSpawned(SummonController summonController, UnitEntity ent) {
			if(summonController.queueCount == 0) {
				Fsm.Event(finish);
				Finish();
			}
		}
	}
}