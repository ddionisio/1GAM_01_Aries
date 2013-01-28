using UnityEngine;
using System.Collections;

public struct SpellInstance {
	private SpellBase mSpell;
	private float mCurTime;
	private float mTickStart;
	
	public bool alive { get { return mSpell != null; } }
	
	public SpellInstance(UnitEntity unit, SpellBase spell) {
		mCurTime = 0;
		mTickStart = Time.fixedTime;
		mSpell = spell;
		
		mSpell.Apply(unit);
		
		unit.StartCoroutine(DebuffUpdate(unit));
	}
	
	//when update needs to start again
	public void Resume(UnitEntity unit) {
		if(alive) {
			if(unit.statusIndicator != null && unit.statusIndicator.curIcon == mSpell.icon) {
				unit.statusIndicator.Show(mSpell.icon, mSpell.duration - mCurTime);
			}
			
			mTickStart = Time.fixedTime;
			unit.StartCoroutine(DebuffUpdate(unit));
		}
	}
	
	//remove, basically (used by something like dispell)
	public void Stop(UnitEntity unit) {
		if(alive) {
			if(unit.statusIndicator != null && unit.statusIndicator.curIcon == mSpell.icon) {
				unit.statusIndicator.Hide();
			}
			
			mSpell.Remove(unit);
			mSpell = null;
		}
	}
	
	IEnumerator DebuffUpdate(UnitEntity unit) {
		while(alive) {
			if(mCurTime < mSpell.duration) {
				mCurTime += Time.fixedDeltaTime;
				
				if(mSpell.tickDelay > 0.0f && Time.fixedTime - mTickStart > mSpell.tickDelay) {
					mSpell.Tick(unit);
					
					mTickStart = Time.fixedTime;
				}
			
				yield return new WaitForFixedUpdate();
			}
			else {
				mSpell.Remove(unit);
				mSpell = null;
				break;
			}
		}
		
		yield break;
	}
}

public abstract class SpellBase {
	public float duration = 0.0f;
	public float tickDelay = 0.0f; // if > 0, calls tick every delay
	public bool harm = false; //debuff
	public UnitStatusIndicator.Icon icon = UnitStatusIndicator.Icon.NumIcons;
	
	public abstract void Apply(UnitEntity unit);
	public abstract void Remove(UnitEntity unit);
	public abstract void Tick(UnitEntity unit); //for stuff like: poison, etc.
}
