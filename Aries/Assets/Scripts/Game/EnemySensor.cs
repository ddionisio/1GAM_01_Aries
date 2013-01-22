using UnityEngine;
using System.Collections;

public class EnemySensor : Sensor<EntityBase> {
	protected override bool UnitVerify(EntityBase unit) {
		return true;
	}
	
	protected override void UnitAdded(EntityBase unit) {
	}
	
	protected override void UnitRemoved(EntityBase unit) {
	}
}
