using UnityEngine;
using System.Collections;

//action for the game
public enum InputAction {
	MoveX,
	MoveY,
	
	Act,
	Recall,
	Fire,
	Menu,
	
	SummonMode,
	UnSummonMode,
	
	Summon, //make sure to set indices (0-3)
	UnSummon, //make sure to set indices (0-3)
	
	NumAction
}
