using UnityEngine;
using HutongGames.PlayMaker;

[ActionCategory("Game")]
[Tooltip("Set entity's state")]
public class EntitySetState : FsmStateAction
{
	[RequiredField]
	[Tooltip("The entity to set state")]
	[UIHint(UIHint.FsmGameObject)]
	public EntityBase entity;
	
	[Tooltip("The state to change to")]
	public EntityState state = EntityState.normal;
	
	public override void Reset ()
	{
		entity = null;
		state = EntityState.normal;
	}
	
	// Code that runs on entering the state.
	public override void OnEnter()
	{
		entity.state = state;
		
		Finish();
	}


}
