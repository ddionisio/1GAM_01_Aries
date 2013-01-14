using UnityEngine;
using System.Collections;

public class PlayerCursor : MonoBehaviour {
	public Transform origin;
	
	public float radius;
	
	public float distance = 5.0f;
	
	public ActionSensor attackSensor; //for all hostile enemies
	public ActionSensor contextSensor; //anything non-combat related (or sub target for bosses)
	
	private int mLayerMask;
	
	private Vector2 mDir = Vector2.up;
	
	public Vector2 dir {
		get { return mDir; }
		set { mDir = value; }
	}
	
	void Awake() {
	}
	
	// Use this for initialization
	void Start () {
		//put other stuff here if needed
		mLayerMask = Layers.layerMaskWall;
	}
	
	void Update() {
		Vector3 start = origin.position;
		
		//cast to reposition
		if(mDir != Vector2.zero) {
			RaycastHit hit;
			if(Physics.SphereCast(start, radius, mDir, out hit, distance, mLayerMask)) {
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
		Vector3 s = origin == null ? transform.position : origin.position, e = transform.position;
		
		Gizmos.color = Color.blue;
		Gizmos.DrawLine(s, e);
		
		Gizmos.color = Color.cyan;
		Gizmos.DrawWireSphere(e, radius);
	}
}
