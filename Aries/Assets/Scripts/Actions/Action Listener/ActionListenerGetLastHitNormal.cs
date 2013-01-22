using UnityEngine;
using System.Collections;
using HutongGames.PlayMaker;

namespace Game.Actions {
	[ActionCategory("Game")]
	[Tooltip("Get the normal of the collision we last hit. This is updated on each EntityActionHitEnter event.")]
	public class ActionListenerGetLastHitNormal : FSMActionComponentBase<ActionListener> {
		[RequiredField]
		[UIHint(UIHint.Variable)]
		[Tooltip("Store the normal value.")]
		public FsmVector2 storeVector;
		
		public override void Reset ()
		{
			base.Reset ();
			
			storeVector = null;
		}
		
		public override void OnEnter ()
		{
			base.OnEnter ();
			
			ActionListener l = mComp;
			if(l != null)
				storeVector.Value = l.lastHitInfo.normal;
			
			Finish();
		}
	}
}