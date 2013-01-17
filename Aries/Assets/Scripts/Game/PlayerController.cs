using UnityEngine;
using System.Collections;

public class PlayerController : MotionBase {
	public float force;
	
	public ActionTarget followAction;
	
	public string fireProjectile;
	public float fireStartDistance = 0.75f;
	public float fireDelay = 0.5f;
	public float fireAngleSpread = 5.0f;
	
	public float recallRadius = 8.0f;
	public LayerMask recallLayerCheck;
	
	public LayerMask summonLayerCheck;
			
	private enum ActMode {
		Normal,
		Summon,
		UnSummon
	}
	
	private PlayerStat mPlayerStats;
	
	private bool mIsFire = false;
	private float mCurFireTime = 0.0f;
	
	private UnitType[] mTypeSummons = {
		UnitType.sheepMelee, UnitType.sheepRange, UnitType.sheepCarrier, UnitType.sheepHexer };
	
	private ActMode mCurActMode = ActMode.Normal;
	private int mCurSummonInd = 0;
	
	private PlayerCursor mCursor;
	
	private Vector2 mInputDir = Vector2.right;
	
	private UnitEntity mCurSummonUnit = null;
	
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
	
	public void CancelActions() {
		CancelInvoke();
		
		ApplySummon(ActMode.Normal);
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
		
		mPlayerStats = GetComponentInChildren<PlayerStat>();
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
		
		FlockGroup playerGroup = FlockGroup.GetGroup(FlockType.PlayerUnits);
		if(playerGroup != null) {
			playerGroup.addCallback += OnGroupUnitAdd;
			playerGroup.removeCallback += OnGroupUnitRemove;
		}
	}
	
	void OnApplicationFocus(bool focus) {
		if(!focus) {
			ApplySummon(ActMode.Normal);
			mIsFire = false;
		}
	}
	
	void Update() {
		switch(mCurActMode) {
		case ActMode.Normal:
			if(mCurFireTime < fireDelay) {
				mCurFireTime += Time.deltaTime;
			}
			else if(mIsFire) {
				mCurFireTime = 0.0f;
				if(!string.IsNullOrEmpty(fireProjectile)) {
					float rad = fireAngleSpread*Mathf.Deg2Rad;
					Vector2 dir = M8.Math.Rotate(mInputDir, Random.Range(-rad, rad));
					Vector2 start = transform.position;
					start += mInputDir*fireStartDistance;
					
					Projectile.Create(fireProjectile, start, dir);
				}
			}
			break;
			
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
			//mInputDir.Set(moveX, moveY);
			//mInputDir.Normalize();
			mInputDir = dir;
			if(mCursor != null) {
				mCursor.dir = dir;
			}
			
			body.AddForce(moveX*force, moveY*force, 0.0f);
		}
		
		followAction.transform.up = dir;
		
		base.FixedUpdate();
	}
	
	void InputAct(InputManager.Info data) {
		if(data.state == InputManager.State.Pressed) {
			//do something amazing
			Debug.Log("act");
			
			if(cursor.contextSensor.units.Count > 0) {
				PlayerGroup grp = (PlayerGroup)FlockGroup.GetGroup(FlockType.PlayerUnits);
				
				//do something
				Debug.Log("context found: "+cursor.contextSensor.units.Count);
				
				
				ActionTarget target = cursor.contextSensor.GetSingleUnit();
				
				foreach(UnitEntity unit in grp.GetTargetFilter(mTypeSummons[mCurSummonInd], target)) {
					unit.listener.currentTarget = target;
				}
			}
			else if(cursor.attackSensor.units.Count > 0) {
				//attack
			}
			else {
				//recall sheeps
				RecallUnits();
			}
		}
	}
	
