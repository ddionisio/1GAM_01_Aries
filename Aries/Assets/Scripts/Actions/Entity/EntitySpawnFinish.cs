using UnityEngine;
using HutongGames.PlayMaker;

namespace Game.Actions {
	[ActionCategory("Game")]
	[Tooltip("Let entity know that we have finished spawning.")]
	public class EntitySpawnFinish : FSMActionComponentBase<EntityBase>
	{
		// Code that runs on entering the state.
		public override void OnEnter()
		{
			base.OnEnter();
			
			if(mComp != null)
				mComp.SpawnFinish();
			
			Finish();
		}
	}
}