using UnityEngine;
using System.Collections;

public class EnemyActionController : FlockActionController {
	public ActionType defaultType = ActionType.Attack;
	
	public EnemySensor sensor;
	public float actionDelay = 1.0f;
	
	private ActionType mActType = ActionType.NumType;
		
	//get the action type, returns target's if mActType = NumType
	public override ActionType type {
		get { return mActType == ActionType.NumType ? base.type : mActType; }
		set { mActType = value; }
	}
	
	public override void SetActive(bool activate) {
		base.SetActive(activate);
		
		if(activate) {
		}
		else {
			RevertActToDefault();
		}
	}
	
	public void RevertActToDefault() {
		mActType = defaultType;
	}
		
	//look for targets in our sensor
	//true if set successfully
	public bool SetTargetToNearest(bool ignorePriority) {
		ActionTarget nearestTarget = null;
		float nearestSqrDist = 0.0f;
		
		Vector2 pos = transform.position;
		
		foreach(EntityBase ent in sensor.items) {
			ActionTarget target = ent.GetComponent<ActionTarget>();
			
			if(target != null) {
				//check if target is not full
				//check if there's a current target and see if priority is ok
				if(target.vacancy && (ignorePriority || currentTarget == null || currentTarget.priority <= target.priority)) {
					Vector2 otherPos = ent.transform.position;
					float distSqr = (otherPos - pos).sqrMagnitude;
					
					if(nearestTarget == null) {
						nearestTarget = target;
						nearestSqrDist = distSqr;
					}
					else if(distSqr < nearestSqrDist) {
						nearestTarget = target;
						nearestSqrDist = distSqr;
					}
				}
			}
		}
		
		if(nearestTarget != null) {
			//stop high priority target if ignorePriority == true
			if(ignorePriority) {
				StopAction(ActionTarget.Priority.Highest, false);
			}
			
			currentTarget = nearestTarget;
			
			return true;
		}
		
		return false;
	}
			
	protected override void OnDestroy() {
		base.OnDestroy ();
		
		
	}
	
	protected override void Awake() {
		base.Awake();
		
		mActType = defaultType;
	}
}
