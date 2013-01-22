using HutongGames.PlayMaker;

namespace Game.Actions {
	[ActionCategory("Game")]
	[Tooltip("Check entity's health based on given value and criteria")]
	public class StatPlayerCheckResource : StatCheckBase<PlayerStat> {
		protected override float GetStat(PlayerStat s) {
			return s.curResource;
		}
	}
}