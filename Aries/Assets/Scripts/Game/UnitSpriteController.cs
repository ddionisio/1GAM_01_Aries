using UnityEngine;
using System.Collections;

public class UnitSpriteController : MonoBehaviour {
	public enum Dir { E, NE, N, NW, W, SW, S, SE, NumDir }
	
	public enum State {
		Idle,
		Move,
		
		NumStates
	}
	
	[System.Serializable]
	public class StateDir {
		public Dir dir; //append to name
		public bool isFlipped;
	}
	
	[System.Serializable]
	public class StateData {
		public State moveState;
		public State stopState;
		public StateDir[] dirs; //size = 0: omni-dir, size = 8: state per dir
	}
	
	public const float RadToDir = ((float)Dir.NumDir)/(2.0f*Mathf.PI);
	
	public tk2dAnimatedSprite sprite;
	public MotionBase mover;
	public float stopThreshold;
	public float speedTurnThreshold;
	
	//format example:
	// stopState: Idle, dirs: [(E, false), (NE, false), (N, false), (NE, true), (E, true), (SE, true), (S, false), (SE, false)]
	// in sprite, there should be states: "IdleE", "IdleNE", "IdleN", "IdleS", "IdleSE"
	public StateData[] states;
	
	public string defaultState; //state to change to if state is not found in states
	
	private class AnimData {
		public int moveId;
		public int stopId;
		public bool flipped;
		
		public AnimData(tk2dAnimatedSprite spr, string moveName, string stopName, Dir dir, bool aFlipped) {
			//omni dir
			if(dir == Dir.NumDir) {
				moveId = spr.GetClipIdByName(moveName);
				stopId = spr.GetClipIdByName(stopName);
			}
			else {
				string nameDir = dir.ToString();
				
				moveId = spr.GetClipIdByName(moveName + nameDir);
				stopId = spr.GetClipIdByName(stopName + nameDir);
			}
			
			flipped = aFlipped;
		}
	}
					
	private AnimData[][] mAnim;
	private int mCurState = 0;
	
	private int mCurDir = (int)Dir.E;
	private int mDefaultId = 0;
	private Vector2 mCurMoveDir = Vector2.zero;
	
	public int state {
		get { return mCurState; }
		set {
			if(mCurState != value) {
				mCurState = value;
				ApplyCurState();
			}
		}
	}
	
	void Awake() {
	}
	
	void Start () {
		mDefaultId = !string.IsNullOrEmpty(defaultState) ? sprite.GetClipIdByName(defaultState) : 0;
		
		mAnim = new AnimData[states.Length][];
		
		for(int stateInd = 0; stateInd < states.Length; stateInd++) {
			StateData state = states[stateInd];
			
			string moveName = state.moveState.ToString();
			string stopName = state.stopState.ToString();
			
			//omni dir
			if(state.dirs.Length == 0) {
				mAnim[stateInd] = new AnimData[1];
				mAnim[stateInd][0] = new AnimData(sprite, moveName, stopName, Dir.NumDir, false);
			}
			else {
				mAnim[stateInd] = new AnimData[(int)Dir.NumDir];
				
				for(int i = 0; i < state.dirs.Length; i++) {
					StateDir stateDir = state.dirs[i];
					
					mAnim[stateInd][i] = new AnimData(sprite, moveName, stopName, (Dir)i, stateDir.isFlipped);
				}
			}
		}
	}
	
	void Update () {
		//set dir
		if(mover.curSpeed >= speedTurnThreshold) {
			Vector2 moveDir = mover.dir;
			if(moveDir != mCurMoveDir) {
				mCurMoveDir = moveDir;
				
				//get dir index
				float theta = Mathf.Atan2(moveDir.y, moveDir.x);
				if(theta < 0) {
					theta += M8.Math.TwoPI;
				}
				
				int newDir = Mathf.RoundToInt(theta*RadToDir);
				if(mCurDir != newDir) {
					mCurDir = newDir;
					ApplyCurState();
				}
			}
		}
	}
	
	private void ApplyCurState() {
		AnimData[] animDirs = mAnim[mCurState];
		
		int id = mDefaultId;
		bool flipped = false;
		
		//determine id
		if(animDirs != null) {
			if(animDirs.Length == 1) {
				AnimData dat = animDirs[0];
				id = mover.curSpeed <= stopThreshold ? dat.stopId : dat.moveId;
			}
			else {
				AnimData dat = animDirs[mCurDir];
				id = mover.curSpeed <= stopThreshold ? dat.stopId : dat.moveId;
				flipped = dat.flipped;
			}
		}
		
		sprite.Play(id);
		
		//flip
		Vector3 s = sprite.scale;
		s.x = Mathf.Abs(s.x);
		if(flipped) {
			s.x *= -1.0f;
		}
		
		sprite.scale = s;
	}
}
