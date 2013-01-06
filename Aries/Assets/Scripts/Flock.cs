using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//rename
public class Flock : MonoBehaviour {
	public FlockType type;
	
	public FlockSensor sensor;
			
	public float maxForce = 120.0f; //N
	public float maxSpeed = 20.0f; //meters/sec
	
	public float separateDistance = 2.0f;
	
	public float separateFactor = 1.5f;
	public float alignFactor = 1.0f;
	public float cohesionFactor = 1.0f;
	public float moveToFactor = 1.0f;
	
	[System.NonSerialized] public Transform moveTarget = null;
	
	private Vector2 mDir = Vector2.zero;
	private Rigidbody mBody;
	private Transform mTrans;
	
	public Vector2 dir {
		get { return mDir; }
	}
	
	public Rigidbody body {
		get { return mBody; }
	}
		
	void Awake() {
		if(sensor != null) {
			sensor.typeFilter = type;
		}
		
		mBody = rigidbody;
		mTrans = transform;
	}
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		Vector2 sep, align, coh;
		
		ComputeMovement(out sep, out align, out coh);
		
		mBody.AddForce(sep.x, sep.y, 0.0f);
		mBody.AddForce(align.x, align.y, 0.0f);
		mBody.AddForce(coh.x, coh.y, 0.0f);
		
		if(moveTarget != null && (sensor == null || sensor.flocks.Count > 3)) {
			Vector2 targetPos = moveTarget.position;
			
			Vector2 seek = Seek(targetPos);
			seek *= moveToFactor;
			
			mBody.AddForce(seek.x, seek.y, 0.0f);
		}
		
		mBody.velocity = M8.Math.Limit(mBody.velocity, maxSpeed);
	}
	
	private Vector2 Seek(Vector2 target) {
		Vector2 pos = mTrans.localPosition;
		Vector2 vel = mBody.velocity;
		
		Vector2 desired = target - pos;
		desired.Normalize();
		desired *= maxSpeed;
		
		return M8.Math.Limit(desired - vel, maxForce);
	}
	
	private void ComputeMovement(out Vector2 separate, out Vector2 align, out Vector2 cohesion) {
		separate = Vector2.zero;
		align = Vector2.zero;
		cohesion = Vector2.zero;
		
		if(sensor != null && sensor.flocks.Count > 0) {
			Vector2 pos = mTrans.localPosition;
			Vector2 vel = mBody.velocity;
			
			Vector2 dPos;
			float dist;
			
			int numSeparate = 0;
			
			foreach(Flock flock in sensor.flocks) {
				Vector2 otherPos = flock.transform.localPosition;
				Vector2 otherVel = flock.body.velocity;
				
				//separate
				dPos = pos - otherPos;
				if(dPos.magnitude < separateDistance) {
					separate += dPos;
					numSeparate++;
				}
				
				//align speed
				align += otherVel;
				
				//cohesion
				cohesion += otherPos;
			}
			
			//calculate separate
			if(numSeparate > 0) {
				separate /= (float)numSeparate;
				
				dist = separate.magnitude;
				if(dist > 0) {
					separate /= dist;
					separate *= maxForce;
					separate -= vel;
					M8.Math.Limit(ref separate, maxForce);
					
					separate *= separateFactor;
				}
			}
			
			float fCount = (float)sensor.flocks.Count;
			
			//calculate align
			align /= fCount;
			align.Normalize();
			align *= maxSpeed;
			align -= vel; //steer
			M8.Math.Limit(ref align, maxForce);
			
			align *= alignFactor;
			
			//calculate cohesion
			cohesion /= fCount;
			cohesion = Seek(cohesion);
			cohesion *= cohesionFactor;
		}
	}
}
