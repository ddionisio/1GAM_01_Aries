using UnityEngine;
using System.Collections;

public class UnitSpriteController : MonoBehaviour {
	public enum Dir { E, NE, N, NW, W, SW, S, SE, NumDir }
		
	[System.Serializable]
	public class StateDir {
		public Dir dir; //append to name
		public bool horzFlipped;
		public bool vertFlipped;
	}
	
	[System.Serializable]
	public class StateData {
		public string moveName;
		public string stopName;
		public StateDir[] dirs; //size = 0: omni-dir, size = 8: state per dir
	}
	
	public const float AngleToInd = ((float)Dir.NumDir)/M8.Math.TwoPI;
	public const float AngleRes = 5.0f*Mathf.Deg2Rad;
	public const float LameShift = (M8.Math.TwoPI/((float)Dir.NumDir))*0.5f;
	
	public tk2dAnimatedSprite sprite;
	public MotionBase mover;
	public float stopThreshold;
	
	//format example:
	// stopName: idle, dirs: [(E, false,false), (NE, false,false), (N, false,false), (NE, true,false), (E, true,false), (SE, true,false), (S, false,false), (SE, false,false)]
	// in sprite, there should be states: "idleE", "idleNE", "idleN", "idleS", "idleSE"
	public StateData[] states;
	
	public string defaultState; //state to change to if state is not found in states
	
	private class AnimData {
		public int moveId;
		public int stopId;
		public bool horzFlipped;
		public bool vertFlipped;
		
		public AnimData(tk2dAnimatedSprite spr, int defaultId, 
			string moveName, string stopName, 
			Dir dir, 
			bool aHorzFlipped, bool aVertFlipped) {
			//omni dir
			if(dir == Dir.NumDir) {
				moveId = spr.GetClipIdByName(moveName);
				if(moveId < 0) {
					moveId = defaultId;
				}
				
				stopId = spr.GetClipIdByName(stopName);
				if(stopId < 0) {
					stopId = defaultId;
				}
			}
			else {
				string nameDir = dir.ToString();
				
				moveId = spr.GetClipIdByName(moveName + nameDir);
				if(moveId < 0) {
					moveId = defaultId;
				}
				
				stopId = spr.GetClipIdByName(stopName + nameDir);
				if(stopId < 0) {
					stopId = defaultId;
				}
			}
			
			horzFlipped = aHorzFlipped;
			vertFlipped = aVertFlipped;
		}
	}
					
	private AnimData[][] mAnim;
	private int mCurState = 0;
	
	private int mCurDir = (int)Dir.E;
	private Vector2 mCurMoveDir = Vector2.zero;
	private bool mCurStopped = false;
	
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
		int defaultId = !string.IsNullOrEmpty(defaultState) ? sprite.GetClipIdByName(defaultState) : 0;
		
		mAnim = new AnimData[states.Length][];
		
		for(int stateInd = 0; stateInd < states.Length; stateInd++) {
			StateData state = states[stateInd];
			
			string moveName = state.moveName;
			string stopName = state.stopName;
			
			//omni dir
			if(state.dirs.Length == 0) {
				mAnim[stateInd] = new AnimData[1];
				mAnim[stateInd][0] = new AnimData(sprite, defaultId, moveName, stopName, Dir.NumDir, false, false);
			}
			else {
				mAnim[stateInd] = new AnimData[(int)Dir.NumDir];
				
				for(int i = 0; i < state.dirs.Length; i++) {
					StateDir stateDir = state.dirs[i];
					
					mAnim[stateInd][i] = new AnimData(sprite, defaultId, moveName, stopName, stateDir.dir, stateDir.horzFlipped, stateDir.vertFlipped);
				}
			}
		}
	}
	
	void Update () {
		//set dir
		Vector2 moveDir = mover.dir;
		
		//get dir index
		if((moveDir - mCurMoveDir).sqrMagnitude > 0.01f) {
			mCurMoveDir = moveDir;
			
			float theta = Mathf.Atan2(mCurMoveDir.y, mCurMoveDir.x);
			if(theta < 0) {
				theta += M8.Math.TwoPI;
			}
			
			theta -= LameShift;
									
			int newDir = theta < 0 ? 0 : Mathf.RoundToInt(theta*AngleToInd) % (int)Dir.NumDir;
			
			if(mCurDir != newDir) {
				mCurDir = newDir;
				ApplyCurState();
			}
		}
		else if((mover.curSpeed <= stopThreshold && !mCurStopped)
			|| (mover.curSpeed > stopThreshold && mCurStopped)) {
			ApplyCurState();
		}
	}
	
	private void ApplyCurState() {
		AnimData[] animDirs = mAnim[mCurState];
		
		AnimData dat = animDirs.Length == 1 ? animDirs[0] : animDirs[mCurDir];
						
		mCurStopped = mover.curSpeed <= stopThreshold;
		
		sprite.Play(mCurStopped ? dat.stopId : dat.moveId);
		
		//flip
		Vector3 s = sprite.scale;
		s.x = Mathf.Abs(s.x);
		s.y = Mathf.Abs(s.y);
		
		if(dat.horzFlipped) {
			s.x *= -1.0f;
		}
		
		if(dat.vertFlipped) {
			s.y *= -1.0f;
		}
		
		sprite.scale = s;
	}
}
