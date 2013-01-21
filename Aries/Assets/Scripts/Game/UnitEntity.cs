using UnityEngine;
using System.Collections;

public class UnitEntity : EntityBase {
	public UnitAttackType attackType;
	
	public float attackRange = 0.0f; //range to attack (0 for melee)
	public float attackForceBack = 50.0f; //amount of force to move back after an attack
	
	public float unSummonDelay = 0.5f;
	
	private UnitStat mStats;
	private FlockUnit mFlockUnit;
	private ActionListener mListener;
	private ActionTarget mActTarget;
	private UnitSpriteController mSpriteControl;
	
	private Weapon mWeapon;
	private Weapon.RepeatParam mWeaponParam;
	
	private UnitSpriteController.EventData mLastSpriteEventData;
	
	public UnitSpriteController.EventData lastSpriteEventData { get { return mLastSpriteEventData; } }
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
		
		if(mSpriteControl != null) {
			mSpriteControl.stateFinishCallback += OnSpriteAnimationComplete;
			mSpriteControl.stateEventCallback += OnSpriteAnimationEvent;
		}
		
		if(mListener != null) {
			mListener.enterCallback += OnActionEnter;
			mListener.exitCallback += OnActionExit;
			mListener.hitEnterCallback += OnActionHitEnter;
			mListener.hitExitCallback += OnActionHitExit;
			mListener.finishCallback += OnActionFinish;
		}
	}
	
	// Use this for initialization
	protected override void Start() {
		base.Start();
	}
	
	// Use this to determine if we can attack in range
	public bool CheckRangeAttack() {
		return mFlockUnit != null ? 
			   mFlockUnit.avoidDistance <= mFlockUnit.moveTargetDistance && mFlockUnit.moveTargetDistance <= attackRange && Vector2.Dot(mFlockUnit.moveTargetDir, mFlockUnit.dir) > 0.0f
			 : (mListener.currentTarget.target.position - transform.position).sqrMagnitude <= attackRange*attackRange;
					
	}
	
	public override void Release() {
		ClearData();
		
		mStats.ResetStats();
		
		base.Release();
	}
	
	public override void SpawnFinish() {
		FlockUnitInit();
	}
	
	protected override void ActivatorWakeUp() {
		if(!doSpawn) {
			FlockUnitInit();
		}
		
		base.ActivatorWakeUp();
		//FlockUnitInit();
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
			mListener.exitCallback -= OnActionExit;
			mListener.hitEnterCallback -= OnActionHitEnter;
			mListener.hitExitCallback -= OnActionHitExit;
			mListener.finishCallback -= OnActionFinish;
		}
		
		if(mStats != null) {
			mStats.statChangeCallback -= OnStatChange;
		}
		
		if(mSpriteControl != null) {
			mSpriteControl.stateFinishCallback -= OnSpriteAnimationComplete;
			mSpriteControl.stateEventCallback -= OnSpriteAnimationEvent;
		}
		
		base.OnDestroy();
	}
	
	//optional implements of callbacks
	
	protected virtual void OnActionEnter(ActionListener listen) {
		if(FSM != null) {
			FSM.SendEvent(EntityEvent.ActionEnter);
		}
	}
	
	protected virtual void OnActionExit(ActionListener listen) {
		if(FSM != null) {
			FSM.SendEvent(EntityEvent.ActionExit);
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
	
	protected virtual void OnSpriteAnimationComplete(UnitSpriteState animState, UnitSpriteController.Dir animDir) {
		if(FSM != null) {
			FSM.SendEvent(EntityEvent.SpriteAnimComplete);
		}
	}
	
	protected virtual void OnSpriteAnimationEvent(UnitSpriteState animState, UnitSpriteController.Dir animDir, UnitSpriteController.EventData animDat) {
		mLastSpriteEventData = animDat;
		
		if(FSM != null) {
			FSM.SendEvent(EntityEvent.SpriteAnimEvent);
		}
	}
	
	void OnDrawGizmosSelected() {
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(transform.position, attackRange);
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
			mListener.lockAction = false;
			mListener.currentTarget = null;
		}
		
		if(mActTarget != null) {
			mActTarget.StopAction();
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
