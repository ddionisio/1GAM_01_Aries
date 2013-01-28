using UnityEngine;
using System.Collections;

public class MotionBase : MonoBehaviour {
	public float maxSpeed = 20.0f; //meters/sec
	
	protected Vector2 mDir = Vector2.right;
	protected float mCurSpeed;
	
	private Rigidbody mBody;
		
	public Vector2 dir {
		get { return mDir; }
	}
	
	public Rigidbody body {
		get { return mBody; }
	}
	
	public float curSpeed {
		get { return mCurSpeed; }
	}
	
	public virtual void ResetData() {
		mBody.velocity = Vector3.zero;
	}
	
	protected virtual void Awake() {
		mBody = rigidbody;
	}
	
	protected virtual void FixedUpdate() {
		//get direction and limit speed
		Vector2 vel = mBody.velocity;
		mCurSpeed = vel.magnitude;
		
		if(mCurSpeed > 0) {
			mDir = vel/mCurSpeed;
			
			if(mCurSpeed > maxSpeed) {
				mBody.velocity = mDir*maxSpeed;
			}
		}
	}
}
