using UnityEngine;
using System.Collections;
using HutongGames.PlayMaker;

public class EntityBase : MonoBehaviour {
    public delegate void OnSetState(EntityBase ent, EntityState state);
    public delegate void OnSetBool(EntityBase ent, bool b);
    public delegate void OnFinish(EntityBase ent);

    public float spawnDelay = 0.1f;

    public bool activateOnStart = false; //if we want FSM/other stuff to activate on start (when placing entities on scene)

    public event OnSetState setStateCallback;
    public event OnSetBool setBlinkCallback;
    public event OnFinish spawnCallback;
    public event OnFinish releaseCallback;

    private EntityState mState = EntityState.NumState;
    private EntityState mPrevState = EntityState.NumState;

    private PlayMakerFSM mFSM;
    private EntityActivator mActivator = null;

    private float mBlinkCurTime = 0;
    private float mBlinkDelay = 0;

    private bool mDoSpawnOnWake = false;

    private string mSpawnGroup = null; //set this after being spawned via pool manager

    public static T Spawn<T>(string spawnGroup, string typeName, Vector3 position) where T : EntityBase {
        SpawnPool pool = PoolManager.Pools[spawnGroup];
        Transform spawned = pool.Spawn(pool.prefabs[typeName]);
        T ent = spawned != null ? spawned.GetComponent<T>() : null;

        if(ent != null) {
            ent.mSpawnGroup = spawnGroup;
            ent.transform.position = position;
        }

        return ent;
    }

    /// <summary>
    /// Set by call from Spawn, so use that instead of directly spawning via pool manager.
    /// </summary>
    public string spawnGroup {
        get { return mSpawnGroup; }
    }

    ///<summary>entity is instantiated with activator set to disable on start, call spawn after activator sends a wakeup call.</summary>
    public bool doSpawnOnWake {
        get { return mDoSpawnOnWake; }
    }

    public PlayMakerFSM FSM {
        get { return mFSM; }
    }

    public EntityState state {
        get { return mState; }

        set {
            if(mState != value) {
                if(mState != EntityState.NumState)
                    mPrevState = mState;

                mState = value;

                if(setStateCallback != null) {
                    setStateCallback(this, value);
                }

                StateChanged();
            }
        }
    }

    public EntityState prevState {
        get { return mPrevState; }
    }

    public bool isReleased {
        get {
            if(string.IsNullOrEmpty(mSpawnGroup))
                return false;

            return !PoolManager.Pools[mSpawnGroup].IsSpawned(transform);
        }
    }

    public bool isBlinking {
        get { return mBlinkDelay > 0 && mBlinkCurTime < mBlinkDelay; }
    }

    public void Blink(float delay) {
        mBlinkDelay = delay;
        mBlinkCurTime = 0;

        bool doBlink = delay > 0;

        if(setBlinkCallback != null) {
            setBlinkCallback(this, doBlink);
        }

        SetBlink(doBlink);
    }

    public void Release() {
        //EntityManager.instance.Release(this);
        if(!string.IsNullOrEmpty(mSpawnGroup)) {
            transform.parent = PoolManager.Pools[mSpawnGroup].group;
            PoolManager.Pools[mSpawnGroup].Despawn(transform);
        }
        else {
            StartCoroutine(DestroyDelay());
        }
    }

    /// <summary>
    /// This is to tell the entity that spawning has finished. Use this to start any motion, etc.
    /// </summary>
    public virtual void SpawnFinish() {
    }

    protected virtual void OnDespawned() {
        if(mFSM != null) {
            mFSM.enabled = false;
        }

        if(mActivator != null) {
            mActivator.Release(false);
        }

        if(releaseCallback != null) {
            releaseCallback(this);
        }

        mDoSpawnOnWake = false;

        StopAllCoroutines();
    }

    protected virtual void ActivatorWakeUp() {
        if(mDoSpawnOnWake) { //if we haven't properly spawned yet, do so now
            mDoSpawnOnWake = false;
            StartCoroutine(DoSpawn());
        }
        else if(mFSM != null) {
            //resume FSM
            mFSM.Fsm.Event(EntityEvent.Wake);
        }
    }

    protected virtual void ActivatorSleep() {
        if(mFSM != null) {
            mFSM.Fsm.Event(EntityEvent.Sleep);
        }
    }

    protected virtual void OnDestroy() {
        if(mActivator != null) {
            mActivator.Release(true);
        }

        setStateCallback = null;
        setBlinkCallback = null;
        spawnCallback = null;
        releaseCallback = null;
    }

    protected virtual void Awake() {
        mActivator = GetComponentInChildren<EntityActivator>();
        if(mActivator != null) {
            mActivator.awakeCallback += ActivatorWakeUp;
            mActivator.sleepCallback += ActivatorSleep;
        }

        //only start once we spawn
        mFSM = GetComponentInChildren<PlayMakerFSM>();
        if(mFSM != null) {
            mFSM.Fsm.RestartOnEnable = false; //not when we want to sleep/wake
            mFSM.enabled = false;
        }
    }

    // Use this for initialization
    protected virtual void Start() {
        BroadcastMessage("EntityStart", this, SendMessageOptions.DontRequireReceiver);

        //for when putting entities on scene, skip the spawning state
        if(activateOnStart) {
            if(mFSM != null)
                mFSM.enabled = true;

            StartCoroutine(DoStart());
        }
    }

    protected virtual void StateChanged() {
    }

    protected virtual void SetBlink(bool blink) {
    }

    protected virtual void SpawnStart() {
    }

    //////////internal

    // Update is called once per frame
    private void Update() {
        if(mBlinkDelay > 0) {
            mBlinkCurTime += Time.deltaTime;
            if(mBlinkCurTime >= mBlinkDelay) {
                mBlinkDelay = mBlinkCurTime = 0;

                if(setBlinkCallback != null) {
                    setBlinkCallback(this, false);
                }

                SetBlink(false);
            }
        }
    }

    /// <summary>
    /// Spawn this entity, resets stats, set action to spawning, then later calls OnEntitySpawnFinish.
    /// NOTE: calls after an update to ensure Awake and Start is called.
    /// </summary>
    void OnSpawned() {
        mState = mPrevState = EntityState.NumState; //avoid invalid updates

        //allow activator to start and check if we need to spawn now or later
        //ensure start is called before spawning if we are freshly allocated from entity manager
        if(mActivator != null) {
            mActivator.Start();

            if(mActivator.deactivateOnStart) {
                mDoSpawnOnWake = true; //do it later when we wake up
            }
            else {
                StartCoroutine(DoSpawn());
            }
        }
        else {
            StartCoroutine(DoSpawn());
        }
    }
                
    IEnumerator DoStart() {
        yield return new WaitForFixedUpdate();

        SpawnStart();

        if(mFSM != null) {
            mFSM.SendEvent(EntityEvent.Start);
        }

        yield break;
    }

    IEnumerator DoSpawn() {

        yield return new WaitForFixedUpdate();

        SpawnStart();

        if(spawnCallback != null) {
            spawnCallback(this);
        }

        //start up
        if(mFSM != null) {
            //restart
            mFSM.Fsm.Reinitialize();
            mFSM.enabled = true;

            //allow fsm to boot up, then tell it to spawn
            yield return new WaitForFixedUpdate();

            mFSM.SendEvent(EntityEvent.Spawn);
        }
        else {
            yield return new WaitForSeconds(spawnDelay);

            SpawnFinish();
        }

        yield break;
    }

    IEnumerator DestroyDelay() {
        yield return new WaitForFixedUpdate();

        GameObject.Destroy(gameObject);

        yield break;
    }
}
