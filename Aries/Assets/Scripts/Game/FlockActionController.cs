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
			
			StartCoroutine("ReturnToLeader");
			break;
			
		case ActionType.Follow:
			flockUnit.moveTarget = currentTarget.target;
			
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
		flockUnit.moveTarget = null;
		flockUnit.minMoveTargetDistance = 0.0f;
		
		mTargetMotion = null;
		
		StopCoroutine("FollowStop");
		StopCoroutine("ReturnToLeader");
	}
	
	protected override void OnDestroy ()
	{
		if(attackSensor != null) {
			attackSensor.enterCallback -= AutoAttackEnter;
			attackSensor.exitCallback -= AutoAttackExit;
		}
		
		base.OnDestroy ();
	}
	
	protected virtual void Awake() {
		mAttackCosTheta = Mathf.Cos(attackAngle*Mathf.Deg2Rad);
		
		ResetAutoAttack();
		
		if(attackSensor != null) {
			attackSensor.enterCallback += AutoAttackEnter;
			attackSensor.exitCallback += AutoAttackExit;
		}
	}
	
	protected virtual void AutoAttackEnter(UnitEntity unit) {
		ActionTarget target = unit.actionTarget;
		if(currentActType != ActionType.Attack && target != null 
			&& target.type == ActionType.Attack && target.vacancy && currentPriority <= target.priority) {
			currentTarget = target;
		}
	}
	
	protected virtual void AutoAttackExit(UnitEntity unit) {
	}
	
	IEnumerator ReturnToLeader() {
		//see if we are too far from leader
		float radiusCancelSqr = attackCancelRadius*attackCancelRadius;
		float radiusMoveBackSqr = attackLeaderRadius*attackLeaderRadius;
		
		//attackCancelDelay
		while(currentTarget != null && currentTarget != defaultTarget && leader != null) {
			yield return new WaitForSeconds(attackCancelDelay);
			
			Vector2 pos = transform.position;
			Vector2 leaderPos = leader.position;
			float distSqr = (leaderPos - pos).sqrMagnitude;
			
			if(distSqr > radiusCancelSqr) {
				StopAction(ActionTarget.Priority.High, true);
			}
			else if(distSqr > radiusMoveBackSqr) {
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
		if(attackSensor != null)
			attackSensor.gameObject.SetActive(attackStartEnable);
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
