using UnityEngine;
using System.Collections;

public class SpriteMapOrderStatic : MonoBehaviour {
	
	public float offsetY;
	
	private Transform mTrans;
	
	void Awake() {
		mTrans = transform;
	}
	
	void OnEnable() {
		RefreshPos();
	}
	
	// Use this for initialization
	void Start () {
		RefreshPos();
	}
	
	void RefreshPos() {
		if(SceneGame.instance != null) {
			Vector3 lpos = mTrans.localPosition;
			lpos.z = SceneGame.instance.ComputeZOrder(mTrans.position.y+offsetY);
			mTrans.localPosition = lpos;
		}
	}
	
	void OnDrawGizmos() {
		Vector2 pos = transform.position;
		pos.y += offsetY;
		Gizmos.DrawIcon(pos, "cross", true);
	}
}
