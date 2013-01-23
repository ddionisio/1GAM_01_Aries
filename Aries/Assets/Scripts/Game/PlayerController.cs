using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : MotionBase {
	public float force;
	
	public ActionTarget followAction;
	public Transform followInputRotate;
	
	public float recallRadius = 8.0f;
	public LayerMask recallLayerCheck;
	
	public float summonRadius = 5.0f;
	public LayerMask summonLayerCheck;
	
	public GameObject recallSprite;
	
	public GameObject attackSprite;
	public Color attackColor;
	public ActionSensor attackSensor; //for all hostile enemies
			
	private enum ActMode {
		Normal,
		Summon,
		UnSummon
	}
	
	private Weapon mWeapon;
	private Weapon.RepeatParam mWeaponParam = new Weapon.RepeatParam();
	
	private Player mPlayer;
	
	private UnitType[] mTypeSummons = {
		UnitType.sheepMelee, UnitType.sheepRange, UnitType.sheepCarrier, UnitType.sheepHexer };
	
	private ActMode mCurActMode = ActMode.Normal;
	private int mCurSummonInd = 0;
	
	private PlayerCursor mCursor;
	
	private UnitEntity mCurSummonUnit = null;
	
	private List<ActionTarget> mTargetHolder = new List<ActionTarget>(10);
	
	public Player player {
		get { return mPlayer; }
	}
	
	public Vector2 inputDir {
		get { return mWeaponParam.dir; }
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
	
	public void CancelActions() {
		CancelInvoke();
		
		ApplySummon(ActMode.Normal);
		
		if(mWeapon != null) {
			mWeapon.Release();
		}
	}
	
	public void SpawnStart() {
		recallSprite.SetActive(false);
		attackSprite.SetActive(false);
		
		if(mCursor != null) {
			mCursor.RevertToNeutral();
		}
	}
	
	void OnDestroy() {
		if(Main.instance != null) {
			Main.instance.input.RemoveButtonCall(InputAction.Act, InputAct);
			Main.instance.input.RemoveButtonCall(InputAction.Fire, InputFire);
			Main.instance.input.RemoveButtonCall(InputAction.Menu, InputMenu);
			Main.instance.input.RemoveButtonCall(InputAction.SummonSelect, InputSummonSelect);
			Main.instance.input.RemoveButtonCall(InputAction.SummonSelectNext, InputSummonSelectNext);
			Main.instance.input.RemoveButtonCall(InputAction.SummonSelectPrev, InputSummonSelectPrev);
			Main.instance.input.RemoveButtonCall(InputAction.Summon, InputSummon);
			Main.instance.input.RemoveButtonCall(InputAction.UnSummon, InputUnSummon);
			Main.instance.input.RemoveButtonCall(InputAction.Recall, InputRecall);
		}
		
		FlockGroup playerGroup = FlockGroup.GetGroup(mPlayer.stats.flockGroup);
		if(playerGroup != null) {
			playerGroup.addCallback -= OnGroupUnitAdd;
			playerGroup.removeCallback -= OnGroupUnitRemove;
		}
		
		if(mCursor != null) {
			mCursor.origin = null;
		}
		
		attackSensor.unitAddRemoveCallback -= OnAttackSensorUnitChange;
	}
	
	protected override void Awake() {
		base.Awake();
		
		mWeapon = GetComponentInChildren<Weapon>();
		mWeaponParam.source = transform;
		
		mPlayer = GetComponentInChildren<Player>();
		
		attackSensor.unitAddRemoveCallback += OnAttackSensorUnitChange;
		
		SpawnStart();
	}
	
	// Use this for initialization
	void Start () {
		Main.instance.input.AddButtonCall(InputAction.Act, InputAct);
		Main.instance.input.AddButtonCall(InputAction.Fire, InputFire);
		Main.instance.input.AddButtonCall(InputAction.Menu, InputMenu);
		Main.instance.input.AddButtonCall(InputAction.SummonSelect, InputSummonSelect);
		Main.instance.input.AddButtonCall(InputAction.SummonSelectNext, InputSummonSelectNext);
		Main.instance.input.AddButtonCall(InputAction.SummonSelectPrev, InputSummonSelectPrev);
		Main.instance.input.AddButtonCall(InputAction.Summon, InputSummon);
		Main.instance.input.AddButtonCall(InputAction.UnSummon, InputUnSummon);
		Main.instance.input.AddButtonCall(InputAction.Recall, InputRecall);
		
		FlockGroup playerGroup = FlockGroup.GetGroup(mPlayer.stats.flockGroup);
		if(playerGroup != null) {
			playerGroup.addCallback += OnGroupUnitAdd;
			playerGroup.removeCallback += OnGroupUnitRemove;
		}
		
		mCursor = PlayerCursor.GetByType(mPlayer.stats.flockGroup);
		if(mCursor != null) {
			mCursor.origin = transform;
		}
	}
	
	void OnApplicationFocus(bool focus) {
		if(!focus) {
			CancelActions();
		}
	}
	
	void Update() {
		switch(mCurActMode) {
		case ActMode.Summon:
		case ActMode.UnSummon:
			if(mCurSummonInd != -1 && mCurSummonUnit == null) {
				//keep finding a unit to summon/unsummon
				GrabSummonUnit();
			}
			break;
		}
	}
	
	// Update is called once per frame
	protected override void FixedUpdate () {
		InputManager input = Main.instance.input;
		
		float moveX = input.GetAxis(InputAction.MoveX);
		float moveY = input.GetAxis(InputAction.MoveY);
		
		if(moveX != 0.0f || moveY != 0.0f) {
			mWeaponParam.dir = dir;
			if(mCursor != null) {
				mCursor.dir = dir;
			}
			
			body.AddForce(moveX*force, moveY*force, 0.0f);
		}
		
		followInputRotate.up = dir;
		
		base.FixedUpdate();
	}
	
	void UpdateAttackSensorDisplay() {
		if(attackSensor.units.Count > 0) {
			if(!attackSprite.activeSelf) {
				attackSprite.SetActive(true);
				cursor.cursorSprite.color = attackColor;
			}
		}
		else if(attackSprite.activeSelf) {
			attackSprite.SetActive(false);
			cursor.cursorSprite.color = cursor.neutralColor;
		}
	}
	
	void OnAttackSensorUnitChange() {
		switch(mCurActMode) {
		case ActMode.Normal:
			UpdateAttackSensorDisplay();
			break;
		}
	}
	
	//fill mTargetHolder and sort to nearest
	void UpdateAttackTargetHolder() {
		mTargetHolder.Clear();
		
		HashSet<ActionTarget> attacks = attackSensor.units;
		if(attacks.Count > 0) {
			Vector2 pos = transform.position;
			
			//get entities and sort to nearest
			
			foreach(ActionTarget attack in attacks) {
				//only grab what can be targetted
				//TODO: other things beyond attack?
				if(attack.vacancy && attack.type == ActionType.Attack) {
					Vector2 entPos = attack.transform.position;
					attack.distSqrHolder = (entPos - pos).sqrMagnitude;
					mTargetHolder.Add(attack);
				}
			}
			
			mTargetHolder.Sort((x, y) => (int)(x.distSqrHolder - y.distSqrHolder));
		}
	}
	
	void InputAct(InputManager.Info data) {
		if(data.state == InputManager.State.Pressed) {
			//do something amazing
			Debug.Log("act");
			
			HashSet<ActionTarget> contexts = cursor.contextSensor.units;
			
			PlayerGroup grp = (PlayerGroup)FlockGroup.GetGroup(mPlayer.stats.flockGroup);
			
			if(contexts.Count > 0) {
				//do something
				Debug.Log("context found: "+contexts.Count);
				
				ActionTarget target = cursor.contextSensor.GetSingleUnit();
				
				foreach(UnitEntity unit in grp.GetTargetFilter(target)) {
					unit.listener.currentTarget = target;
				}
			}
			
			UpdateAttackTargetHolder();
			if(mTargetHolder.Count > 0) {
				int curTargetInd = 0;
				
				ActionTarget target = mTargetHolder[curTargetInd];
				
				//go through all units
				//TODO: check distance of unit to target?
				foreach(UnitEntity unit in grp.GetUnits()) {
					ActionListener listener = unit.listener;
					
					//check if unit can attack target
					if(!listener.lockAction && listener.currentPriority <= target.priority) {
						StatBase targetStats = target.GetComponentInChildren<StatBase>();
						if(targetStats == null || unit.stats.CanDamage(targetStats)) {
							unit.listener.currentTarget = target;
							
							//get next target once vacancy is full
							if(!target.vacancy) {
								curTargetInd++;
								if(curTargetInd < mTargetHolder.Count) {
									target = mTargetHolder[curTargetInd];
								}
								else { //done with setting targets
									break;
								}
							}
						}
					}
				}
			}
		}
	}
	
	void InputFire(InputManager.Info data) {
		if(mWeapon != null) {
			if(data.state == InputManager.State.Pressed) {
				mWeapon.Repeat(mWeaponParam);
				player.state = EntityState.attacking;
			}
			else {
				mWeapon.RepeatStop();
				player.state = EntityState.normal;
			}
		}
	}
	
	void InputMenu(InputManager.Info data) {
		if(data.state == InputManager.State.Pressed) {
			Debug.Log("item");
		}
	}
	
	void InputSummon(InputManager.Info data) {
		if(data.state == InputManager.State.Pressed) {
			Debug.Log("summon: "+mCurSummonInd);
			ApplySummon(ActMode.Summon);
		}
		else if(mCurActMode == ActMode.Summon) {
			Debug.Log("summon cancel: "+mCurSummonInd);
			ApplySummon(ActMode.Normal);
		}
	}
	
	void InputUnSummon(InputManager.Info data) {
		if(data.state == InputManager.State.Pressed) {
			Debug.Log("unsummon: "+mCurSummonInd);
			ApplySummon(ActMode.UnSummon);
		}
		else if(mCurActMode == ActMode.UnSummon) {
			Debug.Log("unsummon cancel: "+mCurSummonInd);
			ApplySummon(ActMode.Normal);
		}
	}
	
	void InputSummonSelect(InputManager.Info data) {
		if(data.state == InputManager.State.Pressed) {
			int ind = data.index % mTypeSummons.Length;
			if(mTypeSummons[ind] != UnitType.NumTypes) {
				mCurSummonInd = ind;
			}
		}
	}
	
	void InputSummonSelectPrev(InputManager.Info data) {
		if(data.state == InputManager.State.Pressed) {
			mCurSummonInd = (mCurSummonInd-1)%mTypeSummons.Length;
			
			for(int i = 0; 
				i < mTypeSummons.Length || mTypeSummons[mCurSummonInd] == UnitType.NumTypes; 
				mCurSummonInd = (mCurSummonInd-1)%mTypeSummons.Length, i++);
		}
	}
	
	void InputSummonSelectNext(InputManager.Info data) {
		if(data.state == InputManager.State.Pressed) {
			mCurSummonInd = (mCurSummonInd+1)%mTypeSummons.Length;
			
			for(int i = 0; 
				i < mTypeSummons.Length || mTypeSummons[mCurSummonInd] == UnitType.NumTypes; 
				mCurSummonInd = (mCurSummonInd+1)%mTypeSummons.Length, i++);
		}
	}
	
	void InputRecall(InputManager.Info data) {
		if(data.state == InputManager.State.Pressed) {
			RecallUnits();
		}
	}
	
	//at this point, unit is fully spawned
	void OnGroupUnitAdd(FlockUnit flockUnit) {
		UnitEntity unit = flockUnit.GetComponent<UnitEntity>();
		if(unit != null) {
			FlockActionController actionListen = unit.listener as FlockActionController;
			
			//auto attack if there's an enemy nearby
			if(actionListen != null) {
				UpdateAttackTargetHolder();
				if(mTargetHolder.Count > 0) {
					foreach(ActionTarget target in mTargetHolder) {
						//check if unit can attack target
						if(!actionListen.lockAction && actionListen.currentPriority <= target.priority) {
							//can damage? then target this
							StatBase targetStats = target.GetComponentInChildren<StatBase>();
							if(targetStats == null || unit.stats.CanDamage(targetStats)) {
								actionListen.currentTarget = target;
								break;
							}
						}
					}
				}
				
				actionListen.defaultTarget = followAction;
				actionListen.leader = transform;
			} else {
				Debug.LogWarning("no action listener?");
			}
		}
		else {
			Debug.LogWarning("no unit?");
		}
		
		UnitEntity ent = flockUnit.GetComponent<UnitEntity>();
		if(mCurSummonUnit == ent) {
			//check summoning
			mCurSummonUnit = null;
		}
	}
	
	//unit is also about to be destroyed or released to entity manager
	void OnGroupUnitRemove(FlockUnit unit) {
		FlockActionController actionListen = unit.GetComponent<FlockActionController>();
		if(actionListen != null) {
			actionListen.defaultTarget = null;
			actionListen.leader = null;
		}
		
		UnitEntity ent = unit.GetComponent<UnitEntity>();
		if(ent != null) {
			//check unsummoning
			if(mCurActMode == ActMode.UnSummon && mCurSummonUnit == ent) {
				//refund based on hp percent
				mPlayer.stats.curResource += ent.stats.loveHPScale;
				
				mCurSummonUnit = null;
			}
		}
	}
	
	void OnDrawGizmosSelected() {
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireSphere(transform.position, recallRadius);
		
		Gizmos.color *= 0.65f;
		Gizmos.DrawWireSphere(transform.position, summonRadius);
	}
			
	private void RecallUnits() {
		if(recallSprite != null && !recallSprite.activeSelf) {
			recallSprite.SetActive(true);
		}
		
		Collider[] collides = Physics.OverlapSphere(transform.position, recallRadius, recallLayerCheck.value);
		foreach(Collider col in collides) {
			UnitEntity unit = col.GetComponentInChildren<UnitEntity>();
			if(unit != null) {
				//check type
				unit.listener.StopAction(ActionTarget.Priority.High, true);
			}
		}
	}
		
	//for both summon/unsummon
	private void ApplySummon(ActMode toMode) {
		if(mCurActMode != toMode) {
			//revert previous
			switch(mCurActMode) {
			case ActMode.Normal:
				attackSprite.SetActive(false);
				mCursor.RevertToNeutral();
				break;
				
			case ActMode.Summon:
				mCurSummonUnit = null;
				break;
				
			case ActMode.UnSummon:
				//revert currently selected unsummon
				if(mCurSummonUnit != null && !mCurSummonUnit.isReleased) {
					mCurSummonUnit.FSM.SendEvent(EntityEvent.Resume);
					mCurSummonUnit = null;
				}
				break;
			}
			
			mCurActMode = toMode;
			
			switch(mCurActMode) {
			case ActMode.Normal:
				player.state = EntityState.normal;
				UpdateAttackSensorDisplay();
				break;
				
			case ActMode.Summon:
				player.state = EntityState.castSummon;
				break;
				
			case ActMode.UnSummon:
				player.state = EntityState.castUnSummon;
				break;
			}
		}
	}
	
	private void GrabSummonUnit() {
		PlayerGroup grp;
		
		switch(mCurActMode) {
		case ActMode.Summon:
			grp = (PlayerGroup)FlockGroup.GetGroup(mPlayer.stats.flockGroup);
			
			//check if there's enough resource to summon
			//TODO: user feedback
			UnitType unitType = mTypeSummons[mCurSummonInd];
			if(grp.count < mPlayer.stats.maxSummon) {
				float love = UnitConfig.instance.GetUnitResourceCost(unitType);
				if(mPlayer.stats.curResource >= love) {
					//check if it's safe to summon on the spot
					//summonRadius Physics.CheckSphere(transform.position, radius, layerMask)
					Vector2 pos = transform.position;
					pos += Random.insideUnitCircle*summonRadius;
					if(!Physics.CheckSphere(pos, cursor.radius, summonLayerCheck.value)) {
						string typeName = unitType.ToString();
						EntityManager entMgr = EntityManager.instance;
						mCurSummonUnit = entMgr.Spawn<UnitEntity>(typeName, typeName, null, null);
						if(mCurSummonUnit != null) {
							mCurSummonUnit.transform.position = pos;
							
							//subtract from player resource
							mPlayer.stats.curResource -= love;
						}
					}
				}
			}
			break;
			
		case ActMode.UnSummon:
			grp = (PlayerGroup)FlockGroup.GetGroup(mPlayer.stats.flockGroup);
			
			mCurSummonUnit = grp.GrabUnit(mTypeSummons[mCurSummonInd], ActionTarget.Priority.High);
			if(mCurSummonUnit != null) {
				mCurSummonUnit.FSM.SendEvent(EntityEvent.Remove);
			}
			break;
		}
	}
}
