using UnityEngine;
using System.Collections;

public class UnitStatusIndicator : MonoBehaviour {
	
	public enum Icon {
		Death,
		Unsummon,
		Scared,
		Slow,
		
		NumIcons
	}
	
	public GameObject[] icons;
	
	private Icon mCurIcon = Icon.NumIcons;
	
	public Icon curIcon {
		get { return mCurIcon; }
	}
	
	public void Hide() {
		gameObject.SetActive(false);
	}
	
	//set duration > 0 for hide delay
	public void Show(Icon icon, float duration = 0.0f) {
		if(mCurIcon != Icon.NumIcons && icons[(int)mCurIcon] != null) {
			icons[(int)mCurIcon].SetActive(false);
		}
		
		mCurIcon = icon;
		
		if(mCurIcon != Icon.NumIcons) {
			gameObject.SetActive(true);
			
			if(icons[(int)mCurIcon] != null)
				icons[(int)mCurIcon].SetActive(true);
			
			if(duration > 0.0f) {
				Invoke("Hide", duration);
			}
		}
		else {
			gameObject.SetActive(false);
		}
	}
	
	void Awake() {
		gameObject.SetActive(false);
		
		foreach(GameObject go in icons) {
			if(go != null)
				go.SetActive(false);
		}
	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
