using UnityEngine;
using System.Collections;

public class UserSettings : MonoBehaviour {
	public const string muteKey = "m";
	public const string volumeKey = "v";
			
	public bool isMute { 
		get { return mMute; }
		
		set {
			mMute = value;
			PlayerPrefs.SetInt(muteKey, mMute ? 1 : 0);
			
			ApplyAudioSettings();
		}
	}
	
	public float volume { 
		get { return mVolume; } 
		
		set {
			mVolume = value;
			PlayerPrefs.SetFloat(volumeKey, mVolume);
			
			ApplyAudioSettings();
		}
	}
	
	//need to debug while listening to music
#if UNITY_EDITOR
	private const int muteDefault = 1;
#else
	private const int muteDefault = 0;
#endif
	
	private bool mMute;
	private float mVolume;
	
	// Use this for initialization
	void Awake () {
		//load settings
		mVolume = PlayerPrefs.GetFloat(volumeKey, 1.0f);
						
		mMute = PlayerPrefs.GetInt(muteKey, muteDefault) > 0;
		
		ApplyAudioSettings();
	}
	
	void ApplyAudioSettings() {
		AudioListener.volume = mMute ? 0.0f : mVolume;
	}
}
