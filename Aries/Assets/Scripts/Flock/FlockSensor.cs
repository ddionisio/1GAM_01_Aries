using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FlockSensor : Sensor<FlockUnit> {
	[System.NonSerialized] public FlockType typeFilter;
	
	protected override bool UnitVerify(FlockUnit unit) {
		return unit.type == typeFilter;
	}
	
	protected override void UnitAdded(FlockUnit unit) {
	}
	
	protected override void UnitRemoved(FlockUnit unit) {
	}
}
