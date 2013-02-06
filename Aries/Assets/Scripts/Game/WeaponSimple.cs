using UnityEngine;
using System.Collections;

/// <summary>
/// Just spawns a projectile.
/// </summary>
public class WeaponSimple : Weapon {
	public override void Shoot(Vector2 pos, Vector2 dir, float damageMod, Transform seek) {
        Projectile.Create(projectile, pos, dir, damageMod, seek);
	}
}
