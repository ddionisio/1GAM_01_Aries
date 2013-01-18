using UnityEngine;
using System.Collections;

public class Player : EntityBase {
	
	public override void Release() {
		PlayerController control = GetComponent<PlayerController>();
		if(control != null) {
			control.CancelActions();
			control.followAction.StopAction();
		}
		
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
		
		PlayerStat stat = GetComponentInChildren<PlayerStat>();
		stat.InitResource();
	}
	
	protected override void StateChanged() {
		switch(state) {
		case EntityState.spawning:
			PlayerStat stat = GetComponentInChildren<PlayerStat>();
			stat.InitResource();
			break;
		}
	}
	
	void LateUpdate () {
	
	}
}
