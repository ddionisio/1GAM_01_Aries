using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FlockGroup : MonoBehaviour {					
	public int startNumSpawn;
	
	public float radius;
	
	public Transform target;
		
	private List<Flock> mFlocks;
	
	private float mDistanceSq;
	
	public void AddFlock(Flock flock) {
	}
	
	void Awake() {
		//set initial flocks from scene
		mFlocks = new List<Flock>(transform.childCount);
		
		foreach(Transform t in transform) {
			Flock flock = t.GetComponentInChildren<Flock>();
			if(flock != null) {
				mFlocks.Add(flock);
				
				if(target != null) {
					flock.moveTarget = target;
				}
			}
		}
	}
	
	void Start() {
		if(startNumSpawn > 0) {
			//spawn stuff
		}
	}
	

	void OnDrawGizmosSelected() {
		Gizmos.color = Color.yellow;
		
		if(target != null) {
			Gizmos.DrawWireSphere(transform.position, radius);
		}
		else {
			Gizmos.DrawWireSphere(transform.position, radius);
		}
	}
}
