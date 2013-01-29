using UnityEngine;
using System.Collections;

public class UnitStat : StatBase {
	public UnitType type;
	
	private float mLove; //cost of unit or what enemies drop
	
	public float love {
		get { return mLove; }
	}
	
	public float loveHPScale {
		get { return mLove*HPScale; }
	}
	
	protected override void Awake() {
		base.Awake ();
	}
	
	// Use this for initialization
	void Start() {
		mLove = UnitConfig.GetData(type).resource;
	}
}
