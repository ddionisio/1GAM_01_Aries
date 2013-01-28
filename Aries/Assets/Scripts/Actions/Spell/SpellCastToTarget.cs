using HutongGames.PlayMaker;

namespace Game.Actions {
	[ActionCategory("Game")]
	[Tooltip("Cast to target.")]
	public class SpellCastToTarget : FSMActionComponentBase<UnitEntity> {
		public FsmEvent finish;
		
		public override void Reset()
		{
			base.Reset();
			
			finish = null;
		}
		
		public override void OnEnter()
		{
			base.OnEnter();
			
			UnitEntity unit = mComp;
			if(unit != null && unit.spellCaster != null && unit.listener != null && unit.listener.currentTarget != null) {
				unit.spellCaster.castDoneCallback += SpellFinish;
				
				UnitEntity targetUnit = unit.listener.currentTarget.GetComponent<UnitEntity>();
				
				unit.spellCaster.CastTo(targetUnit);
			}
			else {
				Finish();
			}
		}
		
		public override void OnExit () {
			UnitEntity unit = mComp;
			if(unit != null && unit.spellCaster != null) {
				unit.spellCaster.castDoneCallback -= SpellFinish;
			}
			
			base.OnExit ();
		}
		
		void SpellFinish(SpellCaster caster) {
			Fsm.Event(finish);
		}
	}
}
