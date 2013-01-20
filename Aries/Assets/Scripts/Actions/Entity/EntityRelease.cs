using UnityEngine;
using HutongGames.PlayMaker;

namespace Game.Actions {
	[ActionCategory("Game")]
	[Tooltip("Removes entity and returns it to the manager")]
	public class EntityRelease : FsmStateAction
	{
		private EntityBase entity;
		
		public override void Init(FsmState aState)
		{
			base.Init(aState);
			
			if(entity == null)
				entity = aState.Fsm.Owner.GetComponentInChildren<EntityBase>();
		}
		
		// Code that runs on entering the state.
		public override void OnEnter()
		{
			entity.Release();
			
			Finish();
		}
	}
}