using UnityEngine;
using System.Collections;

public class Player : EntityBase {
	
	protected override void Awake() {
		base.Awake();
	}

	// Use this for initialization
	protected override void Start () {
		base.Start();
		
		CameraController camCtrl = CameraController.instance;
		camCtrl.attachTo = transform;
	}
	
	void LateUpdate () {
	
	}
}
