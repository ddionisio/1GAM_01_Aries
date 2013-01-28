using UnityEngine;
using System.Collections;

public class Jump : FlockUnit {
	public delegate void JumpCompleteCall();
	
	public float speed; //m/s
	public UnitSpriteController spriteCtrl;
	public float height;
	
	public UnitSpriteState jumpUpState = UnitSpriteState.JumpUp;
	public UnitSpriteState jumpDownState = UnitSpriteState.JumpDown;
	
	public event JumpCompleteCall jumpFinishCallback;
	
	private Vector2 mDest;
	
	public Vector2 dest {
		get { return mDest; }
	}
	
	private bool mJumping;
	private float mStartJumpTime;
	private float mJumpDelay;
	private float mSpriteStartY;
	
	public bool JumpToTarget() {
		ActionListener listener = GetComponent<ActionListener>();
		if(listener != null && listener.currentTarget != null) {
			return JumpTo(listener.currentTarget.transform.position);
		}
		
		return false;
	}
	
	public override void ResetData() {
		JumpReset();
												
		base.ResetData();
	}
	
	protected override void OnDestroy () {
		jumpFinishCallback = null;
		
		base.OnDestroy ();
	}
	
	// Update is called once per frame
	protected override void Update() {
		if(mJumping) {
			float t = (Time.time - mStartJumpTime)/mJumpDelay;
			
			if(t < 1.0f) {
				if(spriteCtrl.state != jumpDownState && t >= 0.5f) {
					spriteCtrl.state = jumpDownState;
				}
				
				Vector3 sprPos = spriteCtrl.sprite.transform.position;
				sprPos.y = mSpriteStartY + Mathf.Sin(Mathf.PI*t)*height;
				spriteCtrl.sprite.transform.position = sprPos;
				
				Vector2 pos = mDir*speed*Time.deltaTime;
				transform.position = new Vector3(pos.x, pos.y, transform.position.z);
			}
			else {
				transform.position = new Vector3(mDest.x, mDest.y, transform.position.z);
				JumpReset();
				
				if(jumpFinishCallback != null) {
					jumpFinishCallback();
				}
			}
		}
		else {
			base.Update();
		}
	}
	
	protected override void FixedUpdate () {
		if(!mJumping) {
			base.FixedUpdate();
		}
	}
	
	private bool JumpTo(Vector2 toPos) {
		if(!mJumping) {
			//initialize jumping
			mDest = toPos;
			
			Vector2 sPos = transform.position;
			
			mDir = mDest - sPos;
			
			float dist = mDir.magnitude;
			
			if(dist > 0.0f) {
				mDir /= dist;
				mCurSpeed = speed;
				
				mSpriteStartY = spriteCtrl.sprite.transform.position.y;
				
				mStartJumpTime = Time.time;
				mJumpDelay = dist/speed;
				
				spriteCtrl.state = jumpUpState;
				
				moveTarget = null;
				body.isKinematic = true;
				collider.enabled = false;
				
				mJumping = true;
				
				return true;
			}
		}
		
		return false;
	}
	
	private void JumpReset() {
		if(mJumping) {
			Vector3 p = spriteCtrl.sprite.transform.position;
			p.y = mSpriteStartY;
			spriteCtrl.sprite.transform.position = p;
			
			body.isKinematic = false;
			collider.enabled = true;
			
			mJumping = false;
		}
	}
}
