using UnityEngine;
using System.Collections;

public class StatBase : MonoBehaviour {
	public delegate void OnStatChange(StatBase stat, float delta);
	
	public event OnStatChange hpChangeCallback;
	
	[SerializeField] protected StatHUD hud; //the hud associated with the stat
	
	[SerializeField] float _damage = 1.0f;
	[SerializeField] float _maxHP = 1.0f;
	
	protected float mCurHP;
		
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
				float prevHP = mCurHP;
				mCurHP = value;
				
				if(mCurHP < 0) {
					mCurHP = 0;
				}
				
				if(hud != null) {
					hud.StatsRefresh(this, true);
				}
				
				if(hpChangeCallback != null) {
					hpChangeCallback(this, value - prevHP);
				}
			}
		}
	}
	
	public float HPScale {
		get { return _maxHP != 0.0f ? mCurHP/_maxHP : 0.0f; }
	}
	
	public virtual void Refresh() {
		if(hud != null) {
			hud.StatsRefresh(this, false);
		}
		
		if(hpChangeCallback != null) {
			hpChangeCallback(this, 0);
		}
	}
	
	public virtual void ResetStats() {
		mCurHP = _maxHP;
	}
	
	protected virtual void Awake() {
		ResetStats();
	}
}
