using UnityEngine;
using System.Collections;

public class StatHUDHitpoints : StatHUD {
	public UISlider slider;
	
	public override void StatsRefresh(StatBase stat, bool changed) {
		slider.sliderValue = stat.HPScale;
		
		if(changed) {
			show = true;
		}
	}
}
