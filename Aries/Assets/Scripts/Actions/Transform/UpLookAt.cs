using UnityEngine;
using HutongGames.PlayMaker;

namespace M8.Actions {
	[ActionCategory("M8/Transform")]
	[Tooltip("Set the up vector of game object to target")]
	public class UpLookAt : FsmStateAction
	{
		[RequiredField]
		[Tooltip("The GameObject to rorate.")]
		public FsmOwnerDefault gameObject;

		[Tooltip("The GameObject to Look At.")]
		public FsmGameObject targetObject;
		
		[Tooltip("World position to look at, or local offset from Target Object if specified.")]
		public FsmVector3 targetPosition;

		[Tooltip("Rotate the GameObject to point its up direction vector in the direction hinted at by the Up Vector. See Unity Look At docs for more details.")]
		public FsmVector3 upVector;
		
		[Tooltip("Don't rotate vertically.")]
		public FsmBool lockX;
		public FsmBool lockY;
		public FsmBool lockZ;
		
		[Title("Draw Debug Line")]
		[Tooltip("Draw a debug line from the GameObject to the Target.")]
		public FsmBool debug;

		[Tooltip("Color to use for the debug line.")] 
		public FsmColor debugLineColor;

		[Tooltip("Repeat every frame.")]
		public bool everyFrame = true;

		public override void Reset()
		{
			gameObject = null;
			targetObject = null;
			targetPosition = new FsmVector3 { UseVariable = true};
			upVector = new FsmVector3 { UseVariable = true};
			
			lockX = false;
			lockY = false;
			lockZ = false;
			
			debug = false;
			debugLineColor = Color.yellow;
			everyFrame = true;
		}

		public override void OnEnter()
		{
			DoLookAt();

			if (!everyFrame)
			{
				Finish();
			}
		}

		public override void OnLateUpdate()
		{
			DoLookAt();
		}

		void DoLookAt()
		{
			var go = Fsm.GetOwnerDefaultTarget(gameObject);
			if (go == null)
			{
				return;
			}
			
			var goTarget = targetObject.Value;
			if (goTarget == null && targetPosition.IsNone)
			{
				return;
			}

			Vector3 lookAtPos;
			if (goTarget != null)
			{
				lookAtPos = !targetPosition.IsNone ? goTarget.transform.TransformPoint(targetPosition.Value) : goTarget.transform.position;
			}
			else
			{
				lookAtPos = targetPosition.Value;
			}
			
			if (lockX.Value)
			{
				lookAtPos.x = go.transform.position.x;
			}
			
			if (lockY.Value)
			{
				lookAtPos.y = go.transform.position.y;
			}
			
			if (lockZ.Value)
			{
				lookAtPos.z = go.transform.position.z;
			}
			
			go.transform.up = lookAtPos-go.transform.position;
			
			if (debug.Value)
			{
				Debug.DrawLine(go.transform.position, lookAtPos, debugLineColor.Value);
			}
		}

	}
}