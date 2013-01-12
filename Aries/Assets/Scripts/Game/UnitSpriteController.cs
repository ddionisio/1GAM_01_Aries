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
		public State state;
		public StateDir[] dirs; //size = 0: omni-dir, size = 8: state per dir
	}
	
	public tk2dAnimatedSprite sprite;
	public Motion motion;
	public float speedIdleThreshold;
	public float speedTurnThreshold;
	
	//format example:
	// move: state: Idle, dirs: [(E, false), (NE, false), (N, false), (NE, true), (E, true), (SE, true), (S, false), (SE, false)]
	// in sprite, there should be states: "IdleE", "IdleNE", "IdleN", "IdleS", "IdleSE"
	public StateData[] states;
	
	public string defaultState; //state to change to if state is not found in states
	
	private struct AnimData {
		public int id;
		public bool flipped;
	}
					
	private AnimData[,] mAnim = new AnimData[(int)State.NumStates, (int)Dir.NumDir];
	private State mCurState = State.Idle;
	private Dir mCurDir = Dir.E;
	
	void Awake() {
	}
	
	void Start () {
		int defaultId = !string.IsNullOrEmpty(defaultState) ? sprite.GetClipIdByName(defaultState) : 0;
		
		foreach(StateData state in states) {
			string name = state.state.ToString();
			
			for(int i = 0; i < state.dirs.Length; i++) {
				StateDir stateDir = state.dirs[i];
				
				string nameDir = name + stateDir.dir.ToString();
				
				int id = sprite.GetClipIdByName(nameDir);
				int stateInd = (int)state.state;
				
				mAnim[stateInd, i].id = id >= 0 ? id : defaultId;
				mAnim[stateInd, i].flipped = stateDir.isFlipped;
			}
		}
	}
	
	void Update () {
		
	}
	
	void OnActionEnter(ActionTarget target) {
	}
	
	void OnActionFinish(ActionTarget target) {
	}
}
