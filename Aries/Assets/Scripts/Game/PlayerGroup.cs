using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerGroup : FlockGroup {
	private Dictionary<UnitType, HashSet<UnitEntity>> mUnitsByType = new Dictionary<UnitType, HashSet<UnitEntity>>();
	
	public IEnumerable GetTargetFilter(UnitType type, ActionTarget target) {
		HashSet<UnitEntity> units;
		if(mUnitsByType.TryGetValue(type, out units)) {
			
			foreach(UnitEntity unit in units) {
				if(UnitStateIsValid(unit)) {
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
	}
	
	public UnitEntity GrabUnit(UnitType type, ActionTarget.Priority priority) {
		HashSet<UnitEntity> units;
		
		UnitEntity ret = null;
		
		if(mUnitsByType.TryGetValue(type, out units)) {
			foreach(UnitEntity unit in units) {
				if(UnitStateIsValid(unit)) {
					ActionListener listener = unit.listener;
					if(listener.currentPriority <= priority) {
						ret = unit;
						break;
					}
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
	
	private bool UnitStateIsValid(UnitEntity unit) {
		bool ret = false;
		
		if(unit != null) {
			switch(unit.state) {
			case EntityState.dying:
			case EntityState.spawning:
			case EntityState.unsummon:
				//don't grab these
				break;
				
			default:
				ret = true;
				break;
			}
		}
		
		return ret;
	}
}