	void InputFire(InputManager.Info data) {
		mIsFire = data.state == InputManager.State.Pressed;
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
	
	//at this point, unit is fully spawned
	void OnGroupUnitAdd(FlockUnit unit) {
		ActionListener actionListen = unit.GetComponent<ActionListener>();
		if(actionListen != null) {
			actionListen.defaultTarget = followAction;
		}
		else {
			Debug.LogWarning("no action listener?");
		}
		
		UnitEntity ent = unit.GetComponent<UnitEntity>();
		if(ent != null) {
			//check summoning
			if(ent.prevState == EntityState.spawning) {
				mPlayerStats.curResource -= ent.stats.love;
			}
			
			if(mCurSummonUnit == ent) {
				mCurSummonUnit = null;
			}
		}
		else {
			Debug.LogWarning("no unit entity?");
		}
	}
	
	//unit is also about to be destroyed or released to entity manager
	void OnGroupUnitRemove(FlockUnit unit) {
		ActionListener actionListen = unit.GetComponent<ActionListener>();
		if(actionListen != null) {
			actionListen.defaultTarget = null;
		}
		
		UnitEntity ent = unit.GetComponent<UnitEntity>();
		if(ent != null) {
			//check unsummoning
			if(ent.state == EntityState.unsummon) {
				//refund based on hp percent
				mPlayerStats.curResource += ent.stats.loveHPScale;
			}
			
			if(mCurSummonUnit == ent) {
				mCurSummonUnit = null;
			}
		}
	}
	
	void OnDrawGizmosSelected() {
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireSphere(transform.position, recallRadius);
	}
	
	private void RecallUnits() {
		UnitType type = mCurSummonInd == -1 ? UnitType.NumTypes : mTypeSummons[mCurSummonInd];
		
		Collider[] collides = Physics.OverlapSphere(transform.position, recallRadius, recallLayerCheck.value);
		foreach(Collider col in collides) {
			UnitEntity unit = col.GetComponentInChildren<UnitEntity>();
			if(unit != null) {
				//check type
				if(type == UnitType.NumTypes || unit.stats.type == type) {
					unit.listener.StopAction(ActionTarget.Priority.High, true);
				}
			}
		}
	}
		
	//for both summon/unsummon
	private void ApplySummon(ActMode toMode) {
		if(mCurActMode != toMode) {
			StopSummonAuraFX();
			
			//revert currently selected unsummon
			if(mCurSummonUnit != null && !mCurSummonUnit.isReleased) {
				if(mCurSummonUnit.state == EntityState.unsummon) {
					mCurSummonUnit.state = EntityState.normal;
					mCurSummonUnit.listener.lockAction = false;
				}
				
				mCurSummonUnit = null;
			}
									
			mCurActMode = toMode;
			
			if(mCurActMode != ActMode.Normal) {
				StartSummonAuraFX();
			}
		}
	}
	
	private void GrabSummonUnit() {
		PlayerGroup grp;
		
		switch(mCurActMode) {
		case ActMode.Summon:
			grp = (PlayerGroup)FlockGroup.GetGroup(FlockType.PlayerUnits);
			
			//check if there's enough resource to summon
			//TODO: user feedback
			UnitType unitType = mTypeSummons[mCurSummonInd];
			if(grp.count < mPlayerStats.maxSummon) {
				if(mPlayerStats.curResource >= UnitConfig.instance.GetUnitResourceCost(unitType)) {
					//check if it's safe to summon on the spot
					if(!cursor.CheckArea(summonLayerCheck.value)) {
						string typeName = unitType.ToString();
						EntityManager entMgr = EntityManager.instance;
						mCurSummonUnit = entMgr.Spawn<UnitEntity>(typeName, typeName, null, null);
						if(mCurSummonUnit != null) {
							Vector2 pos = cursor.transform.position;
							mCurSummonUnit.transform.position = pos;
						}
					}
				}
			}
			break;
			
		case ActMode.UnSummon:
			grp = (PlayerGroup)FlockGroup.GetGroup(FlockType.PlayerUnits);
			
			mCurSummonUnit = grp.GrabUnit(mTypeSummons[mCurSummonInd], ActionTarget.Priority.High);
			if(mCurSummonUnit != null) {
				mCurSummonUnit.listener.currentTarget = null;
				mCurSummonUnit.listener.lockAction = true;
				mCurSummonUnit.state = EntityState.unsummon;
			}
			break;
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
}
