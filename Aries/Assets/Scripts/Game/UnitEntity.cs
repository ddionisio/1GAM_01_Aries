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
	
	private Vector2 mAttackHitNormal; //for melee: the contact normal during collision hit, for range: direction to shoot
	
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
		
		switch((UnitSpriteEvent)animDat.valI) {
		case UnitSpriteEvent.WeaponShoot:
			if(mWeapon != null && mListener != null && mListener.currentTarget != null) {
				mWeapon.Shoot(transform.position, mAttackHitNormal, mListener.currentTarget.target);
			}
			break;
			
		case UnitSpriteEvent.WeaponShootRepeat:
			if(mWeapon != null && mListener != null && mListener.currentTarget != null) {
				mWeaponParam.seek = mListener.currentTarget.target;
				mWeaponParam.dir = mAttackHitNormal;
				mWeapon.Repeat(mWeaponParam);
			}
			break;
			
		case UnitSpriteEvent.WeaponShootRepeatStop:
			if(mWeapon != null) {
				mWeaponParam.seek = null;
				mWeapon.RepeatStop();
			}
			break;
		}
	}
	
	// Update is called once per frame
	void LateUpdate () {
		switch(state) {
		case EntityState.attacking:
			//if target is null or something else, then we shouldn't be in this state
			//perform attack if we haven't already
			if(mSpriteControl.state != UnitSpriteState.Attack) {
				//only range types will attack and shoot weapon
				if(attackRange > 0.0f) {
					//distance check
					bool goAttack = mFlockUnit != null ? 
						mFlockUnit.avoidDistance <= mFlockUnit.moveTargetDistance && mFlockUnit.moveTargetDistance <= attackRange && Vector2.Dot(mFlockUnit.moveTargetDir, mFlockUnit.dir) > 0.0f
					 : (mListener.currentTarget.target.position - transform.position).sqrMagnitude <= attackRange*attackRange;
					
					if(goAttack) {
						//attack, should get a call later when we finish
						mSpriteControl.state = UnitSpriteState.Attack;
						
						if(mFlockUnit != null) {
							mAttackHitNormal = mFlockUnit.dir;
							
							//halt
							mFlockUnit.enabled = false;
						}
						
						if(rigidbody != null) {
							rigidbody.velocity = Vector3.zero;
						}
						//
						
						//NOTE: weapon should be shot within an event in the animation
					}
				}
			}
			break;
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
