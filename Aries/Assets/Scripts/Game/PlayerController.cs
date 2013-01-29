using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : MotionBase {
	public const int numSlots = 4;
	
	public float force;
	
	public ActionTarget followAction;
	public Transform followInputRotate;
	
	public float recallRadius = 8.0f;
	
	public float summonRadius = 5.0f;
	public LayerMask summonLayerCheck;
	
	public GameObject recallSprite;
	public float recallDelay = 2.0f;
	public LayerMask recallLayerCheck;
	
	public GameObject attackSprite;
	public Color attackColor;
	public ActionSensor attackSensor; //for all hostile enemies
			
	private enum ActMode {
		Normal,
		Summon,
		UnSummon
	}
	
	private struct SummonSlot {
		public UnitConfig.Data data;
		
		private float curTime;
		private int numQueueSummon;
		
		public bool isReady { get { return curTime >= data.summonCooldown; } }
		
		public float cooldownScale { get { return curTime >= data.summonCooldown ? 1.0f : curTime/data.summonCooldown; } }
		
		public int GetNumSummonable(FlockType flock) {
			PlayerGroup grp = (PlayerGroup)FlockGroup.GetGroup(flock);
			return data.summonMax - grp.GetUnitCountByType(data.type) - numQueueSummon;
		}
		
		//call this to subtract 1 after being able to summon in queue
		public void UpdateSummonQueueAmount(int amt) {
			numQueueSummon += amt;
			if(numQueueSummon < 0)
				numQueueSummon = 0;
		}
		
		/// <summary>
		/// Computes number of summons to make and subtract the resource from stat. returns 0 if can't afford.
		/// </summary>
		public int ApplySummon(PlayerStat stat) {
			if(isReady) {
				int available = GetNumSummonable(stat.flockGroup);
				
				int numSummonable = available > data.summonAmount ? data.summonAmount : available;
				
				float cost = data.resource*((float)numSummonable);
				
				//adjust cost and summon amount if too expensive
				if(cost > stat.curResource) {
					numSummonable = Mathf.FloorToInt(stat.curResource/data.resource);
					cost = data.resource*((float)numSummonable);
				}
				
				if(numSummonable > 0) {
					//assumes there's nothing that will cancel the summon queue while player is alive
					stat.curResource -= cost;
					
					UpdateSummonQueueAmount(numSummonable);
					
					curTime = 0;
					
					return numSummonable;
				}
			}
			
			return 0;
		}
		
		public void Update() {
			if(curTime < data.summonCooldown)
				curTime += Time.deltaTime;
		}
		
		public void Reset() {
			curTime = data.summonCooldown;
			numQueueSummon = 0;
		}
		
		//call during start
		public void Init(UnitType type) {
			data = UnitConfig.GetData(type);
			Reset();
		}
	}
	
	private Weapon mWeapon;
	private Weapon.RepeatParam mWeaponParam = new Weapon.RepeatParam();
	
	private Player mPlayer;
	
	private SummonSlot[] mTypeSummons = new SummonSlot[numSlots];
	
	private Queue<UnitType> mSummonQueue = new Queue<UnitType>(50);
	
	private ActMode mCurActMode = ActMode.Normal;
	private int mCurSummonInd = 0;
	
	private PlayerCursor mCursor;
	
	private UnitEntity mCurUnSummonUnit = null;
	
	private List<ActionTarget> mTargetHolder = new List<ActionTarget>(10);
	
	private bool mAutoAttack = true;
	
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
	
	public bool autoAttack {
		get { return mAutoAttack; }
		set {
			if(mAutoAttack != value) {
				mAutoAttack = value;
				
				//attack/clear currently
				//go through and (de)activate attack sensors
				PlayerGroup grp = (PlayerGroup)FlockGroup.GetGroup(mPlayer.stats.flockGroup);
				foreach(UnitEntity unit in grp.GetUnits()) {
					if(unit != null && unit.listener != null) {
						FlockActionController listener = unit.listener as FlockActionController;
						if(listener != null) {
							listener.autoAttack = mAutoAttack;
							listener.autoSpell = mAutoAttack;
						}
					}
				}
			}
		}
	}
	
	public void CancelActions() {
		ApplySummon(ActMode.Normal);
		
		if(mWeapon != null) {
			mWeapon.Release();
		}
	}
	
	public void CancelRecall() {
		if(followAction.type == ActionType.Retreat) {
			followAction.type = ActionType.Follow;
			recallSprite.SetActive(false);
			StopCoroutine("RecallDelay");
		}
	}
	
	public void ClearSummonQueue() {
		mSummonQueue.Clear();
	}
	
	public void SpawnStart() {
		recallSprite.SetActive(false);
		attackSprite.SetActive(false);
		
		if(mCursor != null) {
			mCursor.RevertToNeutral();
		}
		
		mAutoAttack = true;
		
		foreach(SummonSlot slot in mTypeSummons) {
			slot.Reset();
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
		
		//TODO: get config from user data
		mTypeSummons[0].Init(UnitType.sheepMelee);
		mTypeSummons[1].Init(UnitType.sheepRange);
		mTypeSummons[2].Init(UnitType.sheepAssault);
		mTypeSummons[3].Init(UnitType.sheepHexer);
		
		SpawnStart();
	}
	
	void OnApplicationFocus(bool focus) {
		if(!focus) {
			CancelActions();
		}
	}
	
	void Update() {
		switch(mCurActMode) {
		case ActMode.UnSummon:
			if(mCurSummonInd != -1 && mCurUnSummonUnit == null) {
				//keep finding a unit to unsummon
				GrabUnSummonUnit();
			}
			break;
		}
		
		//keep summoning from queue
		if(mSummonQueue.Count > 0) {
			//attempt to summon on current area, and if success, 
			//dequeue and update summon slot
			UnitType unitType = mSummonQueue.Peek();
			if(GrabSummonUnit(unitType)) {
				mSummonQueue.Dequeue();
				mTypeSummons[(int)unitType].UpdateSummonQueueAmount(-1);
			}
		}
		
		foreach(SummonSlot slot in mTypeSummons) {
			slot.Update();
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
		if(attackSensor.items.Count > 0) {
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
		
		HashSet<ActionTarget> attacks = attackSensor.items;
		if(attacks.Count > 0) {
			Vector2 pos = transform.position;
			
			//get entities and sort to nearest
			
			foreach(ActionTarget attack in attacks) {
				//only grab what can be targetted
				//TODO: other things beyond attack?
				if(attack != null && attack.vacancy && attack.type == ActionType.Attack) {
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
			
			HashSet<ActionTarget> contexts = cursor.contextSensor.items;
			
			PlayerGroup grp = (PlayerGroup)FlockGroup.GetGroup(mPlayer.stats.flockGroup);
			
			if(contexts.Count > 0) {
				//do something
				Debug.Log("context found: "+contexts.Count);
				
				//only interact with one context...
				ActionTarget target = cursor.contextSensor.GetSingleUnit();
				
				foreach(UnitEntity unit in grp.GetTargetFilter(target)) {
					if(unit != null)
						unit.listener.currentTarget = target;
				}
			}
			
			UpdateAttackTargetHolder();
			if(mTargetHolder.Count > 0) {
				//cancel retreat
				CancelRecall();
				
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
			
			DoSummonFromCurrentSelect();
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
			if(mTypeSummons[ind].data != null) {
				mCurSummonInd = ind;
				DoSummonFromCurrentSelect();
			}
		}
	}
	
	void InputSummonSelectPrev(InputManager.Info data) {
		if(data.state == InputManager.State.Pressed) {
			mCurSummonInd = (mCurSummonInd-1)%mTypeSummons.Length;
			
			for(int i = 0; 
				i < mTypeSummons.Length || mTypeSummons[mCurSummonInd].data == null; 
				mCurSummonInd = (mCurSummonInd-1)%mTypeSummons.Length, i++);
		}
	}
	
	void InputSummonSelectNext(InputManager.Info data) {
		if(data.state == InputManager.State.Pressed) {
			mCurSummonInd = (mCurSummonInd+1)%mTypeSummons.Length;
			
			for(int i = 0; 
				i < mTypeSummons.Length || mTypeSummons[mCurSummonInd].data == null; 
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
			
			if(actionListen != null) {
				actionListen.autoAttack = autoAttack;
				actionListen.autoSpell = autoAttack;
				
				actionListen.defaultTarget = followAction;
				actionListen.leader = transform;
			} else {
				Debug.LogWarning("no action listener?");
			}
		}
		else {
			Debug.LogWarning("no unit?");
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
			if(mCurActMode == ActMode.UnSummon && mCurUnSummonUnit == ent) {
				//refund based on hp percent
				mPlayer.stats.curResource += ent.stats.loveHPScale;
				
				mCurUnSummonUnit = null;
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
		//set to retreat to avoid auto attack
		CancelRecall();
						
		Collider[] collides = Physics.OverlapSphere(transform.position, recallRadius, recallLayerCheck.value);
		foreach(Collider col in collides) {
			UnitEntity unit = col.GetComponentInChildren<UnitEntity>();
			if(unit != null) {
				//check type
				unit.listener.StopAction(ActionTarget.Priority.High, true);
			}
		}
		
		StartCoroutine("RecallDelay");
	}
	
	private IEnumerator RecallDelay() {
		if(recallSprite != null) {
			recallSprite.SetActive(true);
		}
		
		followAction.type = ActionType.Retreat;
		
		yield return new WaitForSeconds(recallDelay);
		
		if(recallSprite != null) {
			recallSprite.SetActive(false);
		}
		
		followAction.type = ActionType.Follow;
		
		yield break;
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
				break;
				
			case ActMode.UnSummon:
				//revert currently selected unsummon
				if(mCurUnSummonUnit != null && !mCurUnSummonUnit.isReleased) {
					mCurUnSummonUnit.FSM.SendEvent(EntityEvent.Resume);
					mCurUnSummonUnit = null;
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
	
	private void DoSummonFromCurrentSelect() {
		int numToQueue = mTypeSummons[mCurSummonInd].ApplySummon(mPlayer.stats);
		if(numToQueue > 0) {
			for(int i = 0; i < numToQueue; i++) {
				mSummonQueue.Enqueue(mTypeSummons[mCurSummonInd].data.type);
			}
		}
		else {
			//TODO: player feedback (error sound)
		}
	}
	
	private bool GrabSummonUnit(UnitType unitType) {
		//check if it's safe to summon on the spot
		//summonRadius Physics.CheckSphere(transform.position, radius, layerMask)
		Vector2 pos = transform.position;
		pos += Random.insideUnitCircle*summonRadius;
		if(!Physics.CheckSphere(pos, cursor.radius, summonLayerCheck.value)) {
			string typeName = unitType.ToString();
			EntityManager entMgr = EntityManager.instance;
			UnitEntity ent = entMgr.Spawn<UnitEntity>(typeName, typeName, null, null);
			if(ent != null) {
				ent.transform.position = pos;
				return true;
			}
		}
		
		return false;
	}
	
	private void GrabUnSummonUnit() {
		PlayerGroup grp;
		
		grp = (PlayerGroup)FlockGroup.GetGroup(mPlayer.stats.flockGroup);
		
		mCurUnSummonUnit = grp.GrabUnit(mTypeSummons[mCurSummonInd].data.type, ActionTarget.Priority.High);
		if(mCurUnSummonUnit != null) {
			mCurUnSummonUnit.FSM.SendEvent(EntityEvent.Remove);
		}
	}
}
