using UnityEngine;
using System.Collections;

public class UnitSpriteController : MonoBehaviour {
	public enum Dir { E, N, W, S, NumDir }
	public enum ReverseDir { W, S, E, N, NumDir }
	
	public delegate void OnStateAnimComplete(UnitSpriteState state, Dir dir);
	public delegate void OnStateAnimEvent(UnitSpriteState state, Dir dir, EventData data);
	
	public struct EventData {
		public int valI;
		public float valF;
		public string valS;
		
		public EventData(int i, float f, string s) { valI = i; valF = f; valS = s; }
	}
		
	[System.Serializable]
	public class StateDir {
		public Dir dir; //append to name
		public bool horzFlipped;
		public bool vertFlipped;
	}
	
	[System.Serializable]
	public class StateData {
		public UnitSpriteState state;
		public string moveName;
		public string stopName;
		public bool sticky; //don't change state during update
		public StateDir[] dirs; //size = 0: omni-dir, size = 8: state per dir
	}
	
	public const float AngleToInd = ((float)Dir.NumDir)/M8.Math.TwoPI;
	public const float LameShift = (M8.Math.TwoPI/((float)Dir.NumDir))*0.5f;
	
	public tk2dAnimatedSprite sprite;
	public MotionBase mover;
	public float stopThreshold;
	
	//format example:
	// stopName: idle, dirs: [(E, false,false), (NE, false,false), (N, false,false), (NE, true,false), (E, true,false), (SE, true,false), (S, false,false), (SE, false,false)]
	// in sprite, there should be states: "idleE", "idleNE", "idleN", "idleS", "idleSE"
	public StateData[] states;
	
	public string defaultState; //state to change to if state is not found in states
	
	public event OnStateAnimComplete stateFinishCallback;
	public event OnStateAnimEvent stateEventCallback;
	
	private class AnimData {
		public int moveId;
		public int stopId;
		public bool horzFlipped = false;
		public bool vertFlipped = false;
		public bool sticky = false;
		
		public AnimData(tk2dAnimatedSprite spr, int defaultId, 
			string moveName, string stopName, Dir dir) {
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
		}
	}
					
	private AnimData[][] mAnim = new AnimData[(int)UnitSpriteState.NumState][]; //[UnitSpriteState][Dir]
	
	private UnitSpriteState mCurState = UnitSpriteState.NumState;
	private Dir mCurDir = Dir.E;
	
	private Vector2 mCurMoveDir = Vector2.zero;
	private bool mCurStopped = false;
	
	private int mDefaultStateId = 0;
	
	private bool mReverse = false;
	
	public UnitSpriteState state {
		get { return mCurState; }
		set {
			if(mCurState != value) {
				mCurState = value;
				ApplyCurState();
			}
		}
	}
	
	public bool reverse {
		get { return mReverse; }
		set {
			if(mReverse != value) {
				mReverse = value;
				sprite.ClipFps = mReverse ? Mathf.Abs(sprite.ClipFps)*-1.0f : Mathf.Abs(sprite.ClipFps);
				ApplyCurState();
			}
		}
	}
	
	public bool HasState(UnitSpriteState checkState) {
		return mAnim[(int)checkState] != null;
	}
	
	public void ClearCallbacks() {
		stateFinishCallback = null;
		stateEventCallback = null;
	}
	
	void OnDestroy() {
		ClearCallbacks();
		
		sprite.animationCompleteDelegate -= OnSpriteAnimComplete;
		sprite.animationEventDelegate -= OnSpriteAnimFrameEvent;
	}
	
	void Awake() {
		sprite.animationCompleteDelegate += OnSpriteAnimComplete;
		sprite.animationEventDelegate += OnSpriteAnimFrameEvent;
	}
	
	void Start () {
		mDefaultStateId = !string.IsNullOrEmpty(defaultState) ? sprite.GetClipIdByName(defaultState) : 0;
		
		foreach(StateData state in states) {
			int stateInd = (int)state.state;
			
			string moveName = state.moveName;
			string stopName = state.stopName;
			
			//omni dir
			if(state.dirs.Length == 0) {
				mAnim[stateInd] = new AnimData[1];
				mAnim[stateInd][0] = new AnimData(sprite, mDefaultStateId, moveName, stopName, Dir.NumDir);
				mAnim[stateInd][0].sticky = state.sticky;
			}
			else {
				mAnim[stateInd] = new AnimData[(int)Dir.NumDir];
				
				for(int i = 0; i < state.dirs.Length; i++) {
					StateDir stateDir = state.dirs[i];
					
					mAnim[stateInd][i] = new AnimData(sprite, mDefaultStateId, moveName, stopName, stateDir.dir);
					mAnim[stateInd][i].horzFlipped = stateDir.horzFlipped;
					mAnim[stateInd][i].vertFlipped = stateDir.vertFlipped;
					mAnim[stateInd][i].sticky = state.sticky;
				}
			}
		}
	}
	
