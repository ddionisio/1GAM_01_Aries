using UnityEngine;
using System.Collections;

public class UnitStatusIndicator : MonoBehaviour {
	
	public enum Icon {
		Death,
		Unsummon,
		Curse,
		Scared,
		
		NumIcons
	}
	
	public GameObject[] icons;
	
	private Icon mCurIcon = Icon.NumIcons;
	
	public void Hide() {
		gameObject.SetActive(false);
	}
	
	//set duration > 0 for hide delay
	public void Show(Icon icon, float duration = 0.0f) {
		if(mCurIcon != Icon.NumIcons) {
			icons[(int)mCurIcon].SetActive(false);
		}
		
		mCurIcon = icon;
		
		if(mCurIcon != Icon.NumIcons) {
			gameObject.SetActive(true);
			
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
