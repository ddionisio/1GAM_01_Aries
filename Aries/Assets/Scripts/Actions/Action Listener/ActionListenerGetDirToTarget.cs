using UnityEngine;
using System.Collections;
using HutongGames.PlayMaker;

namespace Game.Actions {
    [ActionCategory("Game")]
    [Tooltip("Get the direction towards the target.")]
    public class ActionListenerGetDirToTarget : FSMActionComponentBase<ActionListener> {
        [RequiredField]
        [UIHint(UIHint.Variable)]
        [Tooltip("Store the normal value.")]
        public FsmVector2 storeVector;

        public override void Reset() {
            base.Reset();

            storeVector = null;
        }

        public override void OnEnter() {
            base.OnEnter();

            if(mComp != null && mComp.currentTarget != null) {
                Vector2 pos = mComp.transform.position;
                Vector2 targetPos = mComp.currentTarget.transform.position;
                storeVector.Value = (targetPos-pos).normalized;
            }

            Finish();
        }
    }
}