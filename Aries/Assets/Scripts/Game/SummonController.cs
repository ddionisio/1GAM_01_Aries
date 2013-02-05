using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SummonController : MonoBehaviour {
	public delegate void Callback(SummonController summonController, UnitEntity ent);
	
	public int queueCapacity = 5;
	public float radius = 5.0f;
	public float checkRadius = 0.8f; //radius to check collision
	public LayerMask checkLayer;
	
	public event Callback summonedCallback;
	
	private Queue<UnitType> mSummonQueue;
	
	public int queueCount { get { return mSummonQueue.Count; } }
	
	public void Summon(UnitType type, int amount) {
		for(int i = 0; i < amount; i++) {
			mSummonQueue.Enqueue(type);
		}
	}
	
	public void ClearSummonQueue() {
		mSummonQueue.Clear();
	}
	
	void Awake() {
		mSummonQueue = new Queue<UnitType>(queueCapacity);
	}
	
	void EntityStart(EntityBase ent) {
		ent.releaseCallback += EntityRelease;
		ent.setStateCallback += EntitySetState;
	}
	
	void EntityRelease(EntityBase ent) {
		ClearSummonQueue();
	}
	
	void EntitySetState(EntityBase ent, EntityState state) {
		switch(state) {
		case EntityState.dying:
			ClearSummonQueue();
			break;
		}
	}
	
	void OnDestroy() {
		summonedCallback = null;
	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		//keep summoning from queue
		if(mSummonQueue.Count > 0) {
			//attempt to summon on current area, and if success, 
			//dequeue and update summon slot
			UnitType unitType = mSummonQueue.Peek();
			UnitEntity unit = GrabSummonUnit(unitType);
			if(unit != null) {
				mSummonQueue.Dequeue();
				
				if(summonedCallback != null) {
					summonedCallback(this, unit);
				}
			}
		}
	}
	
	void OnDrawGizmosSelected() {
		Gizmos.color = new Color(218.0f/255.0f, 164.0f/255.0f, 2.0f/255.0f);
		Gizmos.DrawWireSphere(transform.position, radius);
		
		Gizmos.color *= 0.75f;
		Gizmos.DrawWireSphere(transform.position, checkRadius);
	}
	
	private UnitEntity GrabSummonUnit(UnitType unitType) {
		UnitEntity ent = null;
		//check if it's safe to summon on the spot
		Vector2 pos = transform.position;
		pos += Random.insideUnitCircle*radius;
		if(!Physics.CheckSphere(pos, checkRadius, checkLayer.value)) {
			string typeName = unitType.ToString();
			EntityManager entMgr = EntityManager.instance;
			ent = entMgr.Spawn<UnitEntity>(typeName, typeName, null, null);
			if(ent != null) {
				ent.transform.position = pos;
			}
		}
		
		return ent;
	}
}
