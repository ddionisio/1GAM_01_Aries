using UnityEngine;
using System.Collections;

/// <summary>
/// Generic weapon that shoots towards dir with a bit of spread, and love.
/// </summary>
public class WeaponShoot : Weapon {
	public float startDistance = 0.75f;
	public float angleSpread = 5.0f;
	
	public override void Shoot(Vector2 pos, Vector2 dir, Transform seek) {
		float rad = angleSpread*Mathf.Deg2Rad;
		dir = M8.Math.Rotate(dir, Random.Range(-rad, rad));
		pos += dir*startDistance;
		
		Projectile.Create(projectile, pos, dir);
	}
	
	void OnDrawGizmosSelected() {
		
		Gizmos.color = new Color(1.0f, 201.0f/255.0f, 14.0f/255.0f);
		
		Gizmos.DrawSphere(transform.position + Vector3.right*startDistance, 0.1f);
	}
}
