using UnityEngine;
using System.Collections.Generic;

//global units configuration loaded from file
//put this in core game object
//only use this after awake
public class UnitConfig : MonoBehaviour {
	public class Data {
		public UnitType type;
		public float resource; //resource cost per unit
		public float summonCooldown;
		public int summonAmount;
		public int summonMax;
	}
	
	public TextAsset config;
	
	private static Data[] mUnitsData;
	
	public static Data GetData(UnitType type) {
		return mUnitsData[(int)type];
	}
	
	void OnDestroy() {
		mUnitsData = null;
	}
	
	void Awake() {
		mUnitsData = new Data[(int)UnitType.NumTypes];
		
		//load file
		if(config != null) {
			fastJSON.JSON.Instance.Parameters.UseExtensions = false;
			List<Data> fileData = fastJSON.JSON.Instance.ToObject<List<Data>>(config.text);
			
			foreach(Data configDat in fileData) {
				if(configDat != null) {
					mUnitsData[(int)configDat.type] = configDat;
				}
			}
		}
	}
}
