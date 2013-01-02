using UnityEngine;
using System.Collections;

public class ScreenTransition : MonoBehaviour {
	public enum State {
		In,
		Out,
		None,
		Done
	}
	
	public delegate void OnFinish(State state);
	
	public GUISkin skin;
	public float delay = 1.0f;
	public OnFinish finishCallback = null;
	
	private State mState = State.None;
	private float mCurTime = 0;
	
	public State state {
		get {
			return mState;
		}
		set {
			mCurTime = 0;
			mState = value;
			enabled = mState != State.None;
		}
	}
	
	//ew
	void OnGUI() {
		if(mState == State.None) {
		}
		else if(mState != State.Done) {
			mCurTime += Time.deltaTime;
			
			bool isDone = mCurTime >= delay;
			
			float t = isDone ? 1.0f : mCurTime/delay;
			
			Color c = Color.black;
			
			switch(mState) {
			case State.In:
				c.a = 1.0f - t;
				break;
				
			case State.Out:
				c.a = t;
				break;
			}
			
			GUI.skin = skin;
			GUI.color = c;
			
			GUI.Box(new Rect(0, 0, Screen.width, Screen.height), "", "blank");
			
			if(isDone) {
				if(finishCallback != null) {
					finishCallback(mState);
				}
				
				if(mState == State.Out)
					mState = State.Done;
				else {
					mState = State.None;
					enabled = false;
				}
			}
		}
		else {
			GUI.skin = skin;
			GUI.color = Color.black;
			
			GUI.Box(new Rect(0, 0, Screen.width, Screen.height), "", "blank");
		}
	}
	
	void Awake() {
		enabled = false;
	}
}
