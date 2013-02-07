using UnityEngine;
using HutongGames.PlayMaker;

namespace Game.Actions {
	public class SceneFlagSet : FsmStateAction
	{
        [RequiredField]
        public FsmString name;

        [RequiredField]
        public FsmInt bit;

        [RequiredField]
        public FsmBool val;

        public FsmBool persistent;
				
		public override void Reset()
		{
            name = null;
            bit = null;
            val = null;
            persistent = false;
		}
		
		public override void OnEnter ()
		{
            if(SceneState.instance != null) {
                SceneState.instance.SetFlag(name.Value, bit.Value, val.Value, persistent.Value);
            }

            Finish();
		}
	}
}
