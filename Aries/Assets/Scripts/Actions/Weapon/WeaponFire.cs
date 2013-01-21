using UnityEngine;
using HutongGames.PlayMaker;

namespace Game.Actions {
	[ActionCategory("Game")]
	[Tooltip("Shoot with given weapon.")]
	public class WeaponFire : FSMActionComponentBase<Weapon>
	{
		[Tooltip("Optional target to seek (depends on type of weapon)")]
		public FsmGameObject seek;
		
		[Tooltip("Direction")]
		public FsmVector2 dir;
				
		public override void Reset() {
			base.Reset();
			
			checkChildren = true;
			seek = null;
			dir = Vector2.zero;
		}
		
		// Code that runs on entering the state.
		public override void OnEnter()
		{
			base.OnEnter();
			
			if(mComp != null) {
				mComp.Shoot(dir.Value, seek.Value != null ? seek.Value.transform : null);
			}
			
			Finish();
		}
	}
}