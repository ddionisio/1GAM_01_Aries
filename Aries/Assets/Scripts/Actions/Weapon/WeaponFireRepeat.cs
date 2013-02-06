using UnityEngine;
using HutongGames.PlayMaker;

namespace Game.Actions {
	[ActionCategory("Game")]
	[Tooltip("Shoot with given weapon repeatedly.")]
	public class WeaponFireRepeat : FSMActionComponentBase<Weapon>
	{
        [Tooltip("The Game Object that owns the stat for damage modifier.")]
        public FsmOwnerDefault ownerStat;

		[Tooltip("Optional target to seek (depends on type of weapon)")]
		public FsmGameObject seek;
		
		[Tooltip("Direction")]
		public FsmVector2 dir;
		
		public FsmBool useOwnerPosition;
		
		private Weapon.RepeatParam mParam = new Weapon.RepeatParam();
				
		public override void Reset() {
			base.Reset();
			
			checkChildren = true;
			seek = null;
			dir = Vector2.zero;
			useOwnerPosition = false;
		}
		
		// Code that runs on entering the state.
		public override void OnEnter()
		{
			base.OnEnter();
			
			if(mComp != null) {
                GameObject ownerStatGO = Fsm.GetOwnerDefaultTarget(ownerStat);
                if(ownerStatGO == null)
                    ownerStatGO = mOwnerGO;

                if(ownerStatGO != null) {
                    mParam.sourceStat = ownerStatGO.GetComponent<StatBase>();
                }

				mParam.seek = seek.Value != null ? seek.Value.transform : null;
				mParam.dir = dir.Value;
				GameObject go = mOwnerGO;
				mParam.source = useOwnerPosition.Value && go != null ? go.transform : null;
				
				mComp.Repeat(mParam);
			}
			
			Finish();
		}
	}
}