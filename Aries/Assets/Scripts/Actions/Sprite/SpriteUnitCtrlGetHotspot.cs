using HutongGames.PlayMaker;

namespace Game.Actions {
	[ActionCategory("Game")]
	[Tooltip("Get the hotspot based on current direction.")]
	public class SpriteUnitCtrlGetHotspot : FSMActionComponentBase<UnitSpriteController> {
		[RequiredField]
		[UIHint(UIHint.Variable)]
		[Tooltip("Store the hotspot.")]
		public FsmVector2 storeVector;
		
		public override void Reset ()
		{
			base.Reset();
			checkChildren = true;
			
			storeVector = null;
		}
		
		// Code that runs on entering the state.
		public override void OnEnter()
		{
			base.OnEnter();
			
			UnitSpriteController c = mComp;
			if(c != null) {
				storeVector.Value = c.hotspot;
			}
			
			Finish();
		}
	}
}