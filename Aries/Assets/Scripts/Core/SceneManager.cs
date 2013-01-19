using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SceneManager : MonoBehaviour {
	
	public enum Scene {
		main,
		start,
		test
	}
			
	public const string levelString = "level";
	
	public ScreenTransition screenTransition;
	
	private SceneController mSceneController;
	private string mCurSceneStr;
	private string mCurLevelStr;
	private int mCurLevel;
	private float mPrevTimeScale;
	
	private SceneCheckpoint mCheckPoint = null;
	private string mCheckPointForScene = "";
	
	private bool mFirstTime = true;
	
	private string mSceneToLoad = null;
	
	private List<Transform> mRoots = new List<Transform>();
	
	private bool mIsFullscreen = false;
	
	public int curLevel {
		get {
			return mCurLevel;
		}
	}
	
	public SceneController sceneController {
		get {
			return mSceneController;
		}
	}
	
	public void RootBroadcastMessage(string methodName, Object param, SendMessageOptions options) {
		BroadcastMessage(methodName, param, options);
		
		foreach(Transform t in mRoots) {
			t.BroadcastMessage(methodName, param, options);
		}
	}
	
	public void SetCheckPoint(SceneCheckpoint check) {
		mCheckPointForScene = mCurSceneStr;
		mCheckPoint = check;
	}
		
	public void LoadScene(Scene scene) {
		LoadScene(scene.ToString());
	}
	
	public void LoadScene(string scene) {
		mSceneToLoad = scene;
		
		if(mFirstTime) {
			screenTransition.state = ScreenTransition.State.Done;
			DoLoad();
		}
		else {
			screenTransition.state = ScreenTransition.State.Out;
		}
	}
	
	public void LoadLevel(int level) {
		mCurLevel = level;
		mCurLevelStr = levelString+level;
		LoadScene(mCurLevelStr);
	}
	
	public void ReloadLevel() {
		if(!string.IsNullOrEmpty(mCurLevelStr)) {
			LoadScene(mCurLevelStr);
		}
	}
	
	public void Pause() {
		if(Time.timeScale != 0.0f) {
			mPrevTimeScale = Time.timeScale;
			Time.timeScale = 0.0f;
		}
		
		RootBroadcastMessage("OnScenePause", null, SendMessageOptions.DontRequireReceiver);
	}
	
	public void Resume() {
		Time.timeScale = mPrevTimeScale;
		
		RootBroadcastMessage("OnSceneResume", null, SendMessageOptions.DontRequireReceiver);
	}
	
	/// <summary>
	/// Internal use only. Called at OnLevelWasLoaded in SceneManager or in Main (for debug/dev)
	/// </summary>
	public void InitScene() {
		if(mSceneController == null) {
			mSceneController = (SceneController)Object.FindObjectOfType(typeof(SceneController));
		}
	}
	
	void OnLevelWasLoaded(int sceneInd) {
		//refresh roots, excluding main
		mRoots.Clear();
		Transform[] trans = (Transform[])FindObjectsOfType(typeof(Transform));
		foreach(Transform tran in trans) {
			if(tran.parent == null && tran != transform) {
				mRoots.Add(tran);
			}
		}
		
		InitScene();
		
		if(mCheckPoint != null && mCurSceneStr == mCheckPointForScene) {
			mSceneController.OnCheckPoint(mCheckPoint);
		}
		
		mCheckPoint = null;
		mCheckPointForScene = "";
		
		if(screenTransition.state == ScreenTransition.State.Done) { //we were at out a while back
			mFirstTime = false;
			StartCoroutine(DoScreenTransitionIn());
		}
	}
	
	void Awake() {
		mIsFullscreen = Screen.fullScreen;
		
		mPrevTimeScale = Time.timeScale;
		
		screenTransition.finishCallback = OnScreenTransitionFinish;
	}
	
	void Update() {
		//lame
		//check resolution and fullscreen
		if(mIsFullscreen != Screen.fullScreen) {
			mIsFullscreen = Screen.fullScreen;
			RootBroadcastMessage("OnSceneScreenChanged", null, SendMessageOptions.DontRequireReceiver);
		}
	}
	
	void OnScreenTransitionFinish(ScreenTransition.State state) {
		switch(state) {
		case ScreenTransition.State.In:
			//notify?
			break;
			
		case ScreenTransition.State.Out:
			DoLoad();
			break;
		}
	}
	
	void DoLoad() {
		RootBroadcastMessage("SceneChange", null, SendMessageOptions.DontRequireReceiver);
		
		mCurSceneStr = mSceneToLoad;
		
		Application.LoadLevel(mSceneToLoad);
	}
	
	IEnumerator DoScreenTransitionIn() {
		yield return new WaitForFixedUpdate();
		
		screenTransition.state = ScreenTransition.State.In;
		
		yield break;
	}
}
