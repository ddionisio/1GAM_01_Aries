using UnityEngine;
using HutongGames.PlayMaker;

namespace Game.Actions {
	public class SceneValueGet : FsmStateAction
	{
        [RequiredField]
        public FsmString name;

        [RequiredField]
        [UIHint(UIHint.Variable)]
        public FsmInt toValue;
				
		public override void Reset()
		{
            name = null;
            toValue = null;
		}
		
		public override void OnEnter ()
		{
            if(SceneState.instance != null) {
                toValue.Value = SceneState.instance.GetValue(name.Value);
            }

            Finish();
		}
	}
}
