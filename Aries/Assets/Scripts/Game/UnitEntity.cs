using UnityEngine;
using System.Collections;

public class UnitEntity : EntityBase {
	public UnitAttackType attackType;
	
	public float unSummonDelay = 0.5f;
			
	private UnitStat mStats;
	private FlockUnit mFlockUnit;
	private ActionListener mListener;
	private ActionTarget mActTarget;
	private UnitSpriteController mSpriteControl;
	
	private Weapon mWeapon;
	private Weapon.RepeatParam mWeaponParam;
	
	private float mAttackCosTheta;
	
	public UnitStat stats { get { return mStats; } }
	public FlockUnit flockUnit { get { return mFlockUnit; } }
	public ActionListener listener { get { return mListener; } }
	public ActionTarget actionTarget { get { return mActTarget; } }
	public UnitSpriteController spriteControl { get { return mSpriteControl; } }
	public Weapon weapon { get { return mWeapon; } }
			
	protected override void Awake() {
		base.Awake();
										
		mStats = GetComponentInChildren<UnitStat>();
		mFlockUnit = GetComponentInChildren<FlockUnit>();
		mListener = GetComponentInChildren<ActionListener>();
		mActTarget = GetComponentInChildren<ActionTarget>();
		mSpriteControl = GetComponentInChildren<UnitSpriteController>();
		mWeapon = GetComponentInChildren<Weapon>();
		
		//hook calls up
		mStats.statChangeCallback += OnStatChange;
		
		if(mFlockUnit != null) {
			mFlockUnit.groupMoveEnabled = false;
		}
		
		if(mWeapon != null) {
			mWeaponParam = new Weapon.RepeatParam();
			mWeaponParam.source = transform;
		}
		
		if(mListener != null) {
			mListener.enterCallback += OnActionEnter;
			mListener.hitEnterCallback += OnActionHitEnter;
			mListener.hitExitCallback += OnActionHitExit;
			mListener.finishCallback += OnActionFinish;
		}
	}
	
	// Use this for initialization
	protected override void Start() {
		base.Start();
	}
	
	public override void Release() {
		ClearData();
		
		mStats.ResetStats();
		
		base.Release();
	}
	
	public override void SpawnFinish() {
		FlockUnitInit();
		
		if(mListener != null) {
			mListener.SetActive(true);
		}
	}
	
	protected override void ActivatorWakeUp() {
		if(!doSpawnOnWake) {
			SpawnFinish();
		}
		
		base.ActivatorWakeUp();
	}
	
	protected override void ActivatorSleep() {
		base.ActivatorSleep();
		
		ClearData();
	}
	
	protected override void SetBlink(bool blink) {
	}
	
	protected override void OnDestroy() {
		ClearData();
		
		if(mListener != null) {
			mListener.enterCallback -= OnActionEnter;
			mListener.hitEnterCallback -= OnActionHitEnter;
			mListener.hitExitCallback -= OnActionHitExit;
			mListener.finishCallback -= OnActionFinish;
		}
		
		if(mStats != null) {
			mStats.statChangeCallback -= OnStatChange;
		}
		
		base.OnDestroy();
	}
	
	//optional implements of callbacks
	
	protected virtual void OnActionEnter(ActionListener listen) {
		if(FSM != null) {
			FSM.SendEvent(EntityEvent.ActionEnter);
		}
	}
	
	protected virtual void OnActionFinish(ActionListener listen) {
		if(FSM != null) {
			FSM.SendEvent(EntityEvent.ActionFinish);
		}
	}
	
	protected virtual void OnActionHitEnter(ActionListener listen, ContactPoint info) {
		if(FSM != null) {
			FSM.SendEvent(EntityEvent.ActionHitEnter);
		}
	}
	
	protected virtual void OnActionHitExit(ActionListener listen) {
		if(FSM != null) {
			FSM.SendEvent(EntityEvent.ActionHitExit);
		}
	}
	
	void OnStatChange(StatBase stat) {
		FSM.SendEvent(EntityEvent.StatChanged);
	}
			
	private void ClearData() {
		if(mWeapon != null) {
			mWeaponParam.seek = null;
			mWeapon.Release();
		}
		
		if(mListener != null) {
			mListener.SetActive(false);
		}
		
		if(mActTarget != null) {
			mActTarget.StopAction();
			mActTarget.indicatorOn = false;
		}
		
		if(mSpriteControl != null) {
			mSpriteControl.ClearCallbacks();
		}
		
		FlockUnitRelease();
	}
	
	private void FlockUnitInit() {
		//add to group
		//remove from group if it still exists
		if(mFlockUnit != null) {
			mFlockUnit.enabled = true;
			mFlockUnit.groupMoveEnabled = true;
			mFlockUnit.catchUpEnabled = true;
			mFlockUnit.minMoveTargetDistance = 0.0f;
			mFlockUnit.sensor.collider.enabled = true;
			
			FlockGroup grp = FlockGroup.GetGroup(mFlockUnit.type);
			if(grp != null) {
				grp.AddUnit(mFlockUnit);
			}
		}
	}
	
	private void FlockUnitRelease() {
		//remove from group if it still exists
		if(mFlockUnit != null) {
			FlockGroup grp = FlockGroup.GetGroup(mFlockUnit.type);
			if(grp != null) {
				grp.RemoveUnit(mFlockUnit, null);
			}
			
			mFlockUnit.body.velocity = Vector3.zero;
			mFlockUnit.moveTarget = null;
			mFlockUnit.sensor.collider.enabled = false;
			mFlockUnit.sensor.units.Clear();
		}
	}
}
