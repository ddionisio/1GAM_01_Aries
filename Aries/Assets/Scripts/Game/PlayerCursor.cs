using UnityEngine;
using System.Collections;

public class PlayerCursor : MonoBehaviour {
	public float radius;
	
	public float distance = 5.0f;
	
	public LayerMask checkMask;
	
	public ActionSensor attackSensor; //for all hostile enemies
	public ActionSensor contextSensor; //anything non-combat related (or sub target for bosses)
	
	private Transform mOrigin;
	
	private Vector2 mDir = Vector2.up;
	
	public Transform origin {
		get { return mOrigin != null ? mOrigin : transform; }
		set {
			mOrigin = value;
		}
	}
	
	public Vector2 dir {
		get { return mDir; }
		set { mDir = value; }
	}
	
	public bool CheckArea(int layerMask) {
		return Physics.CheckSphere(transform.position, radius, layerMask);
	}
	
	void Awake() {
	}
	
	// Use this for initialization
	void Start () {
	}
	
	void Update() {
		Vector3 start = origin.position;
		
		//cast to reposition
		if(mDir != Vector2.zero) {
			RaycastHit hit;
			if(Physics.SphereCast(start, radius, mDir, out hit, distance, checkMask.value)) {
				Vector3 delta = mDir*hit.distance;
				transform.position = start + delta;
			}
			else {
				Vector3 delta = mDir*distance;
				transform.position = start + delta;
			}
		}
	}
		
	void OnDrawGizmosSelected() {
		Vector3 s = origin.position, e = transform.position;
		
		Gizmos.color = Color.blue;
		Gizmos.DrawLine(s, e);
		
		Gizmos.color = Color.cyan;
		Gizmos.DrawWireSphere(e, radius);
	}
}
