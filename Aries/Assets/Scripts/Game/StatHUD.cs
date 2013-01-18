using UnityEngine;
using System.Collections;

public class StatHUD : MonoBehaviour {
	
	public float decayDelay = 1.5f;
	
	private bool mShow = true;
	
	public bool show {
		get { return mShow; }
		
		set {
			if(mShow != value) {
				if(value) {
					DoShow();
				}
				else {
					DoHide();
				}
				
				mShow = value;
			}
		}
	}
	
	//changed = when a stat value was changed
	public virtual void StatsRefresh(StatBase stat, bool changed) {
		//refresh values
		
		//possibly show display if changed is true
	}
	
	protected virtual void OnActivate() {
	}
	
	protected virtual void OnDeactivate() {
	}
	
	private void DoShow() {
		gameObject.SetActive(true);
		
		OnActivate();
		
		Invoke("DoHide", decayDelay);
	}
	
	private void DoHide() {
		OnDeactivate();
		
		gameObject.SetActive(false);
		
		mShow = false;
	}
}
