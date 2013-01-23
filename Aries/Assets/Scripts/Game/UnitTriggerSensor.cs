using UnityEngine;
using System.Collections;
using HutongGames.PlayMaker;

public class UnitTriggerSensor : Sensor<UnitEntity> {
	public string fsmToVar = "trigger"; //which variable to give this crap
	
	public int iValue = 0;
	public float fValue = 0.0f;
	public string sValue = "";
			
	public int filterFlags = 0;
	
	private UnitTriggerEvent mData = new UnitTriggerEvent();

	protected override bool UnitVerify(UnitEntity unit) {
		if(unit.stats == null)
			return false;
		
		return (filterFlags&(1<<((int)unit.stats.type))) != 0;
	}
	
	protected override void UnitAdded(UnitEntity unit) {
		if(unit.FSM != null) {
			FsmObject fsmObj = unit.FSM.FsmVariables.GetFsmObject(fsmToVar);
			if(fsmObj != null) {
				fsmObj.Value = (Object)mData;
			}
			else {
				Debug.LogWarning("Can't find variable "+fsmToVar+" in "+unit.FSM.FsmName);
			}
			
			unit.FSM.SendEvent(EntityEvent.UnitTriggerEnter);
		}
	}
	
	protected override void UnitRemoved(UnitEntity unit) {
		if(unit.FSM != null) {
			FsmObject fsmObj = unit.FSM.FsmVariables.GetFsmObject(fsmToVar);
			if(fsmObj != null) {
				fsmObj.Value = (Object)mData;
			}
			else {
				Debug.LogWarning("Can't find variable "+fsmToVar+" in "+unit.FSM.FsmName);
			}
			
			unit.FSM.SendEvent(EntityEvent.UnitTriggerExit);
		}
	}
	
	void Awake() {
		mData.iValue = iValue;
		mData.fValue = fValue;
		mData.sValue = sValue;
	}
}
