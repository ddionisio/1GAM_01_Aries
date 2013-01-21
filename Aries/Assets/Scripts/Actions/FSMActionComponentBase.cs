using UnityEngine;
using HutongGames.PlayMaker;

namespace Game.Actions {
	public abstract class FSMActionComponentBase<T> : FsmStateAction where T : Component
	{
		[Tooltip("The Game Object to work with.")]
		public FsmOwnerDefault owner;
		
		public bool checkChildren;
		
		protected T mComp;
		
		public override void Reset()
		{
			owner = null;
			checkChildren = false;
		}
		
		public override void OnEnter ()
		{
			GameObject go = Fsm.GetOwnerDefaultTarget(owner);
			mComp = go == null ? null : checkChildren ? go.GetComponentInChildren<T>() : go.GetComponent<T>();
#if UNITY_EDITOR
			if(mComp == null) {
				LogWarning("Component: "+typeof(T)+" not found for "+go.name);
			}
#endif
		}
	}
}
