using UnityEngine;
using System.Collections;

public class PlayerStat : StatBase {
	public FlockType flockGroup = FlockType.PlayerOneUnits;
	
	public float maxResource;
	public float minResource; //regen if current resource is below this number
	
	public int maxSummon;
	
	public float resourcePerSecond = 2.0f;
	
	public int minSummonCriteria = 5; //regen if num summon is below this number
	
	public float curResource {
		get { return Main.instance != null ? Main.instance.userData.resources : 0.0f; }
		
		set {
			if(Main.instance != null) {
				float resource = Main.instance.userData.resources;
				
				if(resource != value) {
					Main.instance.userData.resources = value;
					
					StatChanged(true);
				}
			}
		}
	}
	
	public void InitResource() {
		//TODO: check for reserved unit spawns (e.g. level transition)
		if(curResource < minResource) {
			FlockGroup grp = FlockGroup.GetGroup(flockGroup);
			if(grp == null || grp.count < minSummonCriteria) {
				curResource = minResource;
			}
		}
	}
	
	public override void Refresh() {
		
		base.Refresh();
	}
	
	public override void ResetStats() {
		base.ResetStats();
	}
	
	// Update is called once per frame
	void Update () {
		if(curResource < minResource) {
			PlayerGroup grp = (PlayerGroup)FlockGroup.GetGroup(flockGroup);
			if(grp.count < minSummonCriteria) {
				curResource += resourcePerSecond*Time.deltaTime;
			}
		}
	}
}
