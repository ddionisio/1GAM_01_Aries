using UnityEngine;
using System.Collections;

public class ActionSensor : SensorCheckSphere<ActionTarget> {
	protected override bool UnitVerify(ActionTarget unit) {
		return true;
	}
	
	protected override void UnitAdded(ActionTarget unit) {
		unit.indicatorOn = true;
	}
	
	protected override void UnitRemoved(ActionTarget unit) {
		unit.indicatorOn = false;
	}
}
