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
			
			if(mComp != null && mComp.spellCaster != null && mComp.listener != null && mComp.listener.currentTarget != null) {
				mComp.spellCaster.castDoneCallback += SpellFinish;
				
				UnitEntity targetUnit = mComp.listener.currentTarget.GetComponent<UnitEntity>();
				
				mComp.spellCaster.CastTo(targetUnit);
			}
			else {
				Finish();
			}
		}
		
		public override void OnExit () {
			if(mComp != null && mComp.spellCaster != null) {
				mComp.spellCaster.castDoneCallback -= SpellFinish;
			}
			
			base.OnExit ();
		}
		
		void SpellFinish(SpellCaster caster) {
			Fsm.Event(finish);
		}
	}
}
