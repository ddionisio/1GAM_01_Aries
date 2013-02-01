using UnityEngine;
using System.Collections.Generic;

//global spell configuration loaded from file
//put this in core game object
//only use this after awake
public class SpellConfig : MonoBehaviour {
	public class Info {
		public string name;
		
		public float cooldown;
		public float castDelay;
		
		public SpellFlag[] flags;
		
		public SpellBase data;
	}
	
	public TextAsset config;
	
	private static Dictionary<string, Info> mSpells;
	
	public static Info GetInfo(string name) {
		Info ret = null;
		
		mSpells.TryGetValue(name, out ret);
		
		return ret;
	}
	
	void OnDestroy() {
		mSpells = null;
	}
	
	void Awake() {
		//load file
		if(config != null) {
			fastJSON.JSON.Instance.Parameters.UseExtensions = true;
			List<Info> fileData = fastJSON.JSON.Instance.ToObject<List<Info>>(config.text);
			
			mSpells = new Dictionary<string, Info>(fileData.Count);
			
			int id = 1;
			
			foreach(Info info in fileData) {
				if(info != null && info.data != null) {
					info.data._setId(id); id++;
					
					SpellFlag sf = (SpellFlag)0;
					foreach(SpellFlag sfinfo in info.flags) {
						sf |= sfinfo;
					}
					info.data._setFlags(sf);
					
					mSpells.Add(info.name, info);
				}
			}
		}
	}
}
