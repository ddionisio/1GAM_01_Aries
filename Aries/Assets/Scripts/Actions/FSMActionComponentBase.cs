using UnityEngine;
using HutongGames.PlayMaker;

namespace Game.Actions {
	public abstract class FSMActionComponentBase<T> : FsmStateAction where T : Component
	{
		[Tooltip("The Game Object to work with.")]
		public FsmOwnerDefault owner;
		
		public bool checkChildren;
		
		protected T mComp;
		protected GameObject mOwnerGO;
		
		public override void Reset()
		{
			owner = null;
			checkChildren = false;
		}
		
		public override void OnEnter ()
		{
			mOwnerGO = Fsm.GetOwnerDefaultTarget(owner);
			mComp = mOwnerGO == null ? null : checkChildren ? mOwnerGO.GetComponentInChildren<T>() : mOwnerGO.GetComponent<T>();
#if UNITY_EDITOR
			if(mComp == null) {
				LogWarning("Component: "+typeof(T)+" not found for "+mOwnerGO.name);
			}
#endif
		}
	}
}
