using UnityEngine;
using System.Collections;

//collider is used for following, assume it's trigger
public class FlockActionController : ActionListener {
	public FlockUnit flockUnit;
	
	public float actionMaxRadius = 6.75f;
	public float actionCancelRadius = 12.0f; //range to move back to leader
	public float actionCancelDelay = 1.0f; //range to cancel attack if too far from leader
	
	public float followStopDelay = 1.0f;
	public float followStopRadius = 2.0f;
	public float followStopSpeed = 0.01f;
	
	public bool attackStartEnable = false;
	[SerializeField] AttackSensor attackSensor;
	
	public bool spellStartEnable = false;
	[SerializeField] SpellSensor spellSensor;
	
	[System.NonSerializedAttribute] public Transform leader = null;
	
	private MotionBase mTargetMotion = null;
	
	public bool autoAttack {
		get { return attackSensor != null ? attackSensor.gameObject.activeSelf : false; }
		set { 
			if(attackSensor != null) {
				bool isActive = attackSensor.gameObject.activeSelf;
				if(isActive != value) {
					if(!value) {
						//clear out attack target
						if(currentTarget != null && type == ActionType.Attack) {
							StopAction(ActionTarget.Priority.Highest, true);
						}
					}
					
					attackSensor.gameObject.SetActive(value);
				}
			}
		}
	}
	
	public bool autoSpell {
		get { return spellSensor != null ? spellSensor.gameObject.activeSelf : false; }
		set { 
			if(spellSensor != null) {
				bool isActive = spellSensor.gameObject.activeSelf;
				if(isActive != value) {
					if(!value) {
						//clear out attack target
						if(currentTarget != null && type == ActionType.Attack) {
							StopAction(ActionTarget.Priority.Highest, true);
						}
					}
					
					spellSensor.gameObject.SetActive(value);
				}
			}
		}
	}
			
	// Use this to determine if we can attack in range
	public override bool CheckRange() {
		if(currentTarget != null) {
			switch(type) {
			case ActionType.Attack:
				return attackSensor != null && flockUnit.enabled && attackSensor.CheckRange(flockUnit.dir, currentTarget.transform);
				
			default:
				break;
			}
		}
		
		return true;
	}
	
	public override bool CheckSpellRange() {
		if(currentTarget != null) {
			switch(type) {
			case ActionType.Attack:
				return spellSensor != null && flockUnit.enabled && spellSensor.CheckRange(currentTarget.transform);
				
			default:
				break;
			}
		}
		
		return true;
	}
	
	public override void SetActive(bool activate) {
		base.SetActive(activate);
		
		if(!activate) {
			ResetAutoActions();
		}
	}
	
	protected override void OnActionEnter() {
		mTargetMotion = currentTarget.GetComponent<MotionBase>();
		
		switch(type) {
		case ActionType.Disperse:
			break;
			
		case ActionType.Attack:
			flockUnit.moveTarget = currentTarget.transform;
						
			if(gameObject.activeInHierarchy)
				StartCoroutine("ReturnToLeader");
			
			//no need to constantly check
			if(attackSensor != null) {
				attackSensor.enabled = false;
			}
			
			if(spellSensor != null) {
				spellSensor.enabled = false;
			}
			break;
			
		case ActionType.Retreat:
		case ActionType.Follow:
			flockUnit.moveTarget = currentTarget.target;
			
			if(gameObject.activeInHierarchy)
				StartCoroutine("FollowStop");
			break;
			
		default:
			flockUnit.moveTarget = currentTarget.target;
			break;
		}
	}
	
	protected override void OnActionExit() {
	}
	
	protected override void OnActionFinish() {
		//do something
		flockUnit.Stop();
		
		mTargetMotion = null;
		
		if(attackSensor != null) { //renabled after attack
			attackSensor.enabled = true;
		}
		
		if(spellSensor != null) {
			spellSensor.enabled = true;
		}
		
		StopCoroutine("FollowStop");
		StopCoroutine("ReturnToLeader");
	}
	
	protected override void OnDestroy ()
	{
		if(attackSensor != null) {
			attackSensor.stayCallback -= AutoAttackCheck;
		}
		
		if(spellSensor != null) {
			spellSensor.stayCallback -= AutoSpellCheck;
		}
		
		base.OnDestroy ();
	}
	
	protected virtual void Awake() {
		ResetAutoActions();
		
		if(attackSensor != null) {
			attackSensor.stayCallback += AutoAttackCheck;
		}
		
		if(spellSensor != null) {
			spellSensor.stayCallback += AutoSpellCheck;
		}
	}
	
	protected virtual void AutoAttackCheck(UnitEntity unit) {
		if(!(type == ActionType.Attack || type == ActionType.Retreat)) {
			ActionTarget target = unit.actionTarget;
			if(target != null && target.type == ActionType.Attack && target.vacancy && currentPriority <= target.priority) {
				currentTarget = target;
				
				flockUnit.minMoveTargetDistance = attackSensor.minRange;
			}
		}
	}
	
	protected virtual void AutoSpellCheck(UnitEntity unit) {
		if(!(type == ActionType.Attack || type == ActionType.Retreat)) {
			ActionTarget target = unit.actionTarget;
			if(target != null && target.type == ActionType.Attack && target.vacancy && currentPriority <= target.priority) {
				currentTarget = target;
				
				flockUnit.minMoveTargetDistance = 0.0f;
			}
		}
	}
	
	IEnumerator ReturnToLeader() {
		//see if we are too far from leader
		float radiusCancelSqr = actionCancelRadius*actionCancelRadius;
		float radiusMoveBackSqr = actionMaxRadius*actionMaxRadius;
		
		while(!lockAction && currentTarget != null && currentTarget != defaultTarget && leader != null) {
			yield return new WaitForSeconds(actionCancelDelay);
			
			Vector2 pos = transform.position;
			Vector2 leaderPos = leader.position;
			float distSqr = (leaderPos - pos).sqrMagnitude;
			
			if(distSqr > radiusCancelSqr) {
				StopAction(ActionTarget.Priority.High, true);
			}
			else if(distSqr > radiusMoveBackSqr && !lockAction) {
				flockUnit.moveTarget = leader;
			}
			else {
				flockUnit.moveTarget = currentTarget.transform;
			}
		}
	}
	
	IEnumerator FollowStop() {
		while(currentTarget != null && mTargetMotion != null) {
			yield return new WaitForSeconds(followStopDelay);
			
			switch(type) {
			case ActionType.Retreat:
			case ActionType.Follow:
				if(mTargetMotion.curSpeed < followStopSpeed) {
					if(flockUnit.moveTarget != null && flockUnit.moveTargetDistance <= followStopRadius)
						flockUnit.moveTarget = null;
				}
				else {
					flockUnit.moveTarget = currentTarget.target;
				}
				
				yield return new WaitForFixedUpdate();
				break;
				
			default:
				yield break;
			}
		}
		
		yield break;
	}
	
	void ResetAutoActions() {
		if(attackSensor != null) {
			attackSensor.gameObject.SetActive(attackStartEnable);
			attackSensor.enabled = true;
		}
		
		if(spellSensor != null) {
			spellSensor.gameObject.SetActive(spellStartEnable);
			spellSensor.enabled = true;
		}
	}
			
	void OnDrawGizmosSelected() {
		Gizmos.color = new Color(208.0f/255.0f, 149.0f/255.0f, 208.0f/255.0f);
		Gizmos.DrawWireSphere(transform.position, actionCancelRadius);
		
		Gizmos.color *= 0.4f;
		Gizmos.DrawWireSphere(transform.position, actionMaxRadius);
	}
}
