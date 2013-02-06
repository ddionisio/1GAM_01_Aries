using UnityEngine;
using System.Collections;

//attach with a rigid body
public class ActionListener : MonoBehaviour {
    public delegate void Callback(ActionListener listen);
    public delegate void CollisionCallback(ActionListener listen, ContactPoint info);

    public event Callback enterCallback;
    public event CollisionCallback hitEnterCallback;
    public event Callback hitExitCallback;
    public event Callback finishCallback;

    private ActionTarget mCurActionTarget = null;
    private Collider mCurActionCollider = null;
    private ActionTarget mDefaultActionTarget = null;
    private bool mLockAction = false;
    private ContactPoint mLastHitInfo;

    /// <summary>
    /// This is the contact point when we collided with the target
    /// </summary>
    public ContactPoint lastHitInfo {
        get { return mLastHitInfo; }
    }

    ///<summary> lock action, preventing action being set </summary>
    public bool lockAction {
        get { return mLockAction; }

        set {
            mLockAction = value;
        }
    }

    public ActionTarget.Priority currentPriority {
        get {
            //if current target is the default, then that is low
            return mCurActionTarget != null && mCurActionTarget != mDefaultActionTarget ?
                mCurActionTarget.priority : ActionTarget.Priority.Low;
        }
    }

    public Collider currentTargetCollider {
        get { return mCurActionCollider; }
    }

    //use this to manually set target (e.g. attacking with specific units)
    public ActionTarget currentTarget {
        get { return mCurActionTarget; }

        set {
            if(!mLockAction) {
                SetTarget(value);
            }
        }
    }

    //default target is when current stops, the default starts again and becomes current
    //also when current is null and you set default, it starts the action
    public ActionTarget defaultTarget {
        get { return mDefaultActionTarget; }

        set {
            if(value != null) {
                if(mDefaultActionTarget != value) {
                    //check if there's already a current action
                    //if there's a previous default, then clean that up
                    //otherwise, set this to also be current
                    if(mCurActionTarget != null) {
                        if(mDefaultActionTarget != null) {
                            mDefaultActionTarget.RemoveListener(this);
                        }
                    }
                    else {
                        ApplyToCurTarget(value);
                    }

                    mDefaultActionTarget = value;
                }
            }
            else {
                //clear out default, if it is also current, stop it as well
                if(mDefaultActionTarget != null) {
                    if(mDefaultActionTarget == mCurActionTarget) {
                        StopAction(ActionTarget.Priority.Highest, false);
                    }
                    else {
                        mDefaultActionTarget.RemoveListener(this);
                    }

                    mDefaultActionTarget = null;
                }
            }
        }
    }

    //get the action type, default: target's type
    public virtual ActionType type {
        get {
            return mCurActionTarget != null ? mCurActionTarget.type :
                 mDefaultActionTarget != null ? mDefaultActionTarget.type : ActionType.NumType;
        }
        set { }
    }

    //call this for spawning/release and activator wake-up/sleep
    public virtual void SetActive(bool activate) {
        if(activate) {
        }
        else {
            lockAction = false;
            currentTarget = null;
        }
    }

    //true if stopped or there was no action target
    public bool StopAction(ActionTarget.Priority priority, bool resumeDefault) {
        if(mCurActionTarget == null) {
            if(resumeDefault && mDefaultActionTarget != null) {
                ApplyToCurTarget(mDefaultActionTarget);
            }

            return true;
        }
        else if(mCurActionTarget == mDefaultActionTarget || mCurActionTarget.priority <= priority) {
            mCurActionTarget.RemoveListener(this);

            OnActionFinish();

            if(finishCallback != null) {
                finishCallback(this);
            }

            mCurActionTarget = null;
            mCurActionCollider = null;

            if(resumeDefault && mDefaultActionTarget != null) {
                ApplyToCurTarget(mDefaultActionTarget);
            }

            return true;
        }

        return false;
    }

    public virtual bool CheckRange() {
        return true;
    }

    public virtual bool CheckSpellRange() {
        return true;
    }

    protected virtual void OnActionEnter() {
    }

    protected virtual void OnActionExit() {
    }

    protected virtual void OnActionHitEnter(ContactPoint info) {
    }

    protected virtual void OnActionHitExit() {
    }

    protected virtual void OnActionFinish() {
    }

    protected virtual void OnDestroy() {
        defaultTarget = null;
        currentTarget = null;

        enterCallback = null;
        hitEnterCallback = null;
        hitExitCallback = null;
        finishCallback = null;
    }

    //if we are a rigid body and so is the target, these are called
    void OnCollisionEnter(Collision collision) {
        mLastHitInfo = collision.contacts[0];
        if(mLastHitInfo.otherCollider == mCurActionCollider) {
            OnActionHitEnter(mLastHitInfo);

            if(hitEnterCallback != null) {
                hitEnterCallback(this, mLastHitInfo);
            }
        }
    }

    //if we are a rigid body and so is the target, these are called
    void OnCollisionExit(Collision collision) {
        if(collision.collider == mCurActionCollider
            || collision.contacts.Length == 0
            || collision.contacts[0].otherCollider == mCurActionCollider) {

            OnActionHitExit();

            if(hitExitCallback != null) {
                hitExitCallback(this);
            }
        }
    }

    private void SetTarget(ActionTarget target) {
        if(target != null) {
            if(target != mCurActionTarget && target.vacancy) {
                //check if we currently have a target, then determine priority
                if(StopAction(target.priority, false)) {
                    ApplyToCurTarget(target);
                }
            }
        }
        else {
            StopAction(ActionTarget.Priority.Highest, true);
        }
    }

    private void ApplyToCurTarget(ActionTarget target) {
        target.AddListener(this);

        mCurActionTarget = target;
        mCurActionCollider = target.collider;

        OnActionEnter();

        if(enterCallback != null) {
            enterCallback(this);
        }
    }
}
