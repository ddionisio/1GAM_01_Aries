using UnityEngine;
using System.Collections;

//collider is used for following, assume it's trigger
public class FlockActionController : ActionListener {
	public FlockUnit flockUnit;
	
	public float attackMinRange = 0.0f;
	public float attackRange = 0.0f; //range to attack
	public float attackAngle = 0.0f;
	public float attackLeaderRadius = 6.75f;
	public float attackCancelRadius = 12.0f; //range to move back to leader
	public float attackCancelDelay = 1.0f; //range to cancel attack if too far from leader
	
	public float followStopDelay = 1.0f;
	public float followStopRadius = 2.0f;
	public float followStopSpeed = 0.01f;
	
	public bool attackStartEnable = false;
	[SerializeField] AttackSensor attackSensor;
	
	[System.NonSerializedAttribute] public Transform leader = null;
	
	private MotionBase mTargetMotion = null;
	private float mAttackCosTheta;
	
	public bool autoAttack {
		get { return attackSensor != null ? attackSensor.gameObject.activeSelf : false; }
		set { 
			if(attackSensor != null) {
				bool isActive = attackSensor.gameObject.activeSelf;
				if(isActive != value) {
					if(!value) {
						//clear out attack target
						if(currentTarget != null && currentTarget.type == ActionType.Attack) {
							StopAction(ActionTarget.Priority.Highest, true);
						}
					}
					
					attackSensor.gameObject.SetActive(value);
				}
			}
		}
	}
			
	// Use this to determine if we can attack in range
	public override bool CheckRange() {
		if(currentTarget != null) {
			switch(type) {
			case ActionType.Attack:
				Vector2 pos = transform.position;
				Vector2 tpos = currentTarget.transform.position;
				Vector2 dir = tpos-pos;
				float dist = dir.magnitude; dir /= dist;
				
				return flockUnit.enabled 
					&& dist >= attackMinRange
					&& dist <= attackRange
					&& Vector2.Dot(dir, flockUnit.dir) > mAttackCosTheta;
				
			default:
				break;
			}
		}
		
		return true;	
	}
	
	public override void SetActive(bool activate) {
		base.SetActive(activate);
		
		if(!activate) {
			ResetAutoAttack();
		}
	}
	
	protected override void OnActionEnter() {
		mTargetMotion = currentTarget.GetComponent<MotionBase>();
		
		switch(type) {
		case ActionType.Disperse:
			break;
			
		case ActionType.Attack:
			flockUnit.moveTarget = currentTarget.transform;
			flockUnit.minMoveTargetDistance = attackMinRange;
			
			if(gameObject.activeInHierarchy)
				StartCoroutine("ReturnToLeader");
			
			//no need to constantly check
			if(attackSensor != null) {
				attackSensor.enabled = false;
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
		
		StopCoroutine("FollowStop");
		StopCoroutine("ReturnToLeader");
	}
	
	protected override void OnDestroy ()
	{
		if(attackSensor != null) {
			attackSensor.stayCallback -= AutoAttackStay;
		}
		
		base.OnDestroy ();
	}
	
	protected virtual void Awake() {
		mAttackCosTheta = Mathf.Cos(attackAngle*Mathf.Deg2Rad);
		
		ResetAutoAttack();
		
		if(attackSensor != null) {
			attackSensor.stayCallback += AutoAttackStay;
		}
	}
	
	protected virtual void AutoAttackStay(UnitEntity unit) {
		if(!(type == ActionType.Attack || type == ActionType.Retreat)) {
			ActionTarget target = unit.actionTarget;
			if(target != null && target.type == ActionType.Attack && target.vacancy && currentPriority <= target.priority) {
				currentTarget = target;
			}
		}
	}
	
	IEnumerator ReturnToLeader() {
		//see if we are too far from leader
		float radiusCancelSqr = attackCancelRadius*attackCancelRadius;
		float radiusMoveBackSqr = attackLeaderRadius*attackLeaderRadius;
		
		//attackCancelDelay
		while(!lockAction && currentTarget != null && currentTarget != defaultTarget && leader != null) {
			yield return new WaitForSeconds(attackCancelDelay);
			
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
	
	void ResetAutoAttack() {
		if(attackSensor != null) {
			attackSensor.gameObject.SetActive(attackStartEnable);
			attackSensor.enabled = true;
		}
	}
			
	void OnDrawGizmosSelected() {
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(transform.position, attackMinRange);
		
		Gizmos.color *= 0.75f;
		Gizmos.DrawWireSphere(transform.position, attackRange);
		
		Gizmos.color = new Color(208.0f/255.0f, 149.0f/255.0f, 208.0f/255.0f);
		Gizmos.DrawWireSphere(transform.position, attackCancelRadius);
		
		Gizmos.color *= 0.4f;
		Gizmos.DrawWireSphere(transform.position, attackLeaderRadius);
		
		//attackLeaderRadius
	}
}
