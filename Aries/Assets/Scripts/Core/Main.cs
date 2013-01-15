using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Main : MonoBehaviour {
	[System.NonSerialized] public UserSettings userSettings;
	[System.NonSerialized] public UserData userData;
	[System.NonSerialized] public SceneManager sceneManager;
	[System.NonSerialized] public InputManager input;
	
	private static Main mInstance = null;
	
	public static Main instance {
		get {
			return mInstance;
		}
	}
	
	public SceneController sceneController {
		get {
			return sceneManager.sceneController;
		}
	}
	
	void OnDestroy() {
		mInstance = null;
	}
	
	void OnEnable() {
	}
	
	void OnDisable() {
		if(userData != null) {
			userData.Save();
		}
	}
			
	void Awake() {
		mInstance = this;
						
		DontDestroyOnLoad(gameObject);
		
		userData = new UserData();
		userSettings = new UserSettings();
		
		sceneManager = GetComponentInChildren<SceneManager>();
		
		input = GetComponentInChildren<InputManager>();
	}
	
	void Start() {
		//TODO: maybe do other things before starting the game
		//go to start if we are in main scene
		SceneManager.Scene mainScene = SceneManager.Scene.main;
		if(Application.loadedLevelName == mainScene.ToString()) {
			sceneManager.LoadScene(SceneManager.Scene.start);
		}
		else {
			sceneManager.InitScene();
		}
		
	}
}
