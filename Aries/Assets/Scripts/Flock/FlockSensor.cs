using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FlockSensor : MonoBehaviour {
	[System.NonSerialized] public FlockType typeFilter;
	
	public HashSet<FlockUnit> units = new HashSet<FlockUnit>();

	void OnTriggerEnter(Collider other) {
		FlockUnit unit = other.GetComponent<FlockUnit>();
		if(unit != null && typeFilter == unit.type) {
			units.Add(unit);
		}
	}
	
	void OnTriggerExit(Collider other) {
		FlockUnit flock = other.GetComponent<FlockUnit>();
		if(flock != null && typeFilter == flock.type) {
			units.Remove(flock);
		}
	}
}
