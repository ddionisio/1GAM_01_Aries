using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class SpellSensor : SensorSingle<UnitEntity> {
	
	public SpellCaster caster;
	
	public float range = 0.0f;
	
	public bool CheckRange(Transform target) {
		Vector2 pos = transform.position;
		Vector2 tpos = target.position;
		
		return (tpos-pos).sqrMagnitude <= range*range;
	}
	
	protected override bool UnitVerify(UnitEntity unit) {
		return caster.CanCastTo(unit);
	}
	
	void OnDrawGizmosSelected() {
		Gizmos.color = Color.blue;
		Gizmos.DrawWireSphere(transform.position, range);
	}
}
