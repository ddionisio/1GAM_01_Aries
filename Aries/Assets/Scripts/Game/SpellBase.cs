using UnityEngine;
using System.Collections;

public class SpellInstance {
	private SpellBase mSpell;
	private float mCurTime;
	private float mTickStart;
	
	public bool alive { get { return mSpell != null; } }
	
	public bool CheckFlags(SpellFlag spellFlags) {
		return (mSpell.flags & spellFlags) != 0;
	}
	
	public bool IsSpellMatch(SpellBase spell) {
		return spell.id == mSpell.id;
	}
	
	public SpellInstance(UnitEntity unit, SpellBase spell) {
		mCurTime = 0;
		mTickStart = Time.fixedTime;
		mSpell = spell;
		
		if(unit.statusIndicator != null && unit.statusIndicator.curIcon != mSpell.icon) {
			unit.statusIndicator.Show(mSpell.icon, mSpell.duration);
		}

        if(mSpell.mod != null) {
            unit.stats.AddMod(mSpell.mod);
        }
		
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

            if(mSpell.mod != null) {
                unit.stats.RemoveMod(mSpell.mod);
            }
			
			Remove(unit);
			mSpell = null;
		}
	}

    protected virtual void Remove(UnitEntity unit) { }
    protected virtual void Tick(UnitEntity unit) { } //for stuff like: poison, etc.
	
	IEnumerator DebuffUpdate(UnitEntity unit) {
		while(alive) {
			if(mCurTime < mSpell.duration) {
				mCurTime += Time.fixedDeltaTime;
				
				if(mSpell.tickDelay > 0.0f && Time.fixedTime - mTickStart > mSpell.tickDelay) {
					Tick(unit);
					
					mTickStart = Time.fixedTime;
				}
			
				yield return new WaitForFixedUpdate();
			}
			else {
				Stop(unit);
				unit.SpellRemoveDead();
			}
		}
		
		yield break;
	}
}

public class SpellBase {
	public float duration = 0.0f;
	public float tickDelay = 0.0f; // if > 0, calls tick every delay
	public UnitStatusIndicator.Icon icon = UnitStatusIndicator.Icon.NumIcons;

    public StatMod mod = null;

	private SpellFlag mFlags;
	private int mId;
	
	public int id { get { return mId; } }
	public SpellFlag flags { get { return mFlags; } }
	
	public void _setId(int aId) { mId = aId; }
	public void _setFlags(SpellFlag aFlags) { mFlags = aFlags; }

    public virtual SpellInstance Start(UnitEntity unit) {
        return new SpellInstance(unit, this);
    }
}
