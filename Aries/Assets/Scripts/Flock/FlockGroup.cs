using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FlockGroup : MonoBehaviour {	
	public int startNumSpawn;
	
	public float radius;
	
	private List<FlockUnit> mUnits;
	
	public List<FlockUnit> units {
		get { return mUnits; }
	}
	
	public void AddUnit(FlockUnit unit) {
		//reparent?
		mUnits.Add(unit);
		OnAddUnit(unit);
	}
	
	protected virtual void OnAddUnit(FlockUnit unit) {
	}
	
	protected virtual void Awake() {
		//set initial flocks from scene
		mUnits = new List<FlockUnit>(transform.childCount);
		
		foreach(Transform t in transform) {
			FlockUnit unit = t.GetComponentInChildren<FlockUnit>();
			if(unit != null) {
				mUnits.Add(unit);
				OnAddUnit(unit);
			}
		}
	}
	
	protected virtual void Start() {
		if(startNumSpawn > 0) {
			//spawn stuff 
		}
	}
	

	void OnDrawGizmosSelected() {
		Gizmos.color = Color.yellow;
		
		Gizmos.DrawWireSphere(transform.position, radius);
	}
}
