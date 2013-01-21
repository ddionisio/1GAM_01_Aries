using HutongGames.PlayMaker;

namespace Game.Actions {
	[ActionCategory("Game")]
	[Tooltip("Set certain flags for flock unit")]
	public class FlockSetFlags : FSMActionComponentBase<FlockUnit> {
		public FsmBool catchUp;
		public FsmBool groupMove;
		public FsmBool wander;
		
		public override void Reset ()
		{
			base.Reset();
			
			catchUp = true;
			groupMove = true;
			wander = false;
		}
		
		// Code that runs on entering the state.
		public override void OnEnter()
		{
			if(mComp != null) {
				mComp.catchUpEnabled = catchUp.Value;
				mComp.groupMoveEnabled = groupMove.Value;
				mComp.wanderEnabled = wander.Value;
			}
			
			Finish();
		}
	}
}
