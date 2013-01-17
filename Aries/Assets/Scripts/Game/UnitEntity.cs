using UnityEngine;
using System.Collections;

public class UnitEntity : EntityBase {
	public UnitAttackType attackType;
	
	public float attackForceBack = 50.0f; //amount of force to move back after an attack
	
	public float unSummonDelay = 0.5f;
	
	private UnitStat mStats;
	private FlockUnit mFlockUnit;
	private ActionListener mListener;
	private ActionTarget mActTarget;
	private UnitSpriteController mSpriteControl;
	
	private float mCurSummonTime = 0.0f;
	private Vector2 mAttackHitNormal; //the contact normal from when we hit something while attacking
	
	public UnitStat stats { get { return mStats; } }
	public FlockUnit flockUnit { get { return mFlockUnit; } }
	public ActionListener listener { get { return mListener; } }
	public ActionTarget actionTarget { get { return mActTarget; } }
	public UnitSpriteController spriteControl { get { return mSpriteControl; } }
			
	protected override void Awake() {
		base.Awake();
		
		mStats = GetComponentInChildren<UnitStat>();
		mFlockUnit = GetComponentInChildren<FlockUnit>();
		mListener = GetComponentInChildren<ActionListener>();
		mActTarget = GetComponentInChildren<ActionTarget>();
		mSpriteControl = GetComponentInChildren<UnitSpriteController>();
		
		//hook calls up
		mStats.hpChangeCallback += OnHPChange;
		
		mSpriteControl.stateFinishCallback += OnSpriteAnimationComplete;
		
		if(mListener != null) {
			mListener.enterCallback += OnActionEnter;
			mListener.exitCallback += OnActionExit;
			mListener.hitEnterCallback += OnActionHitEnter;
			mListener.finishCallback += OnActionFinish;
		}
	}
	
	// Use this for initialization
	protected override void Start() {
		base.Start();
		
		//stuff
	}
	
	public override void Release() {
		ClearData();
		
		mStats.ResetStats();
						
		base.Release();
	}
		
	protected override void StateChanged() {
		switch(state) {
		case EntityState.spawning:
			//spawn started
			break;
			
		case EntityState.unsummon:
			//fx
			
			mCurSummonTime = 0.0f;
			break;
			
		case EntityState.dying:
			if(mActTarget != null) {
				mActTarget.StopAction();
			}
			break;
		}
	}
	
	protected override void SpawnFinish() {
		//complete
		FlockUnitInit();
		
		if(mListener != null) {
			mListener.lockAction = false;
		}
		
		state = EntityState.normal;
	}
	
	protected override void SetBlink(bool blink) {
	}
	
	protected override void OnDestroy() {
		ClearData();
		
		if(mListener != null) {
			mListener.enterCallback -= OnActionEnter;
			mListener.exitCallback -= OnActionExit;
			mListener.hitEnterCallback -= OnActionHitEnter;
			mListener.finishCallback -= OnActionFinish;
		}
		
		if(mStats != null) {
			mStats.hpChangeCallback -= OnHPChange;
		}
		
		if(mSpriteControl != null) {
			mSpriteControl.stateFinishCallback -= OnSpriteAnimationComplete;
		}
		
		base.OnDestroy();
	}
	
	// Update is called once per frame
	void LateUpdate () {
		switch(state) {
		case EntityState.unsummon:
			mCurSummonTime += Time.deltaTime;
			if(mCurSummonTime >= unSummonDelay) {
				Release();
			}
			break;
		}
	}
	
	
	void OnActionEnter(ActionListener listen) {
		switch(listen.currentTarget.type) {
		case ActionType.Attack:
			mAttackHitNormal = Vector2.zero;
			
			if(mSpriteControl != null) {
				mSpriteControl.state = (int)UnitSpriteState.AttackPursue;
			}
			
			//disable group follow
			mFlockUnit.groupMoveEnabled = false;
			break;
			
		default:
			if(mSpriteControl != null) {
				mSpriteControl.state = (int)UnitSpriteState.Move;
			}
			break;
		}
	}
	
	void OnActionExit(ActionListener listen) {
		/*if(mSpriteControl != null) {
			mSpriteControl.state = (int)UnitSpriteState.Move;
		}*/
	}
	
	void OnActionFinish(ActionListener listen) {
		if(mFlockUnit != null) {
			mFlockUnit.enabled = true;
			
			//re-enable group follow
			mFlockUnit.groupMoveEnabled = true;
		}
		
		if(mSpriteControl != null) {
			mSpriteControl.state = (int)UnitSpriteState.Move;
		}
	}
	
	void OnActionHitEnter(ActionListener listen, ContactPoint info) {
		//collided with target
		switch(listen.currentTarget.type) {
		case ActionType.Attack:
			//perform attack if we haven't already
			if(mSpriteControl.state != (int)UnitSpriteState.Attack) {
				if(attackType == UnitAttackType.Melee) {
					mAttackHitNormal = info.normal;
					
					//attack, should get a call later when we finish
					mSpriteControl.state = (int)UnitSpriteState.Attack;
					
					//halt
					if(mFlockUnit != null) {
						mFlockUnit.enabled = false;
					}
					
					if(rigidbody != null) {
						rigidbody.velocity = Vector3.zero;
					}
					//
					
					//hit the bastard
					StatBase targetStat = listen.currentTarget.GetComponent<StatBase>();
					if(targetStat != null && mStats.CanDamage(targetStat)) {
						targetStat.curHP -= mStats.damage;
					}
					else {
						//TODO: react?
					}
				}
			}
			break;
		}
	}
			
	void OnSpriteAnimationComplete(int state, int dir) {
		if(state == (int)UnitSpriteState.Attack) {
			//make sure we still have a target and we are still attacking
			if(mListener != null && mListener.currentTarget != null) {
				switch(mListener.currentTarget.type) {
				case ActionType.Attack:
					//return to pursue
					if(mFlockUnit != null) {
						mFlockUnit.enabled = true;
					}
					
					//push back a bit
					if(attackForceBack > 0.0f && rigidbody != null) {
						Vector2 force = mAttackHitNormal*attackForceBack;
						rigidbody.AddForce(force.x, force.y, 0.0f);
					}
					
					mSpriteControl.state = (int)UnitSpriteState.AttackPursue;
					break;
				}
			}
		}
		//other things
	}
	
	void OnHPChange(StatBase stat, float delta) {
		if(stat.curHP == 0.0f) {
			state = EntityState.dying;
		}
	}
			
	private void ClearData() {
		if(mListener != null) {
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
