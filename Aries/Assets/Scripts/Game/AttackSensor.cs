using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class AttackSensor : SensorSingle<UnitEntity> {
	
	public StatBase stat;
	
	protected override bool UnitVerify(UnitEntity unit) {
		return stat.CanDamage(unit.stats);
	}
}
