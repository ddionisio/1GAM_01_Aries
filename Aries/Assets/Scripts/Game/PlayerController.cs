using UnityEngine;
using System.Collections;

public class PlayerController : MotionBase {
	public float force;
	
	public ActionTarget followAction;
	public float followActiveDelay = 1.0f;
	
	public PlayerCursor cursor;
	
	private float mCurFollowActiveTime = 0.0f;
	
	private Vector2 mInputDir = Vector2.zero;
	
	public Vector2 inputDir {
		get { return mInputDir; }
	}
	
	void OnDestroy() {
		if(Main.instance != null) {
			Main.instance.input.RemoveButtonCall(InputAction.Act, OnAction);
			Main.instance.input.RemoveButtonCall(InputAction.Recall, OnRecall);
		}
		
		FlockGroup playerGroup = FlockGroup.GetGroup(FlockType.PlayerUnits);
		if(playerGroup != null) {
			playerGroup.addCallback -= OnGroupUnitAdd;
			playerGroup.removeCallback -= OnGroupUnitRemove;
		}
	}
	
	protected override void Awake() {
		base.Awake();
	}
	
	// Use this for initialization
	void Start () {
		Main.instance.input.AddButtonCall(InputAction.Act, OnAction);
		Main.instance.input.AddButtonCall(InputAction.Recall, OnRecall);
		
		FlockGroup playerGroup = FlockGroup.GetGroup(FlockType.PlayerUnits);
		if(playerGroup != null) {
			playerGroup.addCallback += OnGroupUnitAdd;
			playerGroup.removeCallback += OnGroupUnitRemove;
		}
	}
	
	void Update() {
		//cancel actions of units within while recall is active
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
			mInputDir.Set(moveX, moveY);
			mInputDir.Normalize();
			
			cursor.dir = mInputDir;
			
			body.AddForce(moveX*force, moveY*force, 0.0f);
		}
		
		followAction.transform.up = dir;
		
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
	
	void OnGroupUnitAdd(FlockUnit unit) {
		ActionListener actionListen = unit.GetComponent<ActionListener>();
		if(actionListen != null) {
			actionListen.defaultTarget = followAction;
		}
	}
	
	void OnGroupUnitRemove(FlockUnit unit) {
		ActionListener actionListen = unit.GetComponent<ActionListener>();
		if(actionListen != null) {
			actionListen.defaultTarget = null;
		}
	}
}
