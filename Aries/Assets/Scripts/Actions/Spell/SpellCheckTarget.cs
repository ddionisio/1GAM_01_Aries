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
			UnitEntity unit = mComp;
			if(unit != null && unit.spellCaster != null && unit.listener != null && unit.listener.currentTarget != null) {
				UnitEntity targetUnit = unit.listener.currentTarget.GetComponent<UnitEntity>();
				if(unit.spellCaster.CanCastTo(targetUnit)) {
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
