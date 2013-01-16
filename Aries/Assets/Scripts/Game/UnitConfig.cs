using UnityEngine;
using System.Collections.Generic;

//global units configuration loaded from file
//put this in core game object
//only use this after awake
public class UnitConfig : MonoBehaviour {
	public class UnitConfigData {
		public string label; //just to make it easy to refer in the config when editing
		public float resource;
	}
	
	public TextAsset config;
	
	private static UnitConfig mInstance = null;
	
	private List<UnitConfigData> mUnitsData = null;
	
	public static UnitConfig instance {
		get { return mInstance; }
	}
	
	void OnDestroy() {
		mInstance = null;
	}
	
	void Awake() {
		mInstance = this;
		
		//load file
		if(config != null) {
			fastJSON.JSON.Instance.Parameters.UseExtensions = false;
			mUnitsData = fastJSON.JSON.Instance.ToObject<List<UnitConfigData>>(config.text);
		}
	}
	
	public float GetUnitResourceCost(UnitType type) {
		return mUnitsData != null && mUnitsData[(int)type] != null ? mUnitsData[(int)type].resource : 0.0f;
	}
}
