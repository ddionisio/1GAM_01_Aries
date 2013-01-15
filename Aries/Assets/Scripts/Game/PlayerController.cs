using UnityEngine;
using System.Collections;

public class PlayerController : MotionBase {
	public float force;
	
	public ActionTarget followAction;
	public float followActiveDelay = 1.0f;
	
	public PlayerCursor cursor;
	
	private enum ActMode {
		None,
		Summon,
		UnSummon
	}
	
	private float mCurFollowActiveTime = 0.0f;
	
	private UnitType[] mTypeSummons = {
		UnitType.SheepMelee, UnitType.SheepRange, UnitType.SheepCarrier, UnitType.SheepHexer };
	
	private Vector2 mInputDir = Vector2.zero;
	
	public Vector2 inputDir {
		get { return mInputDir; }
	}
	
	void OnDestroy() {
		if(Main.instance != null) {
			Main.instance.input.RemoveButtonCall(InputAction.Act, InputAct);
			Main.instance.input.RemoveButtonCall(InputAction.Recall, InputRecall);
			Main.instance.input.RemoveButtonCall(InputAction.Fire, InputFire);
			Main.instance.input.RemoveButtonCall(InputAction.Item, InputItem);
			Main.instance.input.RemoveButtonCall(InputAction.Summon, InputSummon);
			Main.instance.input.RemoveButtonCall(InputAction.UnSummon, InputUnSummon);
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
		Main.instance.input.AddButtonCall(InputAction.Act, InputAct);
		Main.instance.input.AddButtonCall(InputAction.Recall, InputRecall);
		Main.instance.input.AddButtonCall(InputAction.Fire, InputFire);
		Main.instance.input.AddButtonCall(InputAction.Item, InputItem);
		Main.instance.input.AddButtonCall(InputAction.Summon, InputSummon);
		Main.instance.input.AddButtonCall(InputAction.UnSummon, InputUnSummon);
		
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
	
	void InputAct(InputManager.Info data) {
		if(data.state == InputManager.State.Pressed && GetActMode() == ActMode.None) {
			//do something amazing
			Debug.Log("act");
		}
	}
	
	void InputRecall(InputManager.Info data) {
		if(data.state == InputManager.State.Pressed && GetActMode() == ActMode.None) {
			followAction.sensorOn = true;
			mCurFollowActiveTime = 0.0f;
		}
	}
	
	void InputFire(InputManager.Info data) {
		if(data.state == InputManager.State.Pressed && GetActMode() == ActMode.None) {
			Debug.Log("fire");
		}
	}
	
	void InputItem(InputManager.Info data) {
		if(data.state == InputManager.State.Pressed && GetActMode() == ActMode.None) {
			Debug.Log("item");
		}
	}
	
	void InputSummon(InputManager.Info data) {
		if(data.state == InputManager.State.Pressed && GetActMode() == ActMode.Summon) {
			Debug.Log("summon: "+data.index);
		}
	}
	
	void InputUnSummon(InputManager.Info data) {
		if(data.state == InputManager.State.Pressed && GetActMode() == ActMode.UnSummon) {
			Debug.Log("unsummon: "+data.index);
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
	
	private ActMode GetActMode() {
		InputManager input = Main.instance.input;
		
		ActMode ret;
		
		if(input.IsDown(InputAction.SummonMode)) {
			ret = ActMode.Summon;
		}
		else if(input.IsDown(InputAction.UnSummonMode)) {
			ret = ActMode.UnSummon;
		}
		else {
			ret = ActMode.None;
		}
		
		return ret;
	}
}
