using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerGroup : FlockGroup {
	private Dictionary<UnitType, HashSet<UnitEntity>> mUnitsByType = new Dictionary<UnitType, HashSet<UnitEntity>>();
	
	public int GetUnitCountByType(UnitType type) {
		if(!mUnitsByType.ContainsKey(type))
			return 0;
		
		return mUnitsByType[type].Count;
	}
	
	public IEnumerable GetTargetFilter(UnitType type, ActionTarget target) {
		HashSet<UnitEntity> units;
		if(mUnitsByType.TryGetValue(type, out units)) {
			
			foreach(UnitEntity unit in units) {
				ActionListener listener = unit.listener;
				if(target.vacancy && listener.currentPriority <= target.priority) {
					switch(target.type) {
					case ActionType.Attack:
						StatBase targetStats = target.GetComponentInChildren<StatBase>();
						if(targetStats == null || unit.stats.CanDamage(targetStats)) {
							yield return unit;
						}
						break;
						
					default:
						yield return unit;
						break;
					}
				}
			}
		}
	}
	
	public IEnumerable GetTargetFilter(ActionTarget target) {
		foreach(KeyValuePair<UnitType, HashSet<UnitEntity>> key in mUnitsByType) {
			foreach(UnitEntity unit in key.Value) {
				ActionListener listener = unit.listener;
				if(!listener.lockAction && target.vacancy && listener.currentPriority <= target.priority) {
					switch(target.type) {
					case ActionType.Attack:
						StatBase targetStats = target.GetComponentInChildren<StatBase>();
						if(targetStats == null || unit.stats.CanDamage(targetStats)) {
							yield return unit;
						}
						break;
						
					default:
						yield return unit;
						break;
					}
				}
			}
		}
	}
	
	public IEnumerable GetUnits() {
		foreach(KeyValuePair<UnitType, HashSet<UnitEntity>> key in mUnitsByType) {
			foreach(UnitEntity unit in key.Value) {
				yield return unit;
			}
		}
	}
	
	public UnitEntity GrabUnit(UnitType type, ActionTarget.Priority priority) {
		HashSet<UnitEntity> units;
		
		UnitEntity ret = null;
		
		if(mUnitsByType.TryGetValue(type, out units)) {
			foreach(UnitEntity unit in units) {
				ActionListener listener = unit.listener;
				if(!listener.lockAction && listener.currentPriority <= priority) {
					ret = unit;
					break;
				}
			}
		}
		
		return ret;
	}
	
	protected override void OnAddUnit(FlockUnit unit) {
		UnitEntity unitEntity = unit.GetComponentInChildren<UnitEntity>();
		UnitStat stats = unitEntity.stats;
		
		if(unitEntity != null && stats != null) {
			HashSet<UnitEntity> units;
		
			if(!mUnitsByType.TryGetValue(stats.type, out units)) {
				units = new HashSet<UnitEntity>();
				mUnitsByType.Add(stats.type, units);
			}
			
			units.Add(unitEntity);
		}
	}
	
	protected override void OnRemoveUnit(FlockUnit unit) {
		UnitEntity unitEntity = unit.GetComponentInChildren<UnitEntity>();
		UnitStat stats = unitEntity.stats;
		
		if(unitEntity != null && stats != null) {
			HashSet<UnitEntity> units;
			
			if(mUnitsByType.TryGetValue(stats.type, out units)) {
				units.Remove(unitEntity);
			}
		}
	}
}
