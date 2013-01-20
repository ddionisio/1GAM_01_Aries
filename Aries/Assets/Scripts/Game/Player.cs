using UnityEngine;
using System.Collections;

public class Player : EntityBase {
	
	private PlayerController mControl;
	private UnitSpriteController mSprite;
	private PlayerStat mPlayerStats;
	
	public PlayerStat stats {
		get { return mPlayerStats; }
	}
	
	public override void Release() {
		mControl.CancelActions();
		mControl.followAction.StopAction();
		
		base.Release();
	}
	
	protected override void OnDestroy () {
		mPlayerStats.hpChangeCallback -= OnHPChange;
		
		base.OnDestroy();
	}
	
	protected override void Awake() {
		base.Awake();
		
		mControl = GetComponentInChildren<PlayerController>();
		mPlayerStats = GetComponentInChildren<PlayerStat>();
		mSprite = GetComponentInChildren<UnitSpriteController>();
		
		mPlayerStats.hpChangeCallback += OnHPChange;
		//hoook to hud
	}

	// Use this for initialization
	protected override void Start () {
		base.Start();
		
		CameraController camCtrl = CameraController.instance;
		camCtrl.attachTo = transform;
		
		mPlayerStats.InitResource();
	}
	
	protected override void StateChanged() {
		switch(state) {
		case EntityState.spawning:
			mPlayerStats.InitResource();
			break;
			
		case EntityState.normal:
			mSprite.state = UnitSpriteState.Move;
			break;
			
		case EntityState.castSummon:
			//start fx
			mSprite.state = UnitSpriteState.Casting;
			break;
			
		case EntityState.castUnSummon:
			//start fx
			mSprite.state = UnitSpriteState.Casting;
			break;
			
		case EntityState.attacking:
			mSprite.state = UnitSpriteState.AttackPursue;
			break;
					
		case EntityState.dying:
			mSprite.state = UnitSpriteState.Die;
			
			//gameover
			break;
		}
	}
	
	protected override void SpawnStart () {
		state = EntityState.normal;
	}
	
	void LateUpdate () {
	
	}
	
	void OnHPChange(StatBase stat, float delta) {
		if(stat.curHP == 0.0f) {
			state = EntityState.dying;
		}
	}
}
