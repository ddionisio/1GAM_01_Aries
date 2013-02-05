using UnityEngine;
using HutongGames.PlayMaker;

namespace Game.Actions {
	[ActionCategory("Game")]
	[Tooltip("Get listener's target and store its game object.")]
	public class ActionListenerGetTarget : FSMActionComponentBase<ActionListener> {
		[Tooltip("The game object the target belongs to.")]
		[UIHint(UIHint.FsmGameObject)]
		public FsmGameObject toGameObject;
		
		public override void Reset()
		{
			base.Reset();
			
			toGameObject = null;
		}
		
		public override void OnEnter()
		{
			base.OnEnter();
			
			toGameObject.Value = mComp != null && mComp.currentTarget != null ? mComp.currentTarget.gameObject : null;
			
			Finish();
		}
	}
}