	void Update () {
		AnimData curAnim = GetCurAnimData();
		if(!(curAnim == null || curAnim.sticky)) {
			//set dir
			
			//fuck this
			/*Vector2 moveDir = mover.body.velocity;
			
			mCurMoveDir = moveDir;
			
			float theta = Mathf.Atan2(mCurMoveDir.y, mCurMoveDir.x);
			if(theta < 0) {
				theta += M8.Math.TwoPI;
			}
			
			//theta -= LameShift;
									
			int newDir = theta < 0 ? 0 : Mathf.RoundToInt(theta*AngleToInd) % (int)Dir.NumDir;
			
			bool applyState = true;
			
			mCurDir = newDir;
			
			if((mover.curSpeed <= stopThreshold && !mCurStopped)
				|| (mover.curSpeed > stopThreshold && mCurStopped)) {
				applyState = true;
			}
			
			if(applyState) {
				ApplyCurState();
			}*/
			
			
			//well this is definitely cheaper
			mCurMoveDir = mover.dir;
			
			Dir prevDir = mCurDir;
			
			if(Mathf.Abs(mCurMoveDir.x) >= Mathf.Abs(mCurMoveDir.y)) {
				mCurDir = mCurMoveDir.x < 0.0f ? Dir.W : Dir.E;
			}
			else {
				mCurDir = mCurMoveDir.y < 0.0f ? Dir.S : Dir.N;
			}
			
			if(prevDir != mCurDir 
				|| (mover.curSpeed <= stopThreshold && !mCurStopped)
				|| (mover.curSpeed > stopThreshold && mCurStopped)) {
				ApplyCurState();
			}
			
			//determine animation speed based on move
			//if(mReverse) {
			//}
		}
	}
	
	void OnSpriteAnimComplete(tk2dAnimatedSprite spr, int clipId) {
		if(stateFinishCallback != null) {
			stateFinishCallback(mCurState, mCurDir);
		}
	}
	
	void OnSpriteAnimFrameEvent(tk2dAnimatedSprite sprite, tk2dSpriteAnimationClip clip, tk2dSpriteAnimationFrame frame, int frameNum) {
		if(stateEventCallback != null) {
			stateEventCallback(mCurState, mCurDir, new EventData(frame.eventInt, frame.eventFloat, frame.eventInfo));
		}
	}
	
	private AnimData GetCurAnimData() {
		AnimData[] animDirs = mCurState != UnitSpriteState.NumState ? mAnim[(int)mCurState] : null;
		if(animDirs != null) {
			if(animDirs.Length == 1) {
				return animDirs[0];
			}
			else {
				if(mReverse) {
					return animDirs[(int)((ReverseDir)mCurDir)];
				}
				else {
					return animDirs[(int)mCurDir];
				}
			}
		}
		
		return null;
	}
	
	private void ApplyCurState() {
		AnimData dat = GetCurAnimData();
		
		bool hFlip, vFlip;
		
		if(dat != null) {		
			mCurStopped = mover.curSpeed <= stopThreshold;
			
			sprite.Play(mCurStopped ? dat.stopId : dat.moveId);
			
			//flip, if only 1 dir, assume animation is facing right
			hFlip = mAnim[(int)mCurState].Length == 1 && mCurMoveDir.x < 0.0f ? !dat.horzFlipped : dat.horzFlipped;
			vFlip = dat.vertFlipped;
		}
		else {
			sprite.Play(mDefaultStateId);
			
			//assume animation is facing right
			hFlip = mCurMoveDir.x < 0.0f;
			vFlip = false;
		}
		
		Vector3 s = sprite.scale;
		s.x = Mathf.Abs(s.x);
		s.y = Mathf.Abs(s.y);
		
		if(hFlip) {
			s.x *= -1.0f;
		}
		
		if(vFlip) {
			s.y *= -1.0f;
		}
		
		sprite.scale = s;
	}
}
