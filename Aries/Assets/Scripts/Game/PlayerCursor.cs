using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerCursor : MonoBehaviour {
	public FlockType type = FlockType.PlayerOneUnits;
	
	public tk2dBaseSprite cursorSprite;
	public GameObject contextSprite;
	
	public Color contextColor;
	public Color neutralColor;
	
	public float radius;
	
	public float distance = 5.0f;
	
	public LayerMask checkMask;
	
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
	
	public void RevertToNeutral() {
		contextSprite.SetActive(false);
		cursorSprite.color = neutralColor;
	}
	
	//call this in controller if no attack or other crap
	public void UpdateIndicator(bool updateColor) {
		//determine colors based on contents of attack and context
		if(contextSensor.units.Count > 0) {
			contextSprite.SetActive(true);
			
			if(updateColor)
				cursorSprite.color = contextColor;
		}
		else {
			contextSprite.SetActive(false);
			
			if(updateColor)
				cursorSprite.color = neutralColor;
		}
	}
	
	void OnDestroy() {
		mCursors.Remove(type);
	}
	
	void Awake() {
		mCursors.Add(type, this);
		
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
	}
		
	void OnDrawGizmosSelected() {
		Vector3 s = origin.position, e = transform.position;
		
		Gizmos.color = Color.blue;
		Gizmos.DrawLine(s, e);
		
		Gizmos.color = Color.cyan;
		Gizmos.DrawWireSphere(e, radius);
	}
}
