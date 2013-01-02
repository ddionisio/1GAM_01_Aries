using UnityEngine;
using System.Collections;

public class HUDInterface : MonoBehaviour {
	
	private static HUDInterface mInstance;
	
	public static HUDInterface instance {
		get { return mInstance; }
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
	
	}
	
}
