using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FlockAntiField : MonoBehaviour {
	public enum Type {
		Away, //move away from center
		Dir, //move by given direction (use for things like 'wind')
	}
		
	public Type type = Type.Away;
	
	public float force = 100.0f;
	
	public float angle = 0.0f; //starting at right (degree)
	
	public float updateDelay = 0.1f;
	
	private Vector2 mDir = Vector2.right;
	
	private float mCurDelay = 0;
			
	public Vector2 dir { get { return mDir; } }
	
	private HashSet<Rigidbody> mBodies = new HashSet<Rigidbody>();
	
	void Awake() {
		if(type == Type.Dir) {
			mDir = M8.Math.Rotate(mDir, angle*Mathf.Deg2Rad);
		}
	}
	
	void OnTriggerEnter(Collider t) {
		Rigidbody body = t.rigidbody;
		if(body != null) {
			mBodies.Add(body);
		}
	}
	
	void OnTriggerExit(Collider t) {
		Rigidbody body = t.rigidbody;
		if(body != null) {
			mBodies.Remove(body);
		}
	}
	
	void FixedUpdate () {
		//repel and repent
		if(mBodies.Count > 0) {
			mCurDelay += Time.fixedDeltaTime;
			if(mCurDelay >= updateDelay) {
				mCurDelay = 0.0f;
				
				Vector2 antiPos = transform.position;
								
				switch(type) {
				case Type.Away:
					foreach(Rigidbody body in mBodies) {
						Vector2 pos = body.transform.position;
						Vector2 forceAdd = (pos - antiPos).normalized*force;
						body.AddForce(forceAdd.x, forceAdd.y, 0.0f);
					}
					break;
					
				case Type.Dir:
					foreach(Rigidbody body in mBodies) {
						Vector2 forceAdd = mDir*force;
						body.AddForce(forceAdd.x, forceAdd.y, 0.0f);
					}
					break;
				}
			}
		}
	}
	
	void OnDrawGizmos() {
		if(type == Type.Dir) {
			Gizmos.color = Color.green;
			
			Gizmos.DrawRay(transform.position, M8.Math.Rotate(Vector2.right, angle*Mathf.Deg2Rad));
		}
	}
}