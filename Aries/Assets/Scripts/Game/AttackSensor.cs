using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class AttackSensor : SensorSingle<UnitEntity> {
	
	public StatBase stat;
	
	public float minRange = 0.0f;
	public float maxRange = 0.0f; //range to attack
	public float angle = 0.0f;
	
	private float mCosTheta;
	
	public bool CheckRange(Vector2 curDir, Transform target) {
		Vector2 pos = transform.position;
		Vector2 tpos = target.position;
		Vector2 dir = tpos-pos;
		float dist = dir.magnitude; dir /= dist;
		
		return dist >= minRange
			&& dist <= maxRange
			&& Vector2.Dot(dir, curDir) > mCosTheta;
	}
	
	protected override bool UnitVerify(UnitEntity unit) {
		return stat.CanDamage(unit.stats);
	}
	
	void Awake() {
		mCosTheta = Mathf.Cos(angle*Mathf.Deg2Rad);
	}
	
	void OnDrawGizmosSelected() {
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(transform.position, minRange);
		
		Gizmos.color *= 0.75f;
		Gizmos.DrawWireSphere(transform.position, maxRange);
	}
}
