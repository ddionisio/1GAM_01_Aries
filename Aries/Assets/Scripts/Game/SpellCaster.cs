using UnityEngine;
using System.Collections;

public class SpellCaster : MonoBehaviour {
	public delegate void Callback(SpellCaster caster);
	
	public enum State {
		None,
		Casting,
		Cooldown
	}
	
	public string spell;
	
	public event Callback castDoneCallback;
	
	private SpellConfig.Info mInfo;
	
	private State mState = State.None;
	private float mStartTime;
	private UnitEntity mTarget;
	
	public State state {
		get { return mState; }
	}
	
	public bool CanCastTo(UnitEntity target) {
		return mState == State.None && target != null && target.SpellCheck(mInfo.data);
	}
	
	public void CastTo(UnitEntity target) {
		if(target != null) {
			mState = State.Casting;
			mStartTime = Time.time;
			mTarget = target;
		}
	}
	
	public void Cancel() {
		mState = State.None;
		mTarget = null;
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
		case State.None:
			break;
			
		case State.Casting:
			if(Time.time - mStartTime >= mInfo.castDelay) {
				mTarget.SpellAdd(mInfo.data);
				mTarget = null;
				mStartTime = Time.time;
				mState = State.Cooldown;
				
				if(castDoneCallback != null) {
					castDoneCallback(this);
				}
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
