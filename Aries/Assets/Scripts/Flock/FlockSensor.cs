using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FlockSensor : Sensor<FlockFilter> {
	protected override bool UnitVerify(FlockFilter unit) {
		return true;
	}
	
	protected override void UnitAdded(FlockFilter unit) {
	}
	
	protected override void UnitRemoved(FlockFilter unit) {
	}
}
