using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ActionTarget : MonoBehaviour {
	public ActionInfo action;
	
	public Transform target;
	
	public bool startSensorOff = false; //turn off sensor at start
	
	private HashSet<ActionListener> mListeners = new HashSet<ActionListener>();
	
	private Collider mSensor;
	
	public bool sensorOn {
		get { return mSensor == null ? false : mSensor.enabled; }
		
		set {
			if(mSensor != null) {
				mSensor.enabled = value;
				
				//remove listeners
				if(!value && mListeners.Count > 0) {
					//call exit for each
					ActionListener[] listeners = new ActionListener[mListeners.Count]; 
					mListeners.CopyTo(listeners);
					
					for(int i = 0; i < listeners.Length; i++) {
						listeners[i].StopAction();
					}
					
					mListeners.Clear();
				}
			}
		}
	}
	
	public bool vacancy {
		get {
			return action.limit == ActionInfo.Unlimited || mListeners.Count < action.limit;
		}
	}
	
	public int numListeners {
		get { return mListeners.Count; }
	}
	
	//called by ActionListener during OnTriggerEnter if we are valide
	public void AddListener(ActionListener listener) {
		mListeners.Add(listener);
	}
	
	//called by ActionListener during OnTriggerExit
	public void RemoveListener(ActionListener listener) {
		mListeners.Remove(listener);
	}
	
	void Awake() {
		mSensor = collider;
	}
	
	void Start() {
		if(startSensorOff && mSensor != null) {
			mSensor.enabled = false;
		}
	}
}
