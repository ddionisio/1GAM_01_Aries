using UnityEngine;
using System.Collections;

public class FlockAnti : MonoBehaviour {
	public enum Type {
		Away, //move a flock unit away, average with other anti's nearby, along with factor
		Force, //independent move away using force
		Dir, //move by given direction, averages with other anti's nearby, along with factor
		DirForce, //independent move by given direction using force (use for things like 'wind')
	}
	
	public Type type = Type.Away;
	public FlockType typeFilter;
	
	public float factor = 1.0f;
	public float force = 100.0f;
	public float angle = 0.0f; //starting at right (degree)
	
	private Vector2 mDir = Vector2.right;
	
	public Vector2 dir { get { return mDir; } }
	
	void Awake() {
		if(type == Type.Dir || type == Type.DirForce) {
			mDir = M8.Math.Rotate(mDir, angle*Mathf.Deg2Rad);
		}
	}
	
	void OnDrawGizmos() {
		if(type == Type.Dir || type == Type.DirForce) {
			Gizmos.color = Color.green;
			
			Gizmos.DrawRay(transform.position, M8.Math.Rotate(Vector2.right, angle*Mathf.Deg2Rad));
		}
	}
}
