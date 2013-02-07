using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Main : MonoBehaviour {
    [System.NonSerialized]
    public UserSettings userSettings;
    [System.NonSerialized]
    public SceneManager sceneManager;
    [System.NonSerialized]
    public InputManager input;

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
        PlayerPrefs.Save();
    }

    void Awake() {
        mInstance = this;

        DontDestroyOnLoad(gameObject);

        //TODO: determine platform
        GamePlatform.current = GamePlatform.Type.Default;

        userSettings = new UserSettings();

        sceneManager = GetComponentInChildren<SceneManager>();

        input = GetComponentInChildren<InputManager>();

        //load the string table
        GameLocalize l = GetComponentInChildren<GameLocalize>();
        if(l != null) {
            l.Load(userSettings.language);
        }
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
