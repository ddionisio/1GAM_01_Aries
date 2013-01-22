using HutongGames.PlayMaker;

namespace Game.Actions {
	[ActionCategory("Game")]
	[Tooltip("Check entity's health based on given value and criteria")]
	public class StatCheckHealth : StatCheckBase<StatBase>
	{
		protected override float GetStat(StatBase s) {
			return s.curHP;
		}
	}
}