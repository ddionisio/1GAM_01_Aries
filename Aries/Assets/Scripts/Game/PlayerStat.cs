using UnityEngine;
using System.Collections;

public class PlayerStat : StatBase {
	public FlockType flockGroup = FlockType.PlayerOneUnits;
	
	public float maxResource;
	public float minResource; //regen if current resource is below this number
	
	public int maxSummon;
	
	public float resourcePerSecond = 2.0f;
	
	public int minSummonCriteria = 5; //regen if num summon is below this number
	
	public event OnStatChange resourceChangeCallback;
	
	public float curResource {
		get { return Main.instance != null ? Main.instance.userData.resources : 0.0f; }
		
		set {
			if(Main.instance != null) {
				float resource = Main.instance.userData.resources;
				
				if(resource != value) {
					Main.instance.userData.resources = value;
					
					if(hud != null) {
						hud.StatsRefresh(this, true);
					}
					
					if(resourceChangeCallback != null) {
						resourceChangeCallback(this, value - resource);
					}
				}
			}
		}
	}
	
	public override void Refresh() {
		base.Refresh();
		
		if(resourceChangeCallback != null) {
			resourceChangeCallback(this, 0);
		}
	}
	
	public override void ResetStats() {
		base.ResetStats();
	}
	
	protected override void OnDestroy() {
		resourceChangeCallback = null;
		
		base.OnDestroy();
	}
	
	protected override void Awake() {
		base.Awake();
	}

	// Use this for initialization
	void Start () {
		
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
