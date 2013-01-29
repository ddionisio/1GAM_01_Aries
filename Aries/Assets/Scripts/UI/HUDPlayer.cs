using UnityEngine;
using System.Collections;

public class HUDPlayer : MonoBehaviour {
	public UISlider hp;
	public UISlider energy;
	
	public UILabel hpLabel;
	public UILabel energyLabel;
	
	public UISprite portrait;
	
	public HUDUnitSlot[] unitSlots;
	
	private Player mPlayer;
	
	//call this first:
	
	public void Init(Player player) {
		DeInit();
		
		mPlayer = player;
		
		//hook stuff up
		if(mPlayer != null && mPlayer.stats != null) {
			mPlayer.stats.statChangeCallback += OnStatChange;
			OnStatChange(mPlayer.stats);
			
			RefreshUnitSlots();
		}
	}
	
	public void DeInit() {
		if(mPlayer != null && mPlayer.stats != null) {
			mPlayer.stats.statChangeCallback -= OnStatChange;
			
			for(int i = 0; i < unitSlots.Length; i++) {
				unitSlots[i].Clear();
			}
			
			mPlayer = null;
		}
	}
	
	//after Init:
	
	//call after changing units in slots
	public void RefreshUnitSlots() {
		if(mPlayer != null && mPlayer.stats != null) {
			for(int i = 0; i < unitSlots.Length; i++) {
				unitSlots[i].Setup(mPlayer.stats.flockGroup, mPlayer.control.summonSlots[i]);
			}
		}
	}
	
	void OnDestroy() {
		DeInit();
	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	void OnStatChange(StatBase stat) {
		PlayerStat playerStat = (PlayerStat)stat;
		
		hp.sliderValue = playerStat.HPScale;
		hpLabel.text = Mathf.RoundToInt(playerStat.curHP).ToString();
		
		energy.sliderValue = playerStat.curResourceScale;
		energyLabel.text = Mathf.RoundToInt(playerStat.curResource).ToString();
	}
}
