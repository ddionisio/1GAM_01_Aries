using UnityEngine;
using System.Collections;

public class TransAnimWaveOfs : MonoBehaviour {
	public float pulsePerSecond;
	
	public Vector2 ofs;
	
	private float mCurPulseTime = 0;
	
	private Vector2 mStartPos;
	private Vector2 mEndPos;
	
	void OnEnable() {
		mCurPulseTime = 0;
	}
	
	void Awake() {
		mStartPos = transform.localPosition;
		mEndPos = mStartPos + ofs;
	}
	
	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		mCurPulseTime += Time.deltaTime;
		
		float t = Mathf.Sin(Mathf.PI*mCurPulseTime*pulsePerSecond);
		
		Vector2 newPos = Vector2.Lerp(mStartPos, mEndPos, t*t);
		
		transform.localPosition = new Vector3(newPos.x, newPos.y, transform.localPosition.z);
	}
}
