using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MusicManager : MonoBehaviour {
	[System.Serializable]
	public class MusicData {
		public string name;
		public AudioSource source;
		public float loopDelay;
	}
	
	public MusicData[] music;
	
	public float changeFadeOutDelay;
	
	public string playOnStart;
	
	private static MusicManager mInstance = null;
	
	private enum State {
		None,
		Playing,
		Changing
	}
	
	private const double rate = 44100;
	
	private Dictionary<string, MusicData> mMusic;
	private float mCurTime = 0;
	
	private State mState = State.None;
	
	private MusicData mCurMusic;
	private MusicData mNextMusic;
	
	public static MusicManager instance {
		get {
			return mInstance;
		}
	}
	
	public bool IsPlaying() {
		return mState == State.Playing;
	}
	
	public void Play(string name, bool immediate) {
		if(immediate) {
			Stop(false);
		}
		
		if(mCurMusic != null) {
			mNextMusic = mMusic[name];
			SetState(State.Changing);
		}
		else {
			mCurMusic = mMusic[name];
			mCurMusic.source.volume = 1.0f;
			mCurMusic.source.Play();
			SetState(State.Playing);
		}
	}
	
	public void Stop(bool fade) {
		if(mState != State.None) {
			if(fade) {
				mNextMusic = null;
				SetState(State.Changing);
			}
			else {
				mCurMusic.source.Stop();
				SetState(State.None);
			}
		}
	}
	
	void OnDestroy() {
		mInstance = null;
	}
	
	void Awake() {
		mInstance = this;
		
		mMusic = new Dictionary<string, MusicData>(music.Length);
		foreach(MusicData dat in music) {
			mMusic.Add(dat.name, dat);
		}
	}

	// Use this for initialization
	void Start () {
		if(!string.IsNullOrEmpty(playOnStart)) {
			Play(playOnStart, true);
		}
	}
	
	void SetState(State state) {
		mState = state;
		mCurTime = 0;
	}
	
	// Update is called once per frame
	void Update () {
		switch(mState) {
		case State.None:
			break;
		case State.Playing:
			if(!(mCurMusic.source.loop || mCurMusic.source.isPlaying)) {
				mCurMusic.source.Play((ulong)System.Math.Round(rate*((double)mCurMusic.loopDelay)));
			}
			break;
		case State.Changing:
			mCurTime += Time.deltaTime;
			if(mCurTime >= changeFadeOutDelay) {
				mCurMusic.source.Stop();
				
				if(mNextMusic != null) {
					mCurMusic.source.volume = 1.0f;
					mNextMusic.source.Play();
					
					mCurMusic = mNextMusic;
					mNextMusic = null;
					
					SetState(State.Playing);
				}
				else {
					SetState(State.None);
				}
			}
			else {
				mCurMusic.source.volume = 1.0f - mCurTime/changeFadeOutDelay;
			}
			break;
		}
	}
}
