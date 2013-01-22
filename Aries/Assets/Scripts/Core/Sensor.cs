using UnityEngine;
using System.Collections.Generic;

public abstract class Sensor<T> : MonoBehaviour where T : Component {
	protected abstract bool UnitVerify(T unit);
	protected abstract void UnitAdded(T unit);
	protected abstract void UnitRemoved(T unit);
	
	private HashSet<T> mUnits = new HashSet<T>();
	
	public HashSet<T> units {
		get {
			CleanUp();
			return mUnits;
		}
	}
			
	void OnTriggerEnter(Collider other) {
		T unit = other.GetComponent<T>();
		if(unit != null && UnitVerify(unit)) {
			if(mUnits.Add(unit)) {
				UnitAdded(unit);
			}
		}
	}
	
	void OnTriggerExit(Collider other) {
		T unit = other.GetComponent<T>();
		if(unit != null && mUnits.Remove(unit)) {
			UnitRemoved(unit);
		}
	}
	
	void CleanUp() {
		mUnits.RemoveWhere(delegate(T unit) {return unit == null || !unit.gameObject.activeInHierarchy;});
	}
}
