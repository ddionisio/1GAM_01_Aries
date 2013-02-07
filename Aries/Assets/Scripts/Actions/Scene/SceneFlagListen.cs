using UnityEngine;
using HutongGames.PlayMaker;

namespace Game.Actions {
    public class SceneFlagListen : FsmStateAction {
        [RequiredField]
        public FsmString name;

        [RequiredField]
        public FsmInt bit;

        public FsmEvent isTrue;
        public FsmEvent isFalse;

        public override void Reset() {
            name = null;
            bit = null;

            isTrue = null;
            isFalse = null;
        }

        public override void OnEnter() {
            if(SceneState.instance != null) {
                SceneState.instance.onValueChange += StateCallback;
            }
            else {
                Finish();
            }
        }

        public override void OnExit() {
            if(SceneState.instance != null) {
                SceneState.instance.onValueChange -= StateCallback;
            }
        }

        void StateCallback(string aName, int newVal) {
            if(name.Value == aName) {
                int mask = 1 << bit.Value;
                if((newVal & mask) != 0)
                    Fsm.Event(isTrue);
                else
                    Fsm.Event(isFalse);
            }
        }

        public override string ErrorCheck() {
            if(FsmEvent.IsNullOrEmpty(isTrue) &&
                FsmEvent.IsNullOrEmpty(isFalse))
                return "Action sends no events!";
            return "";
        }
    }
}
