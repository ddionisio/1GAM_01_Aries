using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StatBase : MonoBehaviour {
	public delegate void OnStatChange(StatBase stat);
	
	public event OnStatChange statChangeCallback;
	
	[SerializeField] protected StatHUD hud; //the hud associated with the stat
	
	[SerializeField] float _damage = 1.0f;
	[SerializeField] float _maxHP = 1.0f;
	
	public UnitDamageType damageType;
	
	[HideInInspector] public UnitDamageType immuneFlags;
	
	[System.NonSerialized] public bool invulnerable = false;
	
	protected float mCurHP;
	
	//TODO: stat modifier for debuffs and buffs
	
	/// <summary>
	/// Determines whether this instance can damage the specified target.
	/// </summary>
	public bool CanDamage(StatBase target) {
		return target == null || target.invulnerable || (target.immuneFlags & damageType) == (UnitDamageType)0;
	}
	
	public bool CanBeHurtBy(UnitDamageType aType) {
		return !invulnerable && (immuneFlags & aType) == (UnitDamageType)0;
	}
		
	public virtual float damage {
		get { return _damage; }
	}
	
	public virtual float maxHP {
		get { return _maxHP; }
	}
	
	public virtual float curHP {
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
	
	public float HPScale {
		get { return _maxHP != 0.0f ? mCurHP/_maxHP : 0.0f; }
	}
	
	public virtual void Refresh() {
		StatChanged(false);
	}
	
	public virtual void ResetStats() {
		mCurHP = _maxHP;
		invulnerable = false;
		
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
		ResetStats();
	}
}
