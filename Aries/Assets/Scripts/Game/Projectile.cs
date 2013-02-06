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
    public float startForceAddRand;
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

    public GameObject activateOnDying;

    /*public bool oscillate;
    public float oscillateForce;
    public float oscillateDelay;*/

    private Vector2 mStartDir = Vector2.zero;
    private Transform mSeek = null;
    private float mDamageMod = 0.0f;

    //private Vector2 mOscillateDir;
    //private bool mOscillateSwitch;

    public static Projectile Create(string typeName, Vector2 startPos, Vector2 dir, float damageMod, Transform seek, Transform toParent = null) {
        Projectile ret = EntityManager.instance.Spawn<Projectile>(typeName, typeName, toParent, null);

        if(ret != null) {
            ret.mStartDir = dir;
            ret.transform.position = new Vector3(startPos.x, startPos.y, ret.transform.position.z);

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

        if(mover != null)
            mover.body.velocity = Vector3.zero;

        if(collider != null)
            collider.enabled = false;

        if(activateOnDying != null)
            activateOnDying.SetActive(false);

        base.Release();
    }

    protected override void Awake() {
        base.Awake();

        if(collider != null)
            collider.enabled = false;
    }

    // Use this for initialization
    protected override void Start() {
        base.Start();

    }

    public override void SpawnFinish() {
        if(decayDelay == 0) {
            OnDecayEnd();
        }
        else {
            Invoke("OnDecayEnd", decayDelay);

            if(collider != null)
                collider.enabled = true;

            if(seekDelay > 0.0f) {
                Invoke("OnSeekStart", seekDelay);
            }
            else {
                state = EntityState.normal;
            }

            //starting direction and force
            if(rigidbody != null && mStartDir != Vector2.zero) {
                //set force
                if(startForceAddRand != 0.0f) {
                    rigidbody.AddForce(mStartDir * (startForce + Random.value * startForceAddRand));
                }
                else {
                    rigidbody.AddForce(mStartDir * startForce);
                }
            }

            if(applyDirToUp) {
                transform.up = mStartDir;
                InvokeRepeating("OnUpUpdate", 0.1f, 0.1f);
            }
        }
    }

    protected override void SpawnStart() {
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

                if(mover != null)
                    mover.body.velocity = Vector3.zero;

                if(explodeOnDeath && explodeRadius > 0.0f) {
                    DoExplode();
                }

                if(activateOnDying != null)
                    activateOnDying.SetActive(true);

                Invoke("Release", dieDelay);
                break;
        }
    }

    void ApplyContact(GameObject go, Vector2 normal) {
        switch(contactType) {
            case ContactType.End:
                state = EntityState.dying;
                break;

            case ContactType.Stop:
                if(mover != null)
                    mover.body.velocity = Vector3.zero;
                break;

            case ContactType.Bounce:
                if(mover != null) {
                    Vector2 reflect = M8.Math.Reflect(mover.dir, normal);
                    Vector2 vel = mover.body.velocity;
                    float velMag = vel.magnitude;
                    mover.body.velocity = reflect * velMag;
                }
                break;
        }

        //do damage
        if(!explodeOnDeath) {
            StatBase stat = go.GetComponentInChildren<StatBase>();
            if(stat != null) {
                if(maxDamage > minDamage)
                    stat.DamageBy(damageType, Random.Range(minDamage, maxDamage), mDamageMod);
                else
                    stat.DamageBy(damageType, minDamage, mDamageMod);
            }
        }
    }

    void OnCollisionEnter(Collision collision) {
        ContactPoint cp = collision.contacts[0];
        ApplyContact(cp.otherCollider.gameObject, cp.normal);
    }

    /*void OnTrigger(Collider collider) {
        ApplyContact(collider.gameObject, -mover.dir);
    }*/

    void OnDecayEnd() {
        state = EntityState.dying;
    }

    void OnSeekStart() {
        state = EntityState.seek;
    }

    void OnUpUpdate() {
        if(mover != null && mover.dir != Vector2.zero) {
            transform.up = mover.dir;
        }
    }

    void FixedUpdate() {
        switch(state) {
            case EntityState.seek:
                if(mover != null && mSeek != null) {
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

                        Vector2 force = M8.Math.Steer(mover.body.velocity, _dir * mover.maxSpeed, seekForce, 1.0f);
                        mover.body.AddForce(force.x, force.y, 0.0f);
                    }
                }
                break;
        }
    }

    void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explodeRadius);
    }

    private void DoExplode() {
        Vector3 pos = transform.position;
        float explodeRadiusSqr = explodeRadius * explodeRadius;
        float damageRange = maxDamage - minDamage;

        //TODO: spawn fx

        Collider[] cols = Physics.OverlapSphere(pos, explodeRadius, explodeMask.value);

        foreach(Collider col in cols) {
            if(col != null && col.rigidbody != null) {
                //hurt?
                StatBase stats = col.GetComponent<StatBase>();
                if(stats != null) {
                    col.rigidbody.AddExplosionForce(explodeForce, pos, explodeRadius, 0.0f, ForceMode.Force);

                    float distSqr = (col.transform.position - pos).sqrMagnitude;

                    stats.DamageBy(damageType, minDamage + damageRange * (1.0f - distSqr / explodeRadiusSqr), mDamageMod);
                }
            }
        }
    }
}
