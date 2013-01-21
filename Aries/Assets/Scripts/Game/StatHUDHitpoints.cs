using UnityEngine;
using System.Collections;

public class StatHUDHitpoints : StatHUD {
	public UISlider slider;
	
	public override void StatsRefresh(StatBase stat) {
		slider.sliderValue = stat.HPScale;
	}
}
