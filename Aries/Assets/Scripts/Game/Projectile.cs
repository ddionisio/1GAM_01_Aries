using UnityEngine;
using System.Collections;

public class Projectile : EntityBase {
	public enum ContactType {
		None,
		End,
		Explode,
		Stop,
		Bounce
	}
	
	public MotionBase mover;
	
	public float startForce;
	public float seekForce;
	
	public float seekDelay = 0.0f;
	public float seekAngleCap = 360.0f;
	
	public float decayDelay;
	public float dieDelay;
	
	public LayerMask explodeMask;
	public float explodeForce;
	public float explodeRadius;
	
	public float minDamage; //for explosions, damage is determined by distance, outer = minDamage, inner = maxDamage
	public float maxDamage;
			
	public ContactType contactType = ContactType.End;
	
	public bool explodeOnDeath;
	
	public bool applyDirToUp;
	
	private Vector2 mStartDir = Vector2.zero;
	private Transform mSeek = null;
	
	public static Projectile Create(string typeName, Vector2 startPos, Vector2 dir, Transform toParent = null) {
		Projectile ret = EntityManager.instance.Spawn<Projectile>(typeName, typeName, toParent, null);
		
		if(ret != null) {
			ret.mStartDir = dir;
			ret.transform.position = startPos;
		}
		
		return ret;
	}
	
	public Transform seek {
		get { return mSeek; }
		set {
			mSeek = value;
		}
	}
	
	public override void Release() {
		CancelInvoke();
		
		mover.body.velocity = Vector3.zero;
		
		collider.enabled = false;
		
		base.Release();
	}
	
	protected override void Awake() {
		base.Awake();
		
		collider.enabled = false;
	}

	// Use this for initialization
	protected override void Start () {
		base.Start();
		
	}
	
	protected override void StateChanged() {
		switch(state) {
		case EntityState.spawning:
			if(applyDirToUp && mStartDir != Vector2.zero) {
				transform.up = mStartDir;
			}
			break;
			
		case EntityState.dying:
			CancelInvoke();
			
			mover.body.velocity = Vector3.zero;
			
			if(explodeOnDeath) {
				DoExplode();
			}
			
			Invoke("Release", dieDelay);
			break;
		}
	}
	
	protected override void SpawnFinish() {
		Invoke("OnDecayEnd", decayDelay);
		
		collider.enabled = true;
		
		if(seekDelay > 0.0f) {
			Invoke("OnSeekStart", seekDelay);
		}
		else {
			state = EntityState.normal;
		}
		
		//starting direction and force
		if(mStartDir != Vector2.zero) {
			//set force
			rigidbody.AddForce(mStartDir*startForce);
		}
		
		if(applyDirToUp) {
			InvokeRepeating("OnUpUpdate", 0.0f, 0.1f);
		}
	}
	
	 void OnCollisionEnter(Collision collision) {
		switch(contactType) {
		case ContactType.End:
			state = EntityState.dying;
			break;
			
		case ContactType.Explode:
			DoExplode();
			break;
			
		case ContactType.Stop:
			mover.body.velocity = Vector3.zero;
			break;
		}
	}
	
	void OnDecayEnd() {
		state = EntityState.dying;
	}
	
	void OnSeekStart() {
		state = EntityState.seek;
	}
	
	void OnUpUpdate() {
		if(mover.dir != Vector2.zero) {
			transform.up = mover.dir;
		}
	}
		
	void FixedUpdate() {
		switch(state) {
		case EntityState.seek:
			if(mSeek != null) {
				//steer torwards seek
				Vector2 pos = transform.position;
				Vector2 dest = mSeek.position;
				Vector2 _dir = dest - pos;
				float dist = _dir.magnitude;
				
				if(dist > 0.0f) {
					_dir /= dist;
					
					//restrict
					if(seekAngleCap < 360.0f) {
						M8.Math.DirCap(mover.dir, ref _dir, seekAngleCap);
					}
					
					Vector2 force = M8.Math.Steer(mover.body.velocity, _dir*mover.maxSpeed, seekForce, 1.0f);
					mover.body.AddForce(force.x, force.y, 0.0f);
				}
			}
			break;
		}
	}
	
	private void DoExplode() {
		Vector3 pos = transform.position;
		
		//TODO: spawn fx
		
		Collider[] cols = Physics.OverlapSphere(pos, explodeRadius, explodeMask.value);
		
		foreach(Collider col in cols) {
			if(col != null && col.rigidbody != null) {
				col.rigidbody.AddExplosionForce(explodeForce, pos, explodeRadius, 0.0f, ForceMode.Force);
			}
		}
	}
}
