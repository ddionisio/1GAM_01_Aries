using HutongGames.PlayMaker;

namespace Game.Actions {
	[ActionCategory("Game")]
	[Tooltip("Change unit sprite controller's reverse")]
	public class SpriteUnitCtrlSetReverse : FSMActionComponentBase<UnitSpriteController> {
		public FsmBool reverse;
		
		public override void Reset ()
		{
			base.Reset();
			
			checkChildren = true;
			reverse = null;
		}
		
		// Code that runs on entering the state.
		public override void OnEnter()
		{
			base.OnEnter();
			
			UnitSpriteController c = mComp;
			if(c != null)
				c.reverse = reverse.Value;
			
			Finish();
		}
	}
}