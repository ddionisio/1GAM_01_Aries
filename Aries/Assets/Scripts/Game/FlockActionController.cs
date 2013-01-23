using UnityEngine;
using System.Collections;

//collider is used for following, assume it's trigger
public class FlockActionController : ActionListener {
	public FlockUnit flockUnit;
	
	public float attackMinRange = 0.0f;
	public float attackRange = 0.0f; //range to attack
	public float attackAngle = 0.0f;
	public float attackCancelRadius = 12.0f;
	public float attackCancelDelay = 1.0f; //range to cancel attack if too far from leader
	
	public float followStopDelay = 1.0f;
	public float followStopRadius = 2.0f;
	public float followStopSpeed = 0.01f;
	
	[System.NonSerializedAttribute] public Transform leader = null;
	
	private MotionBase mTargetMotion = null;
	private float mAttackCosTheta;
	
	// Use this to determine if we can attack in range
	public override bool CheckRange() {
		if(currentTarget != null) {
			switch(type) {
			case ActionType.Attack:
				return flockUnit.enabled 
					&& flockUnit.moveTargetDistance >= attackMinRange
					&& flockUnit.moveTargetDistance <= attackRange 
					&& Vector2.Dot(flockUnit.moveTargetDir, flockUnit.dir) > mAttackCosTheta;
				
			default:
				break;
			}
		}
		
		return true;	
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
	
	protected virtual void Awake() {
		mAttackCosTheta = Mathf.Cos(attackAngle*Mathf.Deg2Rad);
	}
	
	IEnumerator ReturnToLeader() {
		//see if we are too far from leader
		float radiusSqr = attackCancelRadius*attackCancelRadius;
		
		//attackCancelDelay
		while(currentTarget != null && currentTarget != defaultTarget && leader != null) {
			yield return new WaitForSeconds(attackCancelDelay);
			
			Vector2 pos = transform.position;
			Vector2 leaderPos = leader.position;
			
			if((leaderPos - pos).sqrMagnitude > radiusSqr) {
				StopAction(ActionTarget.Priority.High, true);
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
	
	void OnDrawGizmosSelected() {
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(transform.position, attackMinRange);
		
		Gizmos.color *= 0.75f;
		Gizmos.DrawWireSphere(transform.position, attackRange);
	}
}
