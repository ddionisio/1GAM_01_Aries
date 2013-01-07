using UnityEngine;
using System.Collections;

//put this with Main in a game object to persist between scenes
public class Layers : MonoBehaviour {
	//only use these after awake
	public static int layerIgnoreRaycast;
	public static int layerWall;
	
	public static int layerMaskWall;
	
	void Awake() {
		layerIgnoreRaycast = LayerMask.NameToLayer("Ignore Raycast");
		layerWall = LayerMask.NameToLayer("Wall");
			
		layerMaskWall = 1<<layerWall;
	}
}
