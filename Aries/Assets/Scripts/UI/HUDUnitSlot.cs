using UnityEngine;
using System.Collections;

public class HUDUnitSlot : MonoBehaviour {
	public const string unitCountFormat = "{0}/{1}";
	
	public UISprite portrait;
	public UISprite frame;
	public UILabel hotkeyLabel;
	public UILabel unitCountLabel;
	public UIFilledSprite cooldown;
	
	private bool mAvailable = true;
	private FlockType mGroupType = FlockType.NumType;
	
	private UnitType mSummonType = UnitType.NumTypes;
	private int mSummonCount = 0;
	private int mSummonMax = 0;
			
	public void Setup(FlockType grpType, SummonSlot slot) {
		Clear();
						
		if(slot.data != null) {
			//set group
			mGroupType = grpType;
		
			if(mGroupType != FlockType.NumType) {
				FlockGroup grp = FlockGroup.GetGroup(mGroupType);
				if(grp != null) {
					grp.addCallback += AddUnit;
					grp.removeCallback += RemoveUnit;
				}
			}
			
			hotkeyLabel.gameObject.SetActive(true);
			unitCountLabel.gameObject.SetActive(true);
			cooldown.gameObject.SetActive(true);
			
			//set portrait image based on type
			string portraitRef = slot.data.type.ToString();
			if(portrait.atlas.GetSprite(portraitRef) != null) {
				portrait.spriteName = portraitRef;
				portrait.gameObject.SetActive(true);
			}
			else {
				Debug.LogWarning("sprite not found: "+portraitRef);
				portrait.gameObject.SetActive(false);
			}
			
			//set info
			cooldown.fillAmount = 0.0f;
			
			mSummonType = slot.data.type;
			mSummonMax = slot.data.summonMax;
			
			//setup initial count
			UpdateUnitCountFromGroup();
			
			//hotkey
			
			SetAvailable(true);
		}
		else {
			portrait.gameObject.SetActive(false);
			hotkeyLabel.gameObject.SetActive(false);
			unitCountLabel.gameObject.SetActive(false);
			cooldown.gameObject.SetActive(false);
			
			SetAvailable(false);
		}
		
		//if(slot.
		//data.
	}
	
	//when changing/deinit
	public void Clear() {
		//group
		if(mGroupType != FlockType.NumType) {
			FlockGroup grp = FlockGroup.GetGroup(mGroupType);
			if(grp != null) {
				grp.addCallback -= AddUnit;
				grp.removeCallback -= RemoveUnit;
			}
			
			mGroupType = FlockType.NumType;
		}
		
		//other
	}
	
	public void SetAvailable(bool available) {
		Color clr;
		
		if(mAvailable != available) {
			if(available) {
				clr = portrait.color;
				clr.r = 1.0f; clr.g = 1.0f; clr.b = 1.0f;
				portrait.color = clr;
				
				clr = frame.color;
				clr.r = 1.0f; clr.g = 1.0f; clr.b = 1.0f;
				frame.color = clr;
			}
			else {
				clr = portrait.color;
				clr.r = 0.5f; clr.g = 0.5f; clr.b = 0.5f;
				portrait.color = clr;
				
				clr = frame.color;
				clr.r = 0.5f; clr.g = 0.5f; clr.b = 0.5f;
				frame.color = clr;
			}
			
			mAvailable = available;
		}
	}
	
	public void UpdateCooldown(float amt) {
		cooldown.fillAmount = amt;
	}
	
	
	
	void OnDestroy() {
		Clear();
	}
	
	void UpdateUnitCount() {
		unitCountLabel.text = string.Format(unitCountFormat, mSummonCount, mSummonMax);
	}
	
	//when changing player
	void UpdateUnitCountFromGroup() {
		mSummonCount = 0;
		
		if(mGroupType != FlockType.NumType && mSummonType != UnitType.NumTypes) {
			PlayerGroup grp = FlockGroup.GetGroup(mGroupType) as PlayerGroup;
			if(grp != null) {
				mSummonCount = grp.GetUnitCountByType(mSummonType);
			}
		}
		
		UpdateUnitCount();
	}
		
	void AddUnit(FlockUnit unit) {
		UnitEntity unitEnt = unit.GetComponent<UnitEntity>();
		if(unitEnt != null && unitEnt.stats != null && unitEnt.stats.type == mSummonType) {
			mSummonCount++;
			UpdateUnitCount();
		}
	}
	
	void RemoveUnit(FlockUnit unit) {
		UnitEntity unitEnt = unit.GetComponent<UnitEntity>();
		if(unitEnt != null && unitEnt.stats != null && unitEnt.stats.type == mSummonType) {
			mSummonCount--;
			UpdateUnitCount();
		}
	}
}
