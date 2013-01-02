using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WaypointManager : MonoBehaviour {
	
	private static WaypointManager mInstance = null;
	private Dictionary<string, List<Transform>> mWaypoints;
	
	public static WaypointManager instance {
		get {
			return mInstance;
		}
	}
	
	public List<Transform> GetWaypoints(string name) {
		List<Transform> ret = null;
		mWaypoints.TryGetValue(name, out ret);
		return ret;
	}
	
	//get the first one if > 1
	public Transform GetWaypoint(string name) {
		Transform ret = null;
		List<Transform> wps;
		if(mWaypoints.TryGetValue(name, out wps) && wps.Count > 0) {
			ret = wps[0];
		}
		return ret;
	}
		
	void OnDestroy() {
		mInstance = null;
		mWaypoints.Clear();
	}
	
	void Awake() {
		mInstance = this;
		
		mWaypoints = new Dictionary<string, List<Transform>>(transform.childCount);
		
		//generate waypoints based on their names
		foreach(Transform child in transform) {
			List<Transform> points;
			
			if(child.childCount > 0) {
				points = new List<Transform>(child.childCount);
				foreach(Transform t in child) {
					points.Add(t);
				}
				points.Sort(delegate(Transform t1, Transform t2) {
					return t1.name.CompareTo(t2.name);
				});
			}
			else {
				points = new List<Transform>(1);
				points.Add(child);
			}
			
			mWaypoints.Add(child.name, points);
		}
	}
	
	void OnDrawGizmos() {
		foreach(Transform t in transform) {
			Gizmos.color = Color.white;
			Gizmos.DrawIcon(t.position, "waypoint", true);
		}
	}
}
