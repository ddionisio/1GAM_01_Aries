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
			
			UnitEntity u = mComp;
			if(u != null) {
				storeInt.Value = u.lastSpriteEventData.valI;
				storeString.Value = u.lastSpriteEventData.valS;
				storeFloat.Value = u.lastSpriteEventData.valF;
			}
			
			Finish();
		}
	}
}