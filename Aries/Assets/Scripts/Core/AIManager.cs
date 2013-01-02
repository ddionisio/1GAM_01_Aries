using UnityEngine;
using System.Collections;

public class AIManager : MonoBehaviour {
	public SequencerState states;
	
	private static AIManager mInstance;
	
	public static AIManager instance {
		get {
			return mInstance;
		}
	}
	
	void OnDestroy() {
		mInstance = null;
	}
	
	void Awake() {
		mInstance = this;
		
		states.Load();
	}
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
