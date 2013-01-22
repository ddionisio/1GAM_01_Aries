using UnityEngine;
using HutongGames.PlayMaker;

namespace Game.Actions {
	[ActionCategory("Game")]
	[Tooltip("Removes entity and returns it to the manager")]
	public class EntityRelease : FSMActionComponentBase<EntityBase>
	{
		// Code that runs on entering the state.
		public override void OnEnter()
		{
			base.OnEnter();
			
			EntityBase e = mComp;
			if(e != null)
				e.Release();
			
			Finish();
		}
	}
}