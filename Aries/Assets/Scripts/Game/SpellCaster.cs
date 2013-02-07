using UnityEngine;
using System.Collections;

public class SpellCaster : MonoBehaviour {
	public delegate void Callback(SpellCaster caster);
	
	public enum State {
        Inactive,
		None,
		Casting,
		Cooldown
	}
	
	public string spell;
	
	public event Callback castDoneCallback;
	
	private SpellConfig.Info mInfo;

    private State mState = State.Inactive;
	private float mStartTime;
	private UnitEntity mTarget;
	
	public State state {
		get { return mState; }
	}

    public void Ready() {
        if(mState == State.Inactive)
            mState = State.None;
    }
	
	public bool CanCastTo(UnitEntity target) {
		return mState == State.None && target != null && target.SpellCheck(mInfo.data);
	}
	
	public void CastTo(UnitEntity target) {
		if(target != null && !target.isReleased) {
			mState = State.Casting;
			mStartTime = Time.time;
			mTarget = target;
		}
	}
	
    /// <summary>
    /// If disable is true, make sure to call Ready again later. This prevents spell casting.
    /// </summary>
	public void Cancel(bool disable) {
		if(mTarget != null) {
            mState = disable ? State.Inactive : State.None;
			mTarget = null;
		}
	}
	
	public void ClearCallbacks() {
		castDoneCallback = null;
	}
	
	void OnDestroy() {
		ClearCallbacks();
	}
	
	void Awake() {
	}

	// Use this for initialization
	void Start () {
		mInfo = SpellConfig.GetInfo(spell);
	}
	
	// Update is called once per frame
	void Update () {
		switch(mState) {
        case State.Inactive:
		case State.None:
			break;
			
		case State.Casting:
			if(Time.time - mStartTime >= mInfo.castDelay) {
				if(mTarget != null && !mTarget.isReleased) {
					mTarget.SpellAdd(mInfo.data);
					mTarget = null;
										
					if(castDoneCallback != null) {
						castDoneCallback(this);
					}
				}
				
				mStartTime = Time.time;
				mState = State.Cooldown;
			}
			break;
			
		case State.Cooldown:
			if(Time.time - mStartTime >= mInfo.cooldown) {
				mState = State.None;
			}
			break;
		}
	}
}
