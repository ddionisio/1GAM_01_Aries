using UnityEngine;
using System.Collections;

public abstract class Weapon : MonoBehaviour {
	public class RepeatParam {
		public Transform source = null;
		public Vector2 dir = Vector2.right;
		public Transform seek = null;
	}
	
	public string projectile;
	public float delayPerShot = 0.5f;
	public float recoilForce = 0.0f;
	
	private bool mIsFiring = false;
	private bool mIsRepeatActive = false;
	
	/// <summary>
	/// Shoot from specified position with given dir and seek (if required by projectile)
	/// </summary>
	public abstract void Shoot(Vector2 pos, Vector2 dir, Transform seek);
	
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
		while(mIsFiring && param.source != null) {
			Shoot(param.source.position, param.dir, param.seek);
			
			yield return new WaitForSeconds(delayPerShot);
		}
		
		mIsRepeatActive = false;
		
		yield break;
	}
}
