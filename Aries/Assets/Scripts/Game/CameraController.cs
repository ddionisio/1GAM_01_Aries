using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {
	
	public Camera mainCamera;
	
	public float moveDelay;
	
	private static CameraController mInstance;
	
	private Transform mAttachTo;
	
	private Vector3 mCurVelocity = Vector3.zero;
		
	public static CameraController instance { get { return mInstance; } }
	
	public Transform attachTo {
		get { return mAttachTo; }
		
		set {
			mAttachTo = value;
		}
	}
	
	void OnDestroy() {
		mInstance = null;
	}
	
	void Awake() {
		mInstance = this;
	}

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		if(mAttachTo != null) {
			transform.position = Vector3.SmoothDamp(transform.position, mAttachTo.position, ref mCurVelocity, moveDelay);
		}
	}
}
