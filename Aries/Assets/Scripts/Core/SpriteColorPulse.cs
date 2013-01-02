using UnityEngine;
using System.Collections;

public class SpriteColorPulse : MonoBehaviour {
	public float pulsePerSecond;
	public Color startColor;
	public Color endColor = Color.white;
	
	private tk2dBaseSprite mSprite;
	
	private Color mDestColor;
	private float mCurPulseTime = 0;
	
	void Awake() {
		mSprite = GetComponent<tk2dBaseSprite>();
	}
	
	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		mCurPulseTime += Time.deltaTime;
		
		float t = Mathf.Sin(Mathf.PI*mCurPulseTime*pulsePerSecond);
		t *= t;
		
		mSprite.color = Color.Lerp(startColor, endColor, t);
	}
}
