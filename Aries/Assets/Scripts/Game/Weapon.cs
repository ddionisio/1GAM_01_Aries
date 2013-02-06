using UnityEngine;
using System.Collections;

public abstract class Weapon : MonoBehaviour {
	public class RepeatParam {
		public Transform source = null;
        public StatBase sourceStat = null;
		public Vector2 ofs = Vector2.zero;
		public Vector2 dir = Vector2.right;
		public Transform seek = null;
	}
	
	public string projectile;
	public float delayPerShot = 0.5f;
	public float recoilForce = 0.0f;
	public bool useUpVector = false;

	private bool mIsFiring = false;
	private bool mIsRepeatActive = false;
    	
	/// <summary>
	/// Shoot from specified position with given dir and seek (if required by projectile)
	/// </summary>
	public abstract void Shoot(Vector2 pos, Vector2 dir, float damageMod, Transform seek);
	
	/// <summary>
	/// Shoot from weapon's position and given offset (ofs).
	/// </summary>
    public void ShootOfs(Vector2 ofs, Vector2 dir, float damageMod, Transform seek) {
		Vector2 p = transform.position;
        Shoot(p + ofs, dir, damageMod, seek);
	}

    public void ShootUpDir(Vector2 ofs, float damageMod, Transform seek) {
		Vector2 p = transform.position;
		Shoot(p + ofs, transform.up, damageMod, seek);
	}
	
	public void Repeat(RepeatParam param) {
		mIsFiring = true;
		
		if(!mIsRepeatActive) {
			mIsRepeatActive = true;
			StartCoroutine(DoShoot(param));
		}
	}
	
	public void RepeatStop() {
		mIsFiring = false;
	}
	
	/// <summary>
	/// Call this when you are releasing the entity, stops the co-routine
	/// </summary>
	public void Release() {
		mIsFiring = false;
		
		StopAllCoroutines();
	}

	private IEnumerator DoShoot(RepeatParam param) {
		while(mIsFiring) {
			Vector2 p = param.source != null ? param.source.position : transform.position;
            float damageMod = param.sourceStat != null ? param.sourceStat.damageMod : 0.0f;
			p += param.ofs;
			
			if(useUpVector) {
				Shoot(p, transform.up, damageMod, param.seek);
			}
			else {
                Shoot(p, param.dir, damageMod, param.seek);
			}
									
			yield return new WaitForSeconds(delayPerShot);
		}
		
		mIsRepeatActive = false;
		
		yield break;
	}
}
