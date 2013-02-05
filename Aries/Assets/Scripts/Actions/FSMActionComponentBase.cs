using UnityEngine;
using HutongGames.PlayMaker;

namespace Game.Actions {
	public abstract class FSMActionComponentBase<T> : FsmStateAction where T : Component
	{
		[Tooltip("The Game Object to work with.")]
		public FsmOwnerDefault owner;
		
		public bool checkChildren;
		
		protected GameObject mOwnerGO {
			get {
				return Fsm.GetOwnerDefaultTarget(owner);
			}
		}
		
		protected T mComp;
				
		public override void Reset()
		{
			owner = null;
			checkChildren = false;
		}
		
		public override void OnEnter ()
		{
			GameObject go = mOwnerGO;
			mComp = go == null ? null : checkChildren ? go.GetComponentInChildren<T>() : go.GetComponent<T>();
		}
	}
}
