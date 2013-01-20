using HutongGames.PlayMaker;

namespace Game.Actions {
	[ActionCategory("Game")]
	[Tooltip("Check entity's health based on given value and criteria")]
	public class StatUnitCheckResource : StatCheckBase<UnitStat> {
		protected override float GetStat() {
			return stats.love;
		}
	}
}