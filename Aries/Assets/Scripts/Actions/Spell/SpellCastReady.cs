using HutongGames.PlayMaker;

namespace Game.Actions {
    [ActionCategory("Game")]
    [Tooltip("Set casting to ready, make sure this is called before casting spells.")]
    public class SpellCastReady : FSMActionComponentBase<UnitEntity> {
        public override void OnEnter() {
            base.OnEnter();

            if(mComp != null && mComp.spellCaster != null) {
                mComp.spellCaster.Ready();
            }

            Finish();
        }
    }
}
