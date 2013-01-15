using UnityEngine;
using System.Collections;

public class Player : EntityBase {
	
	public override void Release() {
		
		base.Release();
	}
	
	protected override void Awake() {
		base.Awake();
		
		PlayerController control = GetComponentInChildren<PlayerController>();
		control.cursor = GameObject.FindObjectOfType(typeof(PlayerCursor)) as PlayerCursor;
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
