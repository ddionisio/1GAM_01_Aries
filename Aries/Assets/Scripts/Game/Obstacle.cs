using UnityEngine;
using System.Collections;

public class Obstacle : EntityBase {
	public bool updatePathing = false;
	
	private tk2dAnimatedSprite mSprite;
	private StatBase mStats;
	private ActionTarget mActTarget;
	private bool mPathCreated = true;
	
	public tk2dAnimatedSprite sprite { get { return mSprite; } }
	public StatBase stats { get { return mStats; } }
	public ActionTarget actionTarget { get { return mActTarget; } }

	protected override void Awake() {
		base.Awake();
		
		mSprite = GetComponentInChildren<tk2dAnimatedSprite>();
		mStats = GetComponentInChildren<UnitStat>();
		mActTarget = GetComponentInChildren<ActionTarget>();
		
		//hook calls up
		mStats.hpChangeCallback += OnHPChange;
	}
	
	// Use this for initialization
	protected override void Start() {
		base.Start();
		
		//stuff
	}
	
	public override void Release() {
		//clear out path blocks
		if(updatePathing && mPathCreated) {
			mPathCreated = false;
		}
		
		ClearData();
		
		mStats.ResetStats();
						
		base.Release();
	}
		
	protected override void StateChanged() {
		switch(state) {
		case EntityState.spawning:
			//block path
			if(updatePathing) {
				mPathCreated = true;
			}
			
			//spawn started
			break;
			
		case EntityState.dying:
			//clear out path blocks
			if(updatePathing && mPathCreated) {
				mPathCreated = false;
			}
			
			if(mActTarget != null) {
				mActTarget.StopAction();
			}
			break;
		}
	}
	
	protected override void SpawnFinish() {
		state = EntityState.normal;
	}
	
	protected override void SetBlink(bool blink) {
	}
	
	protected override void OnDestroy() {
		ClearData();
		
		if(mStats != null) {
			mStats.hpChangeCallback -= OnHPChange;
		}
		
		base.OnDestroy();
	}
	
	// Update is called once per frame
	/*void LateUpdate () {
		switch(state) {
		}
	}*/
	
	
	void OnHPChange(StatBase stat, float delta) {
		Debug.Log("ouch");
		if(stat.curHP == 0.0f) {
			state = EntityState.dying;
		}
	}
			
	private void ClearData() {
		if(mActTarget != null) {
			mActTarget.StopAction();
		}
	}
}
