using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {
	
	public Camera mainCamera;
	
	public float moveDelay;
	
	private static CameraController mInstance;
	
	private Transform mAttachTo;
	
	private Vector2 mMoveStart;
	
	private float mCurMoveTime;
	
	public CameraController instance { get { return mInstance; } }
	
	public Transform attachTo {
		get { return mAttachTo; }
		
		set {
			mAttachTo = value;
			
			if(mAttachTo != null) {
				StartMove();
			}
		}
	}
	
	void OnDestroy() {
		mInstance = null;
	}
	
	void Awake() {
		mInstance = this;
		
		mCurMoveTime = moveDelay;
	}

	// Use this for initialization
	void Start () {
		//iTween.mo
	}
	
	// Update is called once per frame
	void Update () {
		if(mAttachTo != null) {
			if(mCurMoveTime < moveDelay) {
				mCurMoveTime += Time.deltaTime;
										
				Vector2 dest = mAttachTo.position;
							
				if(mCurMoveTime < moveDelay) {			
					transform.position = new Vector2(
						M8.Ease.Out(mCurMoveTime, moveDelay, mMoveStart.x, dest.x - mMoveStart.x),
						M8.Ease.Out(mCurMoveTime, moveDelay, mMoveStart.y, dest.y - mMoveStart.y));
				}
				else {
					transform.position = dest;
				}
			}
		}
		else if(transform.position != mAttachTo.position) {
			StartMove();
		}
	}
	
	private void StartMove() {
		mMoveStart = transform.position;
		mCurMoveTime = 0.0f;
	}
}
