using UnityEngine;
using HutongGames.PlayMaker;

namespace Game.Actions {
	[ActionCategory("Game")]
	[Tooltip("Stop repeating fire.")]
	public class WeaponFireStop : FSMActionComponentBase<Weapon>
	{
		// Code that runs on entering the state.
		public override void OnEnter()
		{
			base.OnEnter();
			
			Weapon w = mComp;
			if(w != null) {
				w.RepeatStop();
			}
			
			Finish();
		}
	}
}