using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FlockSensor : MonoBehaviour {
	[System.NonSerialized] public FlockType typeFilter;
	
	public HashSet<Flock> flocks = new HashSet<Flock>();

	void OnTriggerEnter(Collider other) {
		Flock flock = other.GetComponent<Flock>();
		if(flock != null && typeFilter == flock.type) {
			flocks.Add(flock);
		}
	}
	
	void OnTriggerExit(Collider other) {
		Flock flock = other.GetComponent<Flock>();
		if(flock != null && typeFilter == flock.type) {
			flocks.Remove(flock);
		}
	}
}
