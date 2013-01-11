using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ActionTarget : MonoBehaviour {
	public enum Type {
		Follow, //normally attach the action's target to a unit
		Move,
		Disperse,
		Attack, //normally attach the action's target to a unit (or maybe not)
		
		NumType
	}
	
	public enum Priority {
		Highest,
		High,
		Normal,
		Low
	}
	
	public const int Unlimited = -1;
	
	public Type type;
	public Priority priority = Priority.Normal;
	public int limit = Unlimited; //-1 is no limit for who can perform this action within the region
	
	public Transform target;
	
	public bool stopOnExit = false;
	
	public bool startSensorOff = false; //turn off sensor at start
	
	private HashSet<ActionListener> mListeners = new HashSet<ActionListener>();
	
	private Collider mSensor;
	
	public bool sensorOn {
		get { return mSensor == null ? false : mSensor.enabled; }
		
		set {
			if(mSensor != null) {
				mSensor.enabled = value;
				
				//remove listeners
				if(!value) {
					StopAction();
				}
			}
		}
	}
	
	public bool vacancy {
		get {
			return limit == Unlimited || mListeners.Count < limit;
		}
	}
	
	public int numListeners {
		get { return mListeners.Count; }
	}
	
	//when we want to finish action
	public void StopAction() {
		//remove listeners
		if(mListeners.Count > 0) {
			//call exit for each
			ActionListener[] listeners = new ActionListener[mListeners.Count]; 
			mListeners.CopyTo(listeners);
			
			for(int i = 0; i < listeners.Length; i++) {
				listeners[i].StopAction(Priority.Highest);
			}
			
			mListeners.Clear();
		}
	}
	
	//called by ActionListener during OnTriggerEnter if we are valide
	public void AddListener(ActionListener listener) {
		mListeners.Add(listener);
	}
	
	//called by ActionListener when we stop action
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
