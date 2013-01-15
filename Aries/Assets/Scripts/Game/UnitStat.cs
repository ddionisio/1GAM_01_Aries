using UnityEngine;
using System.Collections;

public class UnitStat : StatBase {
	public UnitType type;
	
	[SerializeField] float _love; //cost of unit or what enemies drop
	
	public float love {
		get { return _love; }
	}
	
	public float loveHPScale {
		get { return _love*HPScale; }
	}
	
	protected override void Awake() {
		base.Awake ();
	}
	
	// Use this for initialization
	void Start() {
	
	}
	
	// Update is called once per frame
	void Update() {
	
	}
}
