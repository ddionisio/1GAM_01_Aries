using UnityEngine;
using System.Collections;

//action for the game
public enum InputAction {
	MoveX,
	MoveY,
	
	Act,
	Fire,
	Menu,
	
	SummonSelect, //for pc
	SummonSelectNext,
	SummonSelectPrev,
	
	Summon, //make sure to set indices (0-3)
	UnSummon, //make sure to set indices (0-3)
	
	NumAction
}
