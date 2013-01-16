using UnityEngine;
using System.Collections;

public class UnitEntity : EntityBase {
	
	public float unSummonDelay = 0.5f;
	
	private UnitStat mStats;
	private FlockUnit mFlockUnit;
	private ActionListener mListener;
	
	private float mCurSummonTime = 0.0f;
	
	public UnitStat stats { get { return mStats; } }
	public FlockUnit flockUnit { get { return mFlockUnit; } }
	public ActionListener listener { get { return mListener; } }
			
	protected override void Awake() {
		base.Awake();
		
		mStats = GetComponentInChildren<UnitStat>();
		mFlockUnit = GetComponentInChildren<FlockUnit>();
		mListener = GetComponentInChildren<ActionListener>();
	}
	
	// Use this for initialization
	protected override void Start() {
		base.Start();
		
		//stuff
	}
	
	public override void Release() {
		FlockUnitRelease();
		
		listener.currentTarget = null;
		
		flockUnit.body.velocity = Vector3.zero;
		flockUnit.moveTarget = null;
		flockUnit.sensor.collider.enabled = false;
		flockUnit.sensor.units.Clear();
		
		stats.ResetStats();
						
		base.Release();
	}
		
	protected override void StateChanged() {
		switch(state) {
		case EntityState.spawning:
			//spawn started
			break;
			
		case EntityState.unsummon:
			//fx
			
			mCurSummonTime = 0.0f;
			break;
		}
	}
	
	protected override void SpawnFinish() {
		//complete
		FlockUnitInit();
		
		flockUnit.sensor.collider.enabled = true;
		
		state = EntityState.normal;
	}
	
	protected override void SetBlink(bool blink) {
	}
	
	void OnDestroy() {
		FlockUnitRelease();
	}
	
	// Update is called once per frame
	void LateUpdate () {
		switch(state) {
		case EntityState.unsummon:
			mCurSummonTime += Time.deltaTime;
			if(mCurSummonTime >= unSummonDelay) {
				Release();
			}
			break;
		}
	}
	
	private void FlockUnitInit() {
		//add to group
		//remove from group if it still exists
		if(flockUnit != null) {
			FlockGroup grp = FlockGroup.GetGroup(flockUnit.type);
			if(grp != null) {
				grp.AddUnit(flockUnit);
			}
		}
	}
	
	private void FlockUnitRelease() {
		//remove from group if it still exists
		if(flockUnit != null) {
			FlockGroup grp = FlockGroup.GetGroup(flockUnit.type);
			if(grp != null) {
				grp.RemoveUnit(flockUnit, null);
			}
		}
	}
}
