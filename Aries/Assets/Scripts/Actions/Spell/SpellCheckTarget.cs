using HutongGames.PlayMaker;

namespace Game.Actions {
	[ActionCategory("Game")]
	[Tooltip("Check caster's target to see if it is damnable.")]
	public class SpellCheckTarget : FSMActionComponentBase<UnitEntity> {
		public FsmEvent isTrue;
		public FsmEvent isFalse;
		
		public bool everyFrame;
		
		public override void Reset()
		{
			base.Reset();
			
			isTrue = null;
			isFalse = null;
			everyFrame = false;
		}
		
		public override void OnEnter()
		{
			base.OnEnter();
			
			DoCheck();
			if(!everyFrame)
				Finish();
		}
		
		public override void OnUpdate ()
		{
			DoCheck();
		}
		
		void DoCheck() {
			if(mComp != null && mComp.spellCaster != null && mComp.listener != null && mComp.listener.currentTarget != null) {
				UnitEntity targetUnit = mComp.listener.currentTarget.GetComponent<UnitEntity>();
				if(mComp.spellCaster.CanCastTo(targetUnit)) {
					Fsm.Event(isTrue);
				}
				else {
					Fsm.Event(isFalse);
				}
			}
			else {
				Fsm.Event(isFalse);
			}
		}
		
		public override string ErrorCheck()
		{
			if (everyFrame &&
				FsmEvent.IsNullOrEmpty(isTrue) &&
				FsmEvent.IsNullOrEmpty(isFalse))
				return "Action sends no events!";
			return "";
		}
	}
}
