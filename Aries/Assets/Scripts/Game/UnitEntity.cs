using UnityEngine;
using System.Collections;

public class UnitEntity : EntityBase {
	public UnitAttackType attackType;
	
	public UnitStatusIndicator statusIndicator;
	
	private UnitStat mStats;
	private FlockUnit mFlockUnit;
	private ActionListener mListener;
	private ActionTarget mActTarget;
	private UnitSpriteController mSpriteControl;
	
	private Weapon mWeapon;
	private Weapon.RepeatParam mWeaponParam;
	
	private SpellCaster mSpellCaster;
	
	private float mAttackCosTheta;
	
	//just one for now
	protected SpellInstance mSpells;
	
	public UnitStat stats { get { return mStats; } }
	public FlockUnit flockUnit { get { return mFlockUnit; } }
	public ActionListener listener { get { return mListener; } }
	public ActionTarget actionTarget { get { return mActTarget; } }
	public UnitSpriteController spriteControl { get { return mSpriteControl; } }
	public Weapon weapon { get { return mWeapon; } }
	public SpellCaster spellCaster { get { return mSpellCaster; } }
	
	public Player owner {
		get {
			if(mFlockUnit != null) {
				int index = ((int)mFlockUnit.type) - Player.playerIndOfs;
				if(index >= 0 && index < Player.playerCount) {
					return Player.GetPlayer(index);
				}
			}
			
			return null;
		}
	}
	
	///<summary>check to see if this unit is suseptible to given spell</summary>
	public bool SpellCheck(SpellBase spell) {
		return (stats == null 
			&& spell.harm ? (!stats.invulnerable && stats.CanBeHurtBy(UnitDamageType.Curse)) : stats.CanBeHurtBy(UnitDamageType.Miracle));
	}
	
	public void SpellAdd(SpellBase spell) {
		mSpells.Stop(this);
		
		mSpells = new SpellInstance(this, spell);
	}
			
	protected override void Awake() {
		base.Awake();
										
		mStats = GetComponentInChildren<UnitStat>();
		mFlockUnit = GetComponentInChildren<FlockUnit>();
		mListener = GetComponentInChildren<ActionListener>();
		mActTarget = GetComponentInChildren<ActionTarget>();
		mSpriteControl = GetComponentInChildren<UnitSpriteController>();
		mWeapon = GetComponentInChildren<Weapon>();
		mSpellCaster = GetComponentInChildren<SpellCaster>();
		
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
		//clear out debuffs
		mSpells.Stop(this);
		
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
		
		mSpells.Resume(this);
		
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
		
		if(mSpellCaster != null) {
			mSpellCaster.Cancel();
			mSpellCaster.ClearCallbacks();
		}
		
		if(mListener != null) {
			mListener.SetActive(false);
		}
		
		if(mActTarget != null) {
			mActTarget.StopAction();
			mActTarget.indicatorOn = false;
		}
		
		if(mSpriteControl != null) {
			mSpriteControl.reverse = false;
			mSpriteControl.ClearCallbacks();
		}
		
		FlockUnitRelease();
		
		if(statusIndicator != null) {
			statusIndicator.Hide();
		}
	}
	
	private void FlockUnitInit() {
		//add to group
		//remove from group if it still exists
		if(mFlockUnit != null) {
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
			
			mFlockUnit.ResetData();
		}
	}
}
