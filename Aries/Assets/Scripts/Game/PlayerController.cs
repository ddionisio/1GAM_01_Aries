using UnityEngine;
using System.Collections;

public class PlayerController : MotionBase {
	public float force;
	
	public ActionTarget followAction;
	public float followActiveDelay = 1.0f;
	public float summonDelay = 1.0f;
			
	private enum ActMode {
		Normal,
		Summon,
		UnSummon
	}
	
	private float mCurFollowActiveTime = 0.0f;
	
	private UnitType[] mTypeSummons = {
		UnitType.sheepMelee, UnitType.sheepRange, UnitType.sheepCarrier, UnitType.sheepHexer };
	
	private ActMode mCurActMode = ActMode.Normal;
	private int mCurSummonInd = -1;
	private float mCurSummonTime = 0.0f;
	
	private PlayerCursor mCursor;
	
	private Vector2 mInputDir = Vector2.zero;
	
	public Vector2 inputDir {
		get { return mInputDir; }
	}
	
	public PlayerCursor cursor {
		get { return mCursor; }
		set {
			if(mCursor != null) {
				mCursor.origin = null;
			}
			
			mCursor = value;
			
			if(mCursor != null) {
				mCursor.origin = transform;
			}
		}
	}
	
	void OnDestroy() {
		if(Main.instance != null) {
			Main.instance.input.RemoveButtonCall(InputAction.Act, InputAct);
			Main.instance.input.RemoveButtonCall(InputAction.Recall, InputRecall);
			Main.instance.input.RemoveButtonCall(InputAction.Fire, InputFire);
			Main.instance.input.RemoveButtonCall(InputAction.Menu, InputMenu);
			Main.instance.input.RemoveButtonCall(InputAction.Summon, InputSummon);
			Main.instance.input.RemoveButtonCall(InputAction.UnSummon, InputUnSummon);
		}
		
		FlockGroup playerGroup = FlockGroup.GetGroup(FlockType.PlayerUnits);
		if(playerGroup != null) {
			playerGroup.addCallback -= OnGroupUnitAdd;
			playerGroup.removeCallback -= OnGroupUnitRemove;
		}
		
		cursor = null;
	}
	
	protected override void Awake() {
		base.Awake();
	}
	
	// Use this for initialization
	void Start () {
		Main.instance.input.AddButtonCall(InputAction.Act, InputAct);
		Main.instance.input.AddButtonCall(InputAction.Recall, InputRecall);
		Main.instance.input.AddButtonCall(InputAction.Fire, InputFire);
		Main.instance.input.AddButtonCall(InputAction.Menu, InputMenu);
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
		
		if(mCurSummonInd != -1) {
			mCurSummonTime += Time.deltaTime;
			if(mCurSummonTime >= summonDelay) {
				switch(mCurActMode) {
				case ActMode.Summon:
					break;
					
				case ActMode.UnSummon:
					break;
				}
				
				//continue summoning
				mCurSummonTime = 0.0f;
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
			
			if(mCursor != null) {
				mCursor.dir = mInputDir;
			}
			
			body.AddForce(moveX*force, moveY*force, 0.0f);
		}
		
		followAction.transform.up = dir;
		
		base.FixedUpdate();
	}
	
	void InputAct(InputManager.Info data) {
		UpdateActMode();
		
		if(data.state == InputManager.State.Pressed && mCurActMode == ActMode.Normal) {
			//do something amazing
			Debug.Log("act");
		}
	}
	
	void InputRecall(InputManager.Info data) {
		UpdateActMode();
		
		if(data.state == InputManager.State.Pressed && mCurActMode == ActMode.Normal) {
			followAction.sensorOn = true;
			mCurFollowActiveTime = 0.0f;
		}
	}
	
	void InputFire(InputManager.Info data) {
		UpdateActMode();
		
		if(data.state == InputManager.State.Pressed && mCurActMode == ActMode.Normal) {
			Debug.Log("fire");
		}
	}
	
	void InputMenu(InputManager.Info data) {
		UpdateActMode();
		
		if(data.state == InputManager.State.Pressed && mCurActMode == ActMode.Normal) {
			Debug.Log("item");
		}
	}
	
	void InputSummon(InputManager.Info data) {
		UpdateActMode();
		
		if(mCurActMode == ActMode.Summon) {
			if(data.state == InputManager.State.Pressed) {
				Debug.Log("summon: "+data.index);
				
				ApplySummonIndex(data.index);
			}
			else if(data.index == mCurSummonInd) {
				Debug.Log("summon cancel: "+data.index);
				
				ApplySummonIndex(-1);
			}
		}
	}
	
	void InputUnSummon(InputManager.Info data) {
		UpdateActMode();
		
		if(mCurActMode == ActMode.UnSummon) {
			if(data.state == InputManager.State.Pressed) {
				Debug.Log("unsummon: "+data.index);
				
				ApplySummonIndex(data.index);
			}
			else if(data.index == mCurSummonInd) {
				Debug.Log("unsummon cancel: "+data.index);
				
				ApplySummonIndex(-1);
			}
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
	
	private void UpdateActMode() {
		InputManager input = Main.instance.input;
		
		if(input.IsDown(InputAction.SummonMode)) {
			ApplySummon(ActMode.Summon);
		}
		else if(input.IsDown(InputAction.UnSummonMode)) {
			ApplySummon(ActMode.UnSummon);
		}
		else {
			ApplySummon(ActMode.Normal);
		}
	}
	
	//for both summon/unsummon
	private void ApplySummon(ActMode toMode) {
		if(mCurActMode != toMode) {
			StopSummonAuraFX();
			StopSummonFX();
			
			mCurActMode = toMode;
			mCurSummonInd = -1;
			
			StartSummonAuraFX();
		}
	}
	
	private void ApplySummonIndex(int ind) {
		if(mCurSummonInd != ind) {
			StopSummonFX();
			
			mCurSummonInd = ind;
			
			StartSummonFX();
		}
	}
	
	private void StartSummonAuraFX() {
		switch(mCurActMode) {
		case ActMode.Summon:
			break;
			
		case ActMode.UnSummon:
			break;
		}
	}
	
	private void StopSummonAuraFX() {
		switch(mCurActMode) {
		case ActMode.Summon:
			break;
			
		case ActMode.UnSummon:
			break;
		}
	}
	
	private void StartSummonFX() {
		mCurSummonTime = 0.0f;
		
		if(mCurSummonInd != -1) {
			switch(mCurActMode) {
			case ActMode.Summon:
				break;
				
			case ActMode.UnSummon:
				break;
			}
		}
	}
	
	private void StopSummonFX() {
		switch(mCurActMode) {
		case ActMode.Summon:
			break;
			
		case ActMode.UnSummon:
			break;
		}
	}
}
