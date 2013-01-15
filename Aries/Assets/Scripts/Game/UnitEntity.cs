using UnityEngine;
using System.Collections;

public class UnitEntity : EntityBase {
	
	private UnitStat mStats;
	private FlockUnit mFlockUnit;
	private ActionListener mListener;
	
	public UnitStat stats { get { return mStats; } }
	public FlockUnit flockUnit { get { return mFlockUnit; } }
	public ActionListener listener { get { return mListener; } }
			
	protected override void Awake() {
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
		listener.currentTarget = null;
		
		flockUnit.moveTarget = null;
		
		stats.ResetStats();
		
		FlockUnitRelease();
		
		base.Release();
	}
		
	protected override void StateChanged() {
		switch(state) {
		case EntityState.spawning:
			//spawn started
			break;
		}
	}
	
	protected override void SpawnFinish() {
		//complete
		FlockUnitInit();
	}
	
	protected override void SetBlink(bool blink) {
	}
	
	void OnDestroy() {
		FlockUnitRelease();
	}
	
	// Update is called once per frame
	void LateUpdate () {
	
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
