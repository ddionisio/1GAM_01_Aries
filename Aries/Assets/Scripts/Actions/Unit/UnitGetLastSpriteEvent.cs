using UnityEngine;
using System.Collections;
using HutongGames.PlayMaker;

namespace Game.Actions {
	[ActionCategory("Game")]
	[Tooltip("Get the last sprite event from UnitEntity. This is updated on each EntitySpriteAnimEvent event.")]
	public class UnitGetLastSpriteEvent : FSMActionComponentBase<UnitEntity> {
		[UIHint(UIHint.Variable)]
		public FsmInt storeInt;
		
		[UIHint(UIHint.Variable)]
		public FsmString storeString;
		
		[UIHint(UIHint.Variable)]
		public FsmFloat storeFloat;
		
		public override void Reset ()
		{
			base.Reset ();
			
			storeInt = null;
			storeString = null;
			storeFloat = null;
		}
		
		public override void OnEnter ()
		{
			base.OnEnter ();
			
			if(mComp != null) {
				if(storeInt != null)
					storeInt.Value = mComp.lastSpriteEventData.valI;
					storeString.Value = mComp.lastSpriteEventData.valS;
					storeFloat.Value = mComp.lastSpriteEventData.valF;
			}
			
			Finish();
		}
	}
}