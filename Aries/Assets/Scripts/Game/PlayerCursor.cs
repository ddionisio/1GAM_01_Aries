using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerCursor : MonoBehaviour {
	public FlockType type = FlockType.PlayerOneUnits;
	
	public tk2dBaseSprite cursorSprite;
	public GameObject attackSprite;
	public GameObject contextSprite;
	
	public Color neutralColor;
	public Color attackColor;
	public Color contextColor;
	
	public float radius;
	
	public float distance = 5.0f;
	
	public LayerMask checkMask;
	
	public ActionSensor attackSensor; //for all hostile enemies
	public ActionSensor contextSensor; //anything non-combat related (or sub target for bosses)
			
	private static Dictionary<FlockType, PlayerCursor> mCursors = new Dictionary<FlockType, PlayerCursor>();
	
	private Transform mOrigin;
	
	private Vector2 mDir = Vector2.up;
	
	public static PlayerCursor GetByType(FlockType aType) {
		PlayerCursor ret = null;
		mCursors.TryGetValue(aType, out ret);
		return ret;
	}
	
	public Transform origin {
		get { return mOrigin != null ? mOrigin : transform; }
		set {
			mOrigin = value;
		}
	}
	
	public Vector2 dir {
		get { return mDir; }
		set { mDir = value; }
	}
	
	public bool CheckArea(int layerMask) {
		return Physics.CheckSphere(transform.position, radius, layerMask);
	}
	
	void OnDestroy() {
		mCursors.Remove(type);
	}
	
	void Awake() {
		mCursors.Add(type, this);
		
		attackSprite.SetActive(false);
		contextSprite.SetActive(false);
	}
	
	// Use this for initialization
	void Start () {
	}
	
	void Update() {
		Vector3 start = origin.position;
		
		//cast to reposition
		if(mDir != Vector2.zero) {
			RaycastHit hit;
			if(Physics.SphereCast(start, radius, mDir, out hit, distance, checkMask.value)) {
				Vector3 delta = mDir*hit.distance;
				transform.position = start + delta;
			}
			else {
				Vector3 delta = mDir*distance;
				transform.position = start + delta;
			}
		}
		
		//determine colors based on contents of attack and context
		int numAttack = attackSensor.units.Count;
		int numContext = contextSensor.units.Count;
		
		attackSprite.SetActive(numAttack > 0);
		contextSprite.SetActive(numContext > 0);
		
		if(numAttack > 0)
			cursorSprite.color = attackColor;
		else if(numContext > 0)
			cursorSprite.color = contextColor;
		else
			cursorSprite.color = neutralColor;
	}
		
	void OnDrawGizmosSelected() {
		Vector3 s = origin.position, e = transform.position;
		
		Gizmos.color = Color.blue;
		Gizmos.DrawLine(s, e);
		
		Gizmos.color = Color.cyan;
		Gizmos.DrawWireSphere(e, radius);
	}
}
