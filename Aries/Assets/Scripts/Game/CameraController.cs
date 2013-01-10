using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {
	
	public Camera mainCamera;
	
	public float moveDelay;
	
	private static CameraController mInstance;
	
	private Transform mAttachTo;
	
	private Hashtable mMoveParam;
		
	public static CameraController instance { get { return mInstance; } }
	
	public Transform attachTo {
		get { return mAttachTo; }
		
		set {
			mAttachTo = value;
			
			if(mAttachTo != null) {
				mMoveParam.Add(iT.MoveUpdate.position, mAttachTo);
			}
			else {
				mMoveParam.Remove(iT.MoveUpdate.position);
			}
		}
	}
	
	void OnDestroy() {
		mInstance = null;
	}
	
	void Awake() {
		mInstance = this;
		
		mMoveParam = new Hashtable(2);
		mMoveParam.Add(iT.MoveUpdate.time, moveDelay);
	}

	// Use this for initialization
	void Start () {
		//iTween.mo
	}
	
	// Update is called once per frame
	void Update () {
		if(mAttachTo != null) {
			iTween.MoveUpdate(gameObject, mMoveParam);
		}
	}
}
