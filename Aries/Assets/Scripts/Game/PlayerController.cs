using UnityEngine;
using System.Collections;

public class PlayerController : MotionBase {
	public float force;
	
	public FlockSensor followSensor;
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
		
		followSensor.collider.enabled = false;
		followSensor.typeFilter = FlockType.PlayerUnits;
	}
	
	// Use this for initialization
	void Start () {
		Main.instance.input.AddButtonCall(InputAction.Act, OnAction);
		Main.instance.input.AddButtonCall(InputAction.Recall, OnRecall);
	}
	
	void Update() {
		//cancel actions of units within while recall is active
		if(followSensor.collider.enabled) {
			mCurFollowActiveTime += Time.deltaTime;
			if(mCurFollowActiveTime >= followActiveDelay) {
				followSensor.collider.enabled = false;
			}
			else {
				//check flock in sensor
				foreach(FlockUnit unit in followSensor.units) {
					FlockActionController ctrl = unit.GetComponent<FlockActionController>();
					if(ctrl != null && ctrl.curListener != null) {
						ctrl.curListener.StopAction(ActionTarget.Priority.High);
					}
				}
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
			followSensor.collider.enabled = true;
			mCurFollowActiveTime = 0.0f;
		}
	}
}
