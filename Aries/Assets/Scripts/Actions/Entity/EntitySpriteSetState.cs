using UnityEngine;
using HutongGames.PlayMaker;

namespace Game.Actions {
	[ActionCategory("Game")]
	[Tooltip("Let entity know that we have finished spawning.")]
	public class EntitySpriteSetState : FSMActionComponentBase<EntitySpriteController>
	{
		public EntityState state;
		
		public override void Reset ()
		{
			base.Reset ();
			
			checkChildren = true;
			state = EntityState.normal;
		}
		
		// Code that runs on entering the state.
		public override void OnEnter()
		{
			base.OnEnter();
			
			if(mComp != null) {
				mComp.PlayAnim(state);
			}
			
			Finish();
		}
	}
}