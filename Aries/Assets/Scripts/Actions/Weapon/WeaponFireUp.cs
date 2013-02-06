using UnityEngine;
using HutongGames.PlayMaker;

namespace Game.Actions {
	[ActionCategory("Game")]
	[Tooltip("Shoot with given weapon to up vector.")]
	public class WeaponFireUp : FSMActionComponentBase<Weapon>
	{
        [Tooltip("The Game Object that owns the stat for damage modifier.")]
        public FsmOwnerDefault ownerStat;

		[Tooltip("Optional target to seek (depends on type of weapon)")]
		public FsmGameObject seek;
		
		[Tooltip("Offset")]
		public FsmVector2 ofs;
				
		public override void Reset() {
			base.Reset();
			
			checkChildren = true;
			seek = null;
			ofs = Vector2.zero;
		}
		
		// Code that runs on entering the state.
		public override void OnEnter()
		{
			base.OnEnter();
			
			if(mComp != null) {
                float damageMod = 0.0f;
                GameObject ownerStatGO = Fsm.GetOwnerDefaultTarget(ownerStat);
                if(ownerStatGO == null)
                    ownerStatGO = mOwnerGO;

                if(ownerStatGO != null) {
                    StatBase stat = ownerStatGO.GetComponent<StatBase>();
                    if(stat != null) {
                        damageMod = stat.damageMod;
                    }
                }

				mComp.ShootUpDir(ofs.Value, damageMod, seek.Value != null ? seek.Value.transform : null);
			}
			
			Finish();
		}
	}
}