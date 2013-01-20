using UnityEngine;
using HutongGames.PlayMaker;

namespace Game.Actions {
	[ActionCategory("Game")]
	[Tooltip("Removes entity and returns it to the manager")]
	public class EntityRelease : FsmStateAction
	{
		[RequiredField]
		[Tooltip("The entity to release")]
		[UIHint(UIHint.FsmGameObject)]
		public EntityBase entity;
		
		// Code that runs on entering the state.
		public override void OnEnter()
		{
			entity.Release();
			
			Finish();
		}
	}
}