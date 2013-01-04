using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FlockGroup : MonoBehaviour {
	public Flock[] initFlocks; //use for debug, normally we want to spawn
					
	public int startNumSpawn;
	
	public float radius;
	
	public float radiusCheck = 150.0f;
	
	public float cohesionFactor = 0.01f;
	public float targetFactor = 0.01f;
	public float separateFactor = 0.01f;
	//public float steerFactor = 0.03f;
	public float alignVelocityFactor = 0.125f;
	
	public float distance = 50.0f; //minimum distance to each other
			
	private List<Flock> mFlocks;
	
	private float mDistanceSq;
	
	void Awake() {
		if(initFlocks.Length > 0) {
			mFlocks = new List<Flock>(initFlocks.Length);
			
			foreach(Flock flock in initFlocks) {
				mFlocks.Add(flock);
			}
		}
		else {
			mFlocks = new List<Flock>();
		}
		
		mDistanceSq = distance*distance;
	}
	
	void Start() {
		if(initFlocks.Length == 0) {
			//spawn stuff
		}
	}
	
	void Update() {
#if UNITY_EDITOR
		mDistanceSq = distance*distance;
#endif
		
		foreach(Flock flock in mFlocks) {
			Vector2 towardsCenter, awayFromOthers, matchVelocity;
			
			flockComputeVelocities(flock, out towardsCenter, out awayFromOthers, out matchVelocity);
			
			flock.velocity += towardsCenter + awayFromOthers + matchVelocity;
			
			M8.Math.Limit(ref flock.velocity, flock.maxSpeed);
			
			Vector3 pos = flock.transform.localPosition;
			
			/*Vector2 dPos = flock.velocity*Time.deltaTime;
			
			//check for collision
			
			pos.x += dPos.x;
			pos.y += dPos.y;
			
			flock.transform.localPosition = pos;*/
			flock.rigidbody.velocity = flock.velocity;
		}
	}
	
	void OnDrawGizmosSelected() {
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireSphere(transform.position, radius);
	}
	
	private void flockComputeVelocities(Flock flock, out Vector2 towardsCenter, out Vector2 awayFromOthers, out Vector2 matchVelocity) {
		Vector2 flockPos = flock.transform.localPosition;
		
		Vector2 center = Vector2.zero;
		Vector2 avgVel = Vector2.zero;
		
		awayFromOthers = Vector2.zero;
		
		foreach(Flock otherFlock in mFlocks) {
			if(otherFlock != flock) {
				Vector2 otherPos = otherFlock.transform.localPosition;
				
				center += otherPos;
				
				Vector2 dPos = otherPos - flockPos;
				float distSq = dPos.sqrMagnitude;
				
				//if(distSq < mDistanceMatchFactorSq) {
					avgVel += otherFlock.velocity;
				//}
				
				if(distSq < mDistanceSq) {
					awayFromOthers -= dPos;
				}
			}
		}
		
		float avgFactor = (float)(mFlocks.Count-1);
		
		center /= avgFactor;
		
		towardsCenter = (center - flockPos) * cohesionFactor;
		
		avgVel /= avgFactor;
		
		matchVelocity = (avgVel - flock.velocity) * alignVelocityFactor;
	}
}
