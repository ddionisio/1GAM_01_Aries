using UnityEngine;
using System.Collections;

public class PlayerStat : StatBase {
	public FlockType flockGroup = FlockType.PlayerOneUnits;
	
	public float maxResource;
	public float minResource; //regen if current resource is below this number
	
	public float resourcePerSecond = 1.0f;
	
	public float curResource {
		get { return Main.instance != null ? Main.instance.userData.resources : 0.0f; }
		
		set {
			if(Main.instance != null) {
				float resource = Main.instance.userData.resources;
				
				if(resource != value) {
					Main.instance.userData.resources = value;
					if(Main.instance.userData.resources < 0)
						Main.instance.userData.resources = 0;
					else if(Main.instance.userData.resources > maxResource)
						Main.instance.userData.resources = maxResource;
					
					if(Main.instance.userData.resources != resource)
						StatChanged(true);
				}
			}
		}
	}
	
	public void InitResource() {
		if(curResource < minResource) {
			curResource = minResource;
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
			curResource += resourcePerSecond*Time.deltaTime;
		}
	}
}
