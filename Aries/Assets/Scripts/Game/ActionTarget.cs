using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ActionTarget : MonoBehaviour {
	public enum Priority {
		Low,
		Normal,
		High,
		Highest
	}
	
	public const int Unlimited = -1;
	
	public ActionType type;
	public Priority priority = Priority.Normal;
	public int limit = Unlimited; //-1 is no limit for who can perform this action within the region
	
	private HashSet<ActionListener> mListeners = new HashSet<ActionListener>();
	
	[SerializeField] GameObject indicator; //starts as turned off
	[SerializeField] Transform targetTo;
	
	public Transform target {
		get { return targetTo == null ? transform : targetTo; }
	}
	
	public bool indicatorOn {
		get { return indicator != null ? indicator.activeSelf : false; }
		set {
			if(indicator != null) {
				indicator.SetActive(value);
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
				if(listeners[i].defaultTarget == this) {
					listeners[i].defaultTarget = null;
				}
				else if(listeners[i].currentTarget == this) {
					listeners[i].currentTarget = null;
				}
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
	
	void OnDestroy() {
		StopAction();
	}
	
	void Awake() {
		if(indicator != null) {
			indicator.SetActive(false);
		}
	}
	
	void Start() {
	}
}
