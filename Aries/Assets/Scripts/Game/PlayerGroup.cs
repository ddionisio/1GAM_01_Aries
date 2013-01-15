using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerGroup : FlockGroup {
	private Dictionary<UnitType, HashSet<UnitEntity>> mUnitsByType = new Dictionary<UnitType, HashSet<UnitEntity>>();
	
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
