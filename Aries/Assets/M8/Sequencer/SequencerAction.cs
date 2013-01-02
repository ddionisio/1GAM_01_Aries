using UnityEngine;
using System.Collections;

public class SequencerAction  {
	public float delay;
	
	//store any non-readonly fields to given behaviour, don't put them here, store them in behaviour
	//these sequence actions can be shared by different behaviours
	//can set states dependent from outside here
	public virtual void Start(MonoBehaviour behaviour, Sequencer.StateInstance state) {
	}
	
	/// <summary>
	/// Periodic update, return true if done.
	/// </summary>
	public virtual bool Update(MonoBehaviour behaviour, Sequencer.StateInstance state) {
		return true;
	}
	
	//do clean ups here, don't set any states dependent from outside
	public virtual void Finish(MonoBehaviour behaviour, Sequencer.StateInstance state) {
	}
}
