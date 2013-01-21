using UnityEngine;
using System.Collections;

public class FlockFilter : MonoBehaviour {
	public FlockType type;
	public int avoidTypeFilter; //which types to avoid, like the plague
	
	private FlockUnit mFlockUnit;
	
	public bool isLegit { get { return mFlockUnit != null && mFlockUnit.groupMoveEnabled; } }
	
	public void Awake() {
		mFlockUnit = GetComponent<FlockUnit>();
	}
			
	/// <summary>
	/// Check given 'other' to see if we should avoid it.
	/// </summary>
	public bool CheckAvoid(FlockType other) {
		return (avoidTypeFilter & (1<<((int)other))) != 0;
	}
}
