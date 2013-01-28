using HutongGames.PlayMaker;

namespace Game.Actions {
	[ActionCategory("Game")]
	[Tooltip("Cancel casting.")]
	public class SpellCancel : FSMActionComponentBase<UnitEntity> {
		public override void OnEnter()
		{
			base.OnEnter();
			
			UnitEntity unit = mComp;
			if(unit != null && unit.spellCaster != null) {
				unit.spellCaster.Cancel();
			}
			
			Finish();
		}
	}
}
