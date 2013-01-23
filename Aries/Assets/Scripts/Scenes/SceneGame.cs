using UnityEngine;
using System.Collections;
using HutongGames.PlayMaker;

public class SceneGame : SceneController {
	public tk2dTileMap map;
	public float orderZMin = 0.0f;
	public float orderZMax = 0.9f;
	
	private static SceneGame mInstance;
	
	private float mOrderYOfs;
	private float mOrderYMax;
	
	private PlayMakerFSM mFSM;
	
	public static SceneGame instance {
		get { return mInstance; }
	}
	
	public PlayMakerFSM FSM {
		get { return mFSM; }
	}
	
	public float ComputeZOrder(float y) {
		return orderZMin + ((y+mOrderYOfs)/mOrderYMax)*(orderZMax-orderZMin);
	}
	
	protected override void OnDestroy() {
		base.OnDestroy();
		
		mInstance = null;
	}
	
	protected override void Start() {
		base.Start();
	}
	
	protected override void Awake() {
		base.Awake();
		
		mInstance = this;
		
		mFSM = GetComponent<PlayMakerFSM>();
		
		//FsmVariables.GlobalVariables.GetFsmString("fuck").Value
		mOrderYOfs = -map.data.tileOrigin.y;
		mOrderYMax = map.height*map.data.tileSize.y;
	}
}
