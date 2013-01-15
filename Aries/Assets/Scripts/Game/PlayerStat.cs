using UnityEngine;
using System.Collections;

public class PlayerStat : StatBase {
	
	public float maxResource;
	public float initResource;
	
	public event OnStatChange resourceChangeCallback;
	
	private float mCurResource;
	
	public float curResource {
		get { return mCurResource; }
		
		set {
			if(mCurResource != value) {
				float prevResource = mCurResource;
				mCurResource = value;
				
				if(hud != null) {
					hud.StatsRefresh(this, true);
				}
				
				if(resourceChangeCallback != null) {
					resourceChangeCallback(this, value - prevResource);
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
		
		mCurResource = initResource;
	}
	
	protected override void Awake() {
		base.Awake();
	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
