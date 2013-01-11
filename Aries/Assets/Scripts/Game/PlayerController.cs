using UnityEngine;
using System.Collections;

public class PlayerController : MotionBase {
	public float force;
	
	public ActionTarget followAction;
	public float followActiveDelay = 1.0f;
	
	private float mCurFollowActiveTime = 0.0f;
	
	void OnDestroy() {
		if(Main.instance != null) {
			Main.instance.input.RemoveButtonCall(InputAction.Act, OnAction);
			Main.instance.input.RemoveButtonCall(InputAction.Recall, OnRecall);
		}
	}
	
	protected override void Awake() {
		base.Awake();
	}
	
	// Use this for initialization
	void Start () {
		Main.instance.input.AddButtonCall(InputAction.Act, OnAction);
		Main.instance.input.AddButtonCall(InputAction.Recall, OnRecall);
	}
	
	void Update() {
		if(followAction.sensorOn) {
			mCurFollowActiveTime += Time.deltaTime;
			if(mCurFollowActiveTime >= followActiveDelay) {
				followAction.sensorOn = false;
			}
		}
	}
	
	// Update is called once per frame
	protected override void FixedUpdate () {
		InputManager input = Main.instance.input;
		
		float moveX = input.GetAxis(InputAction.MoveX);
		float moveY = input.GetAxis(InputAction.MoveY);
		
		if(moveX != 0.0f || moveY != 0.0f) {
			body.AddForce(moveX*force, moveY*force, 0.0f);
			
			followAction.transform.up = dir;
		}
		
		base.FixedUpdate();
	}
	
	void OnAction(InputManager.Info data) {
		if(data.state == InputManager.State.Pressed) {
			//do something amazing
		}
	}
	
	void OnRecall(InputManager.Info data) {
		if(data.state == InputManager.State.Pressed) {
			followAction.sensorOn = true;
			mCurFollowActiveTime = 0.0f;
		}
	}
}
