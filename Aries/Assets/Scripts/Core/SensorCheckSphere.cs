using UnityEngine;
using System.Collections.Generic;

public abstract class SensorCheckSphere<T> : MonoBehaviour where T : Component {
	public LayerMask mask;
	
	public float radius;
	
	public float delay = 0.1f;

	public HashSet<T> units = new HashSet<T>();
	
	private HashSet<T> mGatherUnits = new HashSet<T>();
	
	protected abstract bool UnitVerify(T unit);
	protected abstract void UnitAdded(T unit);
	protected abstract void UnitRemoved(T unit);
	
	void OnEnable() {
		InvokeRepeating("Check", delay, delay);
	}
	
	void OnDisable() {
		CancelInvoke();
	}
	
	void Check() {
		//get units in area
		mGatherUnits.Clear();
		
		Collider[] cols = Physics.OverlapSphere(transform.position, radius, mask.value);
		foreach(Collider col in cols) {
			T unit = col.GetComponent<T>();
			if(unit != null && UnitVerify(unit)) {
				mGatherUnits.Add(unit);
				
				//check if not in current units
				if(!units.Contains(unit)) {
					//enter
					UnitAdded(unit);
				}
				else {
					units.Remove(unit);
				}
			}
		}
		
		//what remains should be removed
		foreach(T other in units) {
			UnitRemoved(other);
		}
		
		//swap
		HashSet<T> prevSet = units;
		units = mGatherUnits;
		mGatherUnits = prevSet;
	}
	
	void OnDrawGizmosSelected() {
		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere(transform.position, radius);
	}
}
