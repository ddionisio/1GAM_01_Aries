using UnityEngine;
using System.Collections;

//use this to have entities in the map be deactivated at start,
//waking up when certain things trigger it
//make sure to have this as a child,
//destroy it if we are currently active
[RequireComponent(typeof(Collider))]
public class EntityActivator : MonoBehaviour {
	public delegate void Callback();
	
	public bool deactivateOnStart = true;
	public float deactivateDelay = 2.0f;
	
	public event Callback awakeCallback;
	public event Callback sleepCallback;
			
	private bool mIsActive = true;
	private GameObject mParentGo;
	
	/// <summary>
	/// Initialize, call this when you are about to be re-added to the scene
	/// </summary>
	public void Start() {
		if(deactivateOnStart) {
			DoInActive(false);
		}
	}
	
	/// <summary>
	/// Call this when you are about to be released or destroyed
	/// </summary>
	public void Release(bool destroy) {
		if(!mIsActive) {
			//put ourself back in parent
			if(destroy) {
				Object.Destroy(gameObject);
			}
			else {
				//put back to parent
				transform.parent = mParentGo.transform;
			}
			
			CancelInvoke("InActiveDelay");
			mParentGo = null;
			mIsActive = true;
		}
	}
	
	void OnDestroy() {
		awakeCallback = null;
		sleepCallback = null;
	}
	
	void OnTriggerEnter(Collider c) {
		DoActive();
	}
	
	void OnTriggerExit(Collider c) {
		if(mIsActive) {
			Invoke("InActiveDelay", deactivateDelay);
		}
	}
	
	void DoActive() {
		if(!mIsActive) {
			//put ourself back in parent
			mParentGo.SetActive(true);
			transform.parent = mParentGo.transform;
			
			mParentGo = null;
			
			mIsActive = true;
			
			if(awakeCallback != null) {
				awakeCallback();
			}
		}
		else {
			CancelInvoke("InActiveDelay");
		}
	}
	
	void InActiveDelay() {
		DoInActive(true);
	}
	
	void DoInActive(bool notifySleep) {
		if(mIsActive) {
			if(notifySleep && sleepCallback != null) {
				sleepCallback();
			}
			
			//remove from parent
			mParentGo = transform.parent.gameObject;
			
			if(EntityManager.instance != null && EntityManager.instance.activatorHolder) {
				transform.parent = EntityManager.instance.activatorHolder;
			}
			else {
				transform.parent = null;
			}
			
			mParentGo.SetActive(false);
			
			mIsActive = false;
		}
	}
}
