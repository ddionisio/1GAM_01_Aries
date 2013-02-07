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

    [SerializeField]
    GameObject indicator; //starts as turned off
    [SerializeField]
    GameObject targetted; //starts as turned off, on when there are listeners

    [SerializeField]
    Transform targetTo;

    private HashSet<ActionListener> mListeners = new HashSet<ActionListener>();
    private bool mLockTargetted = false;

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

    /// <summary>
    /// Lock the targetted display to active.
    /// </summary>
    public bool lockTargetted {
        get { return mLockTargetted; }
        set {
            if(mLockTargetted != value) {
                mLockTargetted = value;

                if(targetted != null) {
                    targetted.SetActive(mLockTargetted || mListeners.Count > 0);
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
        //Debug.Log("stopping action of: "+gameObject.name);

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
                    listeners[i].StopAction(Priority.Highest, true);
                }
            }

            mListeners.Clear();
        }

        if(targetted != null && !mLockTargetted) {
            targetted.SetActive(false);
        }
    }

    //called by ActionListener during OnTriggerEnter if we are valide
    public void AddListener(ActionListener listener) {
        //Debug.Log("listener added: "+listener.gameObject.name);

        mListeners.Add(listener);

        if(targetted != null) {
            targetted.SetActive(true);
        }
    }

    //called by ActionListener when we stop action
    public void RemoveListener(ActionListener listener) {
        //Debug.Log("listener removed: "+listener.gameObject.name);

        mListeners.Remove(listener);

        if(targetted != null && !mLockTargetted) {
            targetted.SetActive(mListeners.Count > 0);
        }
    }

    void OnDestroy() {
        StopAction();
    }

    void Awake() {
        if(indicator != null) {
            indicator.SetActive(false);
        }

        if(targetted != null) {
            targetted.SetActive(false);
        }
    }

    void Start() {
    }
}
