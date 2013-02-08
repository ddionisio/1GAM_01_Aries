using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UnitEntity : EntityBase {
    public UnitStatusIndicator statusIndicator;

    private UnitStat mStats;
    private FlockUnit mFlockUnit;
    private ActionListener mListener;
    private ActionTarget mActTarget;
    private UnitSpriteController mSpriteControl;

    private Weapon mWeapon;

    private SpellCaster mSpellCaster;

    private float mAttackCosTheta;

    //just one for now
    protected List<SpellInstance> mSpells = new List<SpellInstance>(1);

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
                int index = Player.GroupToIndex(mFlockUnit.type);
                if(index >= 0 && index < Player.playerCount) {
                    return Player.GetPlayer(index);
                }
            }

            return null;
        }
    }

    public bool SpellCheckFlags(SpellFlag spellFlags) {
        return mSpells.FindIndex(x => x.CheckFlags(spellFlags)) != -1;
    }

    //TODO: refactor with a better system, this one sucks!

    ///<summary>check to see if this unit is suseptible to given spell</summary>
    public bool SpellCheck(SpellBase spell) {
        return (stats != null
            && mSpells.FindIndex(x => x.IsSpellMatch(spell)) == -1 //already affected
            && !stats.SpellImmuneCheck(spell.flags));
    }

    public void SpellAdd(SpellBase spell) {
        SpellRemoveDead();
        if(mSpells.FindIndex(x => x.IsSpellMatch(spell)) == -1) {
            mSpells.Add(spell.Start(this));
            if(mSpells.Count > 1)
                Debug.LogWarning("more than one spell? spell count: " + mSpells.Count);
        }
    }

    public void SpellRemoveDead() {
        int ind;
        while((ind = mSpells.FindIndex(x => !x.alive)) != -1) {
            mSpells.RemoveAt(ind);
        }
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

    protected override void OnDespawned() {
        //clear out debuffs
        foreach(SpellInstance si in mSpells) {
            si.Stop(this);
        }

        mSpells.Clear();

        ClearData();

        mStats.ResetStats();

        base.OnDespawned();
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

        SpellRemoveDead();
        foreach(SpellInstance si in mSpells) {
            si.Resume(this);
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
            mWeapon.Release();
        }

        if(mSpellCaster != null) {
            mSpellCaster.Cancel(true);
            mSpellCaster.ClearCallbacks();
        }

        if(mListener != null) {
            mListener.SetActive(false);
        }

        if(mActTarget != null) {
            mActTarget.StopAction();
            mActTarget.indicatorOn = false;
            mActTarget.lockTargetted = false;
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

    void OnPrefabUpdate(GameObject go) {
        UnitStatusIndicator indicator = go.GetComponentInChildren<UnitStatusIndicator>();
        if(indicator != null)
            statusIndicator = indicator;
    }
}
