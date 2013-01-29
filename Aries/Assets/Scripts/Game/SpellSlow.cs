using UnityEngine;
using System.Collections;

public class SlowData : SpellInstance {
	public float prevMass;
	
	public SlowData(UnitEntity unit, SpellSlow spell) : base(unit, spell) {
		Rigidbody body = unit.rigidbody;
		if(body != null) {
			prevMass = body.mass;
			body.mass = prevMass*spell.scaleMass;
		}
	}
	
	protected override void Remove(UnitEntity unit) {
		Rigidbody body = unit.rigidbody;
		if(body != null) {
			body.mass = prevMass;
		}
	}
	
	protected override void Tick(UnitEntity unit) {
	}
}

public class SpellSlow : SpellBase {
	public float scaleMass = 2.0f;

	public override SpellInstance Start(UnitEntity unit) {
		return new SlowData(unit, this);
	}
}
