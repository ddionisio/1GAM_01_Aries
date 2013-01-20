using UnityEngine;
using System.Collections;

public class AIState : SequencerInstance {
}

public class Enemy : UnitEntity {
	public string startAIState;
	
	public bool noSpawn;
	
	private AIState mAIStateInstance = null;
	private string mAICurState;
	
	private bool mAIStarted = false;
	private bool mSpawnFinished = false;
	//private string mLastAIState;
	
	//AI
	public void AIStop() {
		if(mAIStateInstance != null) {
			mAIStateInstance.terminate = true;
			mAIStateInstance = null;
			
			mAICurState = null;
		}
	}
	
	public void AISetPause(bool pause) {
		if(mAIStateInstance != null) {
			mAIStateInstance.pause = pause;
		}
	}
	
	public void AISetState(string state) {
		if(mAICurState != state) {
			AIStop();
			
			mAICurState = state;
			
			if(!string.IsNullOrEmpty(mAICurState)) {
				mAIStateInstance = new AIState();
				AIManager.instance.states.Start(this, mAIStateInstance, state);
			}
		}
	}
	
	public void AIRestart() {
		if(!string.IsNullOrEmpty(mAICurState)) {
			AISetState(mAICurState);
		}
	}
	
	void SequenceChangeState(string state) {
		AISetState(state);
	}
	
	public string aiCurState {
		get {
			return mAICurState;
		}
	}
	
	public bool aiActive {
		get {
			return mAIStateInstance != null;
		}
	}
	
	protected override void StateChanged() {
		base.StateChanged();
		
		switch(prevState) {
		case EntityState.spawning:
			//finished spawning
			mSpawnFinished = true;
			AIInit();
			break;
		}
		
		switch(state) {
		case EntityState.dying:
			AIStop();
			break;
		}
	}
	
	public override void Release() {
		AIStop();
		
		mAIStarted = false;
		mSpawnFinished = false;
		
		base.Release();
	}
	
	protected override void ActivatorWakeUp() {
		base.ActivatorWakeUp();
		
		if(mSpawnFinished) {
			AIInit();
		}
	}
	
	protected override void ActivatorSleep() {
		base.ActivatorSleep();
		
		AIStop();
		
		mAIStarted = false;
	}
	
	protected override void Start () {
		base.Start();
		
		if(noSpawn) {
			AIInit();
			mSpawnFinished = true;
		}
	}
	
	private void AIInit() {
		if(!mAIStarted && !string.IsNullOrEmpty(startAIState)) {
			AISetState(startAIState);
			mAIStarted = true;
		}
	}
}
