using UnityEngine;
using HutongGames.PlayMaker;

namespace Game.Actions {
	[ActionCategory("Game")]
	[Tooltip("Show status indicator.")]
	public class UnitShowStatIndicator : FSMActionComponentBase<UnitEntity>
	{
		public UnitStatusIndicator.Icon icon;
		
		public FsmFloat duration;
				
		public override void Reset() {
			base.Reset();
			
			icon = UnitStatusIndicator.Icon.NumIcons;
			
			duration = null;
		}
		
		// Code that runs on entering the state.
		public override void OnEnter()
		{
			base.OnEnter();
			
			if(mComp != null && mComp.statusIndicator != null) {
				mComp.statusIndicator.Show(icon, duration.Value);
			}
			
			Finish();
		}
	}
}