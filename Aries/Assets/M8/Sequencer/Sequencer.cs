using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Sequencer {
	[System.Serializable]
	public class StateData {
		public string name;
		public TextAsset source;
	}
	
	public class StateInstance {
		public bool pause=false;
		public bool terminate=false;
		public int counter=0;
		public float startTime=0; //the time when start happened
		
		public bool IsDelayReached(float delay) {
			return Time.time >= startTime + delay;
		}
	}
	
	public bool loop = false;
	public List<SequencerAction> actions = null;
			
	public static Dictionary<string, Sequencer> Load(StateData[] sequences) {
		fastJSON.JSON.Instance.UseSerializerExtension = true;
		
		Dictionary<string, Sequencer> ret = new Dictionary<string, Sequencer>(sequences.Length);
		
		foreach(StateData dat in sequences) {
			if(dat.source != null) {
				Sequencer newSequence = (Sequencer)fastJSON.JSON.Instance.ToObject(dat.source.text, typeof(Sequencer));
				ret[dat.name] = newSequence;
			}
		}
		
		return ret;
	}
		
	public IEnumerator Go(StateInstance stateInstance, MonoBehaviour behaviour) {
		if(actions != null) {
			int i = 0;
			int len = actions.Count;
			while(!stateInstance.terminate && i < len) {
				SequencerAction action = actions[i];
				
				if(action.delay > 0) {
					yield return new WaitForSeconds(action.delay);
				}
				
				//ensure nothing is started when we pause or terminate early
				while(!stateInstance.terminate && stateInstance.pause) {
					yield return new WaitForFixedUpdate();
				}
				
				if(stateInstance.terminate) {
					break;
				}
				
				stateInstance.startTime = Time.time;
				action.Start(behaviour, stateInstance);
				
				while(!stateInstance.terminate && !action.Update(behaviour, stateInstance)) {
					yield return new WaitForFixedUpdate();
					
					//ensure we wait until unpaused before updating again
					//warning: state from start might change.........
					//look at this comment if shit hits the fan in game
					//however...we don't want finish or update to happen when we
					//rely on certain state to be consistent outside...
					while(!stateInstance.terminate && stateInstance.pause) {
						yield return new WaitForFixedUpdate();
						continue;
					}
				}
				
				action.Finish(behaviour, stateInstance);
				
				i++;
				if(loop && i == len) {
					i = 0;
					yield return new WaitForFixedUpdate();
				}
			}
		}
		
		yield break;
	}
}
