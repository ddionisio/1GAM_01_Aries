using HutongGames.PlayMaker;

namespace Game.Actions {
	[ActionCategory("Game")]
	[Tooltip("Cancel casting.")]
	public class SpellCancel : FSMActionComponentBase<UnitEntity> {
		public override void OnEnter()
		{
			base.OnEnter();
			
			if(mComp != null && mComp.spellCaster != null) {
				mComp.spellCaster.Cancel();
			}
			
			Finish();
		}
	}
}
