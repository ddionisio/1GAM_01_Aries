using UnityEngine;
using System.Collections;

public class Projectile : EntityBase {
	public enum ContactType {
		None,
		End,
		Stop,
		Bounce
	}
	
	public UnitDamageType damageType;
	
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
	
	/*public bool oscillate;
	public float oscillateForce;
	public float oscillateDelay;*/
	
	private Vector2 mStartDir = Vector2.zero;
	private Transform mSeek = null;
	
	//private Vector2 mOscillateDir;
	//private bool mOscillateSwitch;
	
	public static Projectile Create(string typeName, Vector2 startPos, Vector2 dir, Transform seek, Transform toParent = null) {
		Projectile ret = EntityManager.instance.Spawn<Projectile>(typeName, typeName, toParent, null);
		
		if(ret != null) {
			ret.mStartDir = dir;
			ret.transform.position = startPos;
			
			ret.seek = seek;
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
	
	public override void SpawnFinish ()
	{
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
			transform.up = mStartDir;
			InvokeRepeating("OnUpUpdate", 0.1f, 0.1f);
		}
	}
	
	protected override void SpawnStart ()
	{
		state = EntityState.spawning;
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
	
	void OnCollisionEnter(Collision collision) {
		switch(contactType) {
		case ContactType.End:
			state = EntityState.dying;
			break;
			
		case ContactType.Stop:
			mover.body.velocity = Vector3.zero;
			break;
			
		case ContactType.Bounce:
			if(collision.contacts.Length > 0) {
				Vector2 normal = collision.contacts[0].normal;
				Vector2 reflect = M8.Math.Reflect(mover.dir, normal);
				Vector2 vel = mover.body.velocity;
				float velMag = vel.magnitude;
				mover.body.velocity = reflect*velMag;
			}
			break;
		}
		
		//do damage
		if(minDamage > 0.0f && !explodeOnDeath) {
			StatBase stat = collision.gameObject.GetComponentInChildren<StatBase>();
			if(stat != null) {
				stat.curHP -= minDamage;
			}
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
