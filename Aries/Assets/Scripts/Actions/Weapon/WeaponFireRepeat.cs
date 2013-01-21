using UnityEngine;
using HutongGames.PlayMaker;

namespace Game.Actions {
	[ActionCategory("Game")]
	[Tooltip("Shoot with given weapon repeatedly.")]
	public class WeaponFireRepeat : FSMActionComponentBase<Weapon>
	{
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
				mParam.seek = seek.Value != null ? seek.Value.transform : null;
				mParam.dir = dir.Value;
				mParam.source = useOwnerPosition.Value && mOwnerGO != null ? mOwnerGO.transform : null;
				
				mComp.Repeat(mParam);
			}
			
			Finish();
		}
	}
}