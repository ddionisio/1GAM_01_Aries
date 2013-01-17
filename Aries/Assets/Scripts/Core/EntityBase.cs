using UnityEngine;
using System.Collections;

public class EntityBase : MonoBehaviour {
	public delegate void OnSetState(EntityBase ent, EntityState state);
	public delegate void OnSetBool(EntityBase ent, bool b);
	public delegate void OnFinish(EntityBase ent);
	
	public float spawnDelay = 0.1f;
	
	public event OnSetState setStateCallback;
	public event OnSetBool setBlinkCallback;
	public event OnFinish spawnFinishCallback;
	public event OnFinish releaseCallback;
	
	private EntityState mState = EntityState.NumState;
	private EntityState mPrevState = EntityState.NumState;
	
	private float mEntCurTime = 0;
	private float mBlinkCurTime = 0;
	private float mBlinkDelay = 0;
	
	public EntityState state {
		get { return mState; }
		
		set {
			if(mState != value) {
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
			bool ret = false;
			
			PoolDataController poolDat = GetComponent<PoolDataController>();
			if(poolDat != null) {
				ret = poolDat.claimed;
			}
			
			return ret;
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
	
	/// <summary>
	/// Spawn this entity, resets stats, set action to spawning, then later calls OnEntitySpawnFinish.
	/// NOTE: calls after an update to ensure Awake and Start is called.
	/// </summary>
	public void Spawn() {
		mState = mPrevState = EntityState.NumState; //avoid invalid updates
		//ensure start is called before spawning if we are freshly allocated from entity manager
		StartCoroutine(DoSpawn());
	}
	
	public virtual void Release() {
		if(releaseCallback != null) {
			releaseCallback(this);
		}
		
		StopAllCoroutines();
		EntityManager.instance.Release(this);
	}
	
	protected virtual void OnDestroy() {
		setStateCallback = null;
		setBlinkCallback = null;
		spawnFinishCallback = null;
		releaseCallback = null;
	}
	
	protected virtual void Awake() {
	}

	// Use this for initialization
	protected virtual void Start () {
		BroadcastMessage("EntityStart", this, SendMessageOptions.DontRequireReceiver);
	}
	
	protected virtual void StateChanged() {
	}
	
	protected virtual void SpawnFinish() {
	}
	
	protected virtual void SetBlink(bool blink) {
	}
	
	// Update is called once per frame
	private void Update () {
		switch(mState) {
		case EntityState.spawning:
			mEntCurTime += Time.deltaTime;
			if(mEntCurTime >= spawnDelay) {
				mPrevState = mState;
				mState = EntityState.NumState; //need to be set by something
												
				SpawnFinish();
				
				if(spawnFinishCallback != null) {
					spawnFinishCallback(this);
				}
			}
			break;
		}
		
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
	
	//////////internal
		
	IEnumerator DoSpawn() {
				
		yield return new WaitForFixedUpdate();
		
		mEntCurTime = 0;
		
		state = EntityState.spawning;
		
		yield break;
	}
}
