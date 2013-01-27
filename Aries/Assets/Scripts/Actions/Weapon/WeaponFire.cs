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
		
		[Tooltip("Offset")]
		public FsmVector2 ofs;
				
		public override void Reset() {
			base.Reset();
			
			checkChildren = true;
			seek = null;
			dir = Vector2.zero;
			ofs = Vector2.zero;
		}
		
		// Code that runs on entering the state.
		public override void OnEnter()
		{
			base.OnEnter();
			
			Weapon w = mComp;
			if(w != null) {
				w.ShootOfs(ofs.Value, dir.Value, seek.Value != null ? seek.Value.transform : null);
			}
			
			Finish();
		}
	}
}