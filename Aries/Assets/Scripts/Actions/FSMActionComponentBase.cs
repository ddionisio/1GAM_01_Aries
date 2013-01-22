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
		
		protected T mComp {
			get {
				GameObject go = mOwnerGO;
				return go == null ? null : checkChildren ? go.GetComponentInChildren<T>() : go.GetComponent<T>();
			}
		}
		
		public override void Reset()
		{
			owner = null;
			checkChildren = false;
		}
		
	}
}
