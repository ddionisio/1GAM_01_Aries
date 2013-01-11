using UnityEngine;
using System.Collections;

[System.Serializable]
public class ActionInfo {
	public enum Type {
		Move,
		Disperse,
		Attack,
		
		NumType
	}
	
	public enum Priority {
		Highest,
		High,
		Normal,
		Low
	}
	
	public const int Unlimited = -1;
	
	public Type type;
	public Priority priority = Priority.Normal;
	public int limit = Unlimited; //-1 is no limit for who can perform this action within the region
}
