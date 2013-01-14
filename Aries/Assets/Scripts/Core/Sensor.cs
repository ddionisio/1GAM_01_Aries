using UnityEngine;
using System.Collections.Generic;

public abstract class Sensor<T> : MonoBehaviour where T : MonoBehaviour {
	public HashSet<T> units = new HashSet<T>();
	
	protected abstract bool UnitVerify(T unit);
	protected abstract void UnitAdded(T unit);
	protected abstract void UnitRemoved(T unit);

	void OnTriggerEnter(Collider other) {
		T unit = other.GetComponent<T>();
		if(unit != null && UnitVerify(unit)) {
			if(units.Add(unit)) {
				UnitAdded(unit);
			}
		}
	}
	
	void OnTriggerExit(Collider other) {
		T unit = other.GetComponent<T>();
		if(unit != null && units.Remove(unit)) {
			UnitRemoved(unit);
		}
	}
}
