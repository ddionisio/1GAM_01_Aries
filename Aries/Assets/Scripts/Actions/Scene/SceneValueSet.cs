using UnityEngine;
using HutongGames.PlayMaker;

namespace Game.Actions {
	public class SceneValueSet : FsmStateAction
	{
        [RequiredField]
        public FsmString name;

        [RequiredField]
        public FsmInt val;

        public FsmBool persistent;
				
		public override void Reset()
		{
            name = null;
            val = null;
            persistent = false;
		}
		
		public override void OnEnter ()
		{
            if(SceneState.instance != null) {
                SceneState.instance.SetValue(name.Value, val.Value, persistent.Value);
            }

            Finish();
		}
	}
}
