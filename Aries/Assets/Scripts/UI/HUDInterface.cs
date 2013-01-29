using UnityEngine;
using System.Collections;

public class HUDInterface : MonoBehaviour {
	public HUDPlayer[] playerHUDs;
	
	private static HUDInterface mInstance;
	
	public static HUDInterface instance {
		get { return mInstance; }
	}
	
	public HUDPlayer GetHUDPlayer(Player player) {
		return playerHUDs[player.index];
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
}
