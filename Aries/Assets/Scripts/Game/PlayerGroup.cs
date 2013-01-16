using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerGroup : FlockGroup {
	private Dictionary<UnitType, HashSet<UnitEntity>> mUnitsByType = new Dictionary<UnitType, HashSet<UnitEntity>>();
	
	public UnitEntity GrabUnit(UnitType type, ActionTarget.Priority priority) {
		HashSet<UnitEntity> units;
		
		UnitEntity ret = null;
		
		if(mUnitsByType.TryGetValue(type, out units)) {
			foreach(UnitEntity unit in units) {
				switch(unit.state) {
				case EntityState.dying:
				case EntityState.spawning:
				case EntityState.unsummon:
					//don't grab these
					break;
					
				default:
					ActionListener listener = unit.listener;
					if(listener.currentPriority <= priority) {
						ret = unit;
					}
					break;
				}
				
				if(ret != null)
					break;
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
			
			if(!mUnitsByType.TryGetValue(stats.type, out units)) {
				units.Remove(unitEntity);
			}
		}
	}
}
