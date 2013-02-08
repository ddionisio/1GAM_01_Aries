using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StatMod {
    public float damage = 0.0f; //all damages are added and then: base_damage + base_damage*(sum damage mods), 1.0f = 100% of base damage
    public float mass = 0.0f; //
    public float maxHP = 0.0f;
    public UnitDamageType damageReceiveFlags = (UnitDamageType)0; //can be resist or suseptibility
    public float damageReceive = 0.0f; //hp -= damage + base_damage*(sum damage receives), normally you want this in negative for resist
}

public class StatBase : MonoBehaviour {
    public delegate void OnStatChange(StatBase stat);

    public event OnStatChange statChangeCallback;

    [SerializeField]
    protected StatHUD hud; //the hud associated with the stat

    [SerializeField]
    float _damage = 1.0f;
    [SerializeField]
    float _maxHP = 1.0f;

    public UnitDamageType damageType;

    [HideInInspector]
    public UnitDamageType resistFlags = (UnitDamageType)0; //base resistance, modifies base damage
    [HideInInspector]
    public float resistDamageMod = 1.0f; //1.0 nullifies base damage

    [HideInInspector]
    public SpellFlag spellImmuneFlags = (SpellFlag)0;

    [System.NonSerialized]
    public bool invulnerable = false;
    
    protected float mCurHP;

    protected float mDamageScale = 1.0f;
    protected float mMaxHPAdd = 0.0f;

    private float mMassBase;
    private float mMassAdd;

    private BetterList<StatMod> mMods = new BetterList<StatMod>();
    
    //TODO: stat modifier for debuffs and buffs

    /// <summary>
    /// Determines whether this instance can damage the specified target.
    /// </summary>
    public bool CanDamage(StatBase target) {
        return target != null && !target.invulnerable && target.curHP > 0;
    }

    public float damage {
        get { return _damage; }
    }

    /// <summary>
    /// Get the sum damage modifier of this stat.
    /// </summary>
    public float damageMod {
        get {
            //figure out the damage modifier
            float ret = 0.0f;
            foreach(StatMod mod in mMods) {
                ret += mod.damage;
            }

            return ret;
        }
    }
    
    public float maxHP {
        get { return _maxHP+mMaxHPAdd; }
    }

    public float curHP {
        get { return mCurHP; }

        set {
            if(mCurHP != value) {
                float prev = mCurHP;
                mCurHP = value;

                if(mCurHP < 0) {
                    mCurHP = 0;
                }

                if(mCurHP != prev)
                    StatChanged(true);
            }
        }
    }

    /// <summary>
    /// Check to see if this stat is immune to given spell type
    /// </summary>
    public bool SpellImmuneCheck(SpellFlag spell) {
        return (spell & spellImmuneFlags) != (SpellFlag)0;
    }

    public void AddMod(StatMod mod) {
        mMods.Add(mod);

        //add up hp max and mass
        mMaxHPAdd += mod.maxHP;
        mMassAdd += mMassBase*mod.mass;

        //update based on additions
        if(rigidbody != null) {
            rigidbody.mass = mMassBase + mMassAdd;
        }

        curHP += mod.maxHP;
    }

    public void RemoveMod(StatMod mod) {
        mMods.Remove(mod);

        //subtract hp max and mass
        mMaxHPAdd -= mod.maxHP;
        mMassAdd -= mMassBase * mod.mass;

        //update based on additions
        if(rigidbody != null) {
            rigidbody.mass = mMassBase + mMassAdd;
        }

        //don't let hp overflow
        if(curHP > maxHP) {
            curHP = maxHP;
        }
    }

    /// <summary>
    /// Receive damage from source.
    /// </summary>
    public void DamageBy(StatBase source) {
        DamageBy(source.damageType, source.damage, source.damageMod);
    }

    /// <summary>
    /// Receive damage.
    /// </summary>
    public void DamageBy(UnitDamageType type, float damage, float damageMod) {
        //figure out damage receive modifier
        float damageReceiveMod = 0.0f;
        foreach(StatMod mod in mMods) {
            if((type & mod.damageReceiveFlags) != (UnitDamageType)0) {
                damageReceiveMod += mod.damageReceive;
            }
        }

        float resultDamage;

        if((type & resistFlags) != (UnitDamageType)0)
            resultDamage = damage + damage * damageMod + damage * damageReceiveMod - damage * resistDamageMod;
        else
            resultDamage = damage + damage * damageMod + damage * damageReceiveMod;

        curHP -= resultDamage;
    }

    public float HPScale {
        get { float hpDiv = maxHP; return hpDiv != 0.0f ? mCurHP / hpDiv : 0.0f; }
    }

    public virtual void Refresh() {
        StatChanged(false);
    }

    public virtual void ResetStats() {
        mCurHP = _maxHP;
        invulnerable = false;

        mMaxHPAdd = 0.0f;
        mMassAdd = 0.0f;

        if(rigidbody != null) {
            rigidbody.mass = mMassBase;
        }

        mMods.Clear();

        if(hud != null && hud.isAutoHide) {
            hud.show = false;
        }
    }

    protected void StatChanged(bool doUpdate) {
        if(doUpdate && statChangeCallback != null) {
            statChangeCallback(this);
        }

        if(hud != null) {
            hud.StatsRefresh(this);

            if(doUpdate)
                hud.show = true;
        }
    }

    protected virtual void OnDestroy() {
        statChangeCallback = null;
    }

    protected virtual void Awake() {
        mMassBase = rigidbody != null ? rigidbody.mass : 1.0f;

        ResetStats();
    }

    void OnPrefabUpdate(GameObject go) {
        StatHUD hudRef = go.GetComponentInChildren<StatHUD>();
        if(hudRef != null)
            hud = hudRef;
    }
}
