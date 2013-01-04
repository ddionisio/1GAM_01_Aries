using UnityEngine;
using System.Collections;

//put this with Main in a game object to persist between scenes
public class Layers : MonoBehaviour {
	//only use these after awake
	public static int layerIgnoreRaycast;
	public static int layerFlockPlayer;
	
	public static int layerMaskFlockPlayer;
	
	private static Layers mInstance = null;
	
	public static Layers instance {
		get {
			return mInstance;
		}
	}
	
	void OnDestroy() {
		mInstance = null;
	}
	
	void Awake() {
		mInstance = this;
		
		layerIgnoreRaycast = LayerMask.NameToLayer("Ignore Raycast");
		layerFlockPlayer = LayerMask.NameToLayer("FlockPlayer");
			
		layerMaskFlockPlayer = 1<<layerFlockPlayer;
	}
}
