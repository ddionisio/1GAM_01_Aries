using UnityEngine;
using HutongGames.PlayMaker;

namespace M8.Actions {
	[ActionCategory("M8/Transform")]
	[Tooltip("Gets the Up dir of a Game Object and stores it in a Vector3 Variable or each Axis in a Float Variable")]
	public class GetUpDir : FsmStateAction
	{
		[RequiredField]
		public FsmOwnerDefault gameObject;
		
		[UIHint(UIHint.Variable)]
		public FsmVector3 vector;
		
		[UIHint(UIHint.Variable)]
		public FsmFloat x;
		
		[UIHint(UIHint.Variable)]
		public FsmFloat y;
		
		[UIHint(UIHint.Variable)]
		public FsmFloat z;
		
		public bool everyFrame;

		public override void Reset()
		{
			gameObject = null;
			vector = null;
			x = null;
			y = null;
			z = null;
			everyFrame = false;
		}

		public override void OnEnter()
		{
			DoGetPosition();
			
			if (!everyFrame)
			{
				Finish();
			}		
		}

		public override void OnUpdate()
		{
			DoGetPosition();
		}

		void DoGetPosition()
		{
			var go = Fsm.GetOwnerDefaultTarget(gameObject);
			if (go == null)
			{
				return;
			}

			var up = go.transform.up;				
			
			vector.Value = up;
			x.Value = up.x;
			y.Value = up.y;
			z.Value = up.z;
		}


	}
}