using UnityEngine;
using System.Collections;

public class SpriteMapOrder : MonoBehaviour {
	
	public float offsetY;
	
	private Transform mTrans;
	
	void Awake() {
		mTrans = transform;
	}
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void LateUpdate () {
		Vector3 lpos = mTrans.localPosition;
		lpos.z = SceneGame.instance.ComputeZOrder(mTrans.position.y+offsetY);
		mTrans.localPosition = lpos;
	}
	
	void OnDrawGizmos() {
		Vector2 pos = transform.position;
		pos.y += offsetY;
		Gizmos.DrawIcon(pos, "cross", true);
	}
}
