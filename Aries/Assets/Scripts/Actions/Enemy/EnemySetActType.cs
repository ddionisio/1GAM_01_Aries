using UnityEngine;
using HutongGames.PlayMaker;

namespace Game.Actions {
	[ActionCategory("Game")]
	[Tooltip("Set the enemy controller act to given type or default.")]
	public class EnemySetActType : FSMActionComponentBase<EnemyActionController> {
		[Tooltip("If you set this to NumType, then act will be based on target's act.")]
		public ActionType toType;
		
		[Tooltip("If this is true, ignore toType and just revert act to default.")]
		public bool toDefault;
					
		public override void Reset()
		{
			base.Reset();
			
			toType = ActionType.NumType;
			toDefault = true;
		}
		
		public override void OnEnter()
		{
			base.OnEnter();
			
			if(mComp != null) {
				if(toDefault)
					mComp.RevertActToDefault();
				else
					mComp.type = toType;
			}
			
			Finish();
		}
	}
}
