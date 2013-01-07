using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FlockGroup : MonoBehaviour {					
	public int startNumSpawn;
	
	public float radius;
	
	public Transform target;
		
	private List<FlockUnit> mUnits;
	
	private float mDistanceSq;
	
	public void AddUnit(FlockUnit unit) {
	}
	
	void Awake() {
		//set initial flocks from scene
		mUnits = new List<FlockUnit>(transform.childCount);
		
		foreach(Transform t in transform) {
			FlockUnit unit = t.GetComponentInChildren<FlockUnit>();
			if(unit != null) {
				mUnits.Add(unit);
				
				if(target != null) {
					unit.moveTarget = target;
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
			Gizmos.DrawWireSphere(target.position, radius);
		}
		else {
			Gizmos.DrawWireSphere(transform.position, radius);
		}
	}
}
