using UnityEngine;
using System.Collections;

public class Player : EntityBase {
	public GameObject summonDisplay;
	public GameObject unsummonDisplay;
	
	public const int playerIndOfs = (int)FlockType.PlayerOneUnits;
	public const int playerCount = (int)(FlockType.PlayerFourUnits-FlockType.PlayerOneUnits)+1;
	private static Player[] mPlayers = new Player[playerCount];
	
	private PlayerController mControl;
	private UnitSpriteController mSprite;
	private PlayerStat mPlayerStats;
	
	public static Player GetPlayer(int index) {
		return mPlayers[index];
	}
			
	public PlayerStat stats {
		get { return mPlayerStats; }
	}
	
	public PlayerController control {
		get { return mControl; }
	}
	
	public override void Release() {
		mControl.CancelActions();
		mControl.CancelRecall();
		mControl.followAction.StopAction();
		
		base.Release();
	}
	
	protected override void OnDestroy () {
		mPlayers[(int)mPlayerStats.flockGroup - playerIndOfs] = null;
		
		mPlayerStats.statChangeCallback -= OnStatChange;
		
		base.OnDestroy();
	}
	
	protected override void Awake() {
		base.Awake();
		
		mControl = GetComponentInChildren<PlayerController>();
		mPlayerStats = GetComponentInChildren<PlayerStat>();
		mSprite = GetComponentInChildren<UnitSpriteController>();
		
		mPlayerStats.statChangeCallback += OnStatChange;
		//hoook to hud
		
		mPlayers[(int)mPlayerStats.flockGroup - playerIndOfs] = this;
		
		if(summonDisplay != null) summonDisplay.SetActive(false);
		if(unsummonDisplay != null) unsummonDisplay.SetActive(false);
	}

	// Use this for initialization
	protected override void Start () {
		base.Start();
		
		CameraController camCtrl = CameraController.instance;
		camCtrl.attachTo = transform;
		
		mPlayerStats.InitResource();
	}
	
	protected override void StateChanged() {
		switch(prevState) {
		case EntityState.castSummon:
			if(summonDisplay != null) summonDisplay.SetActive(false);
			break;
			
		case EntityState.castUnSummon:
			
			if(unsummonDisplay != null) unsummonDisplay.SetActive(false);
			break;
		}
		
		switch(state) {
		case EntityState.spawning:
			mPlayerStats.InitResource();
			break;
			
		case EntityState.normal:
			EnableControls();
			
			if(mSprite != null)
				mSprite.state = UnitSpriteState.Move;
			break;
			
		case EntityState.castSummon:
			//start fx
			if(mSprite != null)
				mSprite.state = UnitSpriteState.Casting;
			
			if(summonDisplay != null) summonDisplay.SetActive(true);
			break;
			
		case EntityState.castUnSummon:
			//start fx
			if(mSprite != null)
				mSprite.state = UnitSpriteState.Casting;
			
			if(unsummonDisplay != null) unsummonDisplay.SetActive(true);
			break;
			
		case EntityState.attacking:
			if(mSprite != null)
				mSprite.state = UnitSpriteState.AttackPursue;
			break;
					
		case EntityState.dying:
			if(mSprite != null)
				mSprite.state = UnitSpriteState.Die;
			
			//gameover
			break;
		}
	}
	
	protected override void SpawnStart () {
		state = EntityState.normal;
		
		mControl.SpawnStart();
		
		if(summonDisplay != null) summonDisplay.SetActive(false);
		if(unsummonDisplay != null) unsummonDisplay.SetActive(false);
	}
	
	void LateUpdate () {
	
	}
	
	void OnStatChange(StatBase stat) {
		if(stat.curHP == 0.0f) {
			DisableControls();
			state = EntityState.dying;
			Debug.Log("dead");
		}
	}
	
	void OnUIModalActive() {
		DisableControls();
	}
	
	void OnUIModalInactive() {
		if(state != EntityState.dying) {
			EnableControls();
		}
	}
	
	private void DisableControls() {
		mControl.CancelActions();
		mControl.body.velocity = Vector3.zero;
		mControl.enabled = false;
	}
	
	private void EnableControls() {
		mControl.enabled = true;
	}
}
