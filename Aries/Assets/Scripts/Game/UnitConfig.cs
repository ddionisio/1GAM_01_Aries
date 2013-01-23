using UnityEngine;
using System.Collections.Generic;

//global units configuration loaded from file
//put this in core game object
//only use this after awake
public class UnitConfig : MonoBehaviour {
	public class UnitConfigFile {
		public UnitType type;
		public float resource;
	}
	
	public struct Data {
		public float resource;
	}
	
	public TextAsset config;
	
	private static UnitConfig mInstance = null;
	
	private Data[] mUnitsData = new Data[(int)UnitType.NumTypes];
	
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
			List<UnitConfigFile> fileData = fastJSON.JSON.Instance.ToObject<List<UnitConfigFile>>(config.text);
			
			foreach(UnitConfigFile configDat in fileData) {
				if(configDat != null) {
					mUnitsData[(int)configDat.type].resource = configDat.resource;
				}
			}
		}
	}
	
	public float GetUnitResourceCost(UnitType type) {
		return mUnitsData[(int)type].resource;
	}
}
