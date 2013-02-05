using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UIModalManager : MonoBehaviour {
    public enum Modal {
        Start,
        GameOptions,
        HowToPlay,
        Victory,
        GameOver,
        Confirm,

        NumModal
    }

    [System.Serializable]
    public class UIData {
        public string name;
        public UIController ui;
        public bool exclusive = true; //hide modals behind

        [System.NonSerializedAttribute]
        public Modal type;
    }

    public UIData[] uis;

    public Modal openOnStart = Modal.NumModal;

    public static UIModalManager instance {
        get {
            return mInstance;
        }
    }

    private static UIModalManager mInstance;

    private Stack<UIData> mModalStack = new Stack<UIData>((int)Modal.NumModal);

    public bool ModalIsInStack(Modal modal) {
        bool ret = false;
        foreach(UIData uid in mModalStack) {
            if(uid.type == modal) {
                ret = true;
                break;
            }
        }
        return ret;
    }

    public Modal ModalGetTop() {
        Modal ret = Modal.NumModal;
        if(mModalStack.Count > 0) {
            ret = mModalStack.Peek().type;
        }
        return ret;
    }

    //closes all modal and open this
    public void ModalReplace(Modal modal) {
        ModalClearStack(false);
        ModalPushToStack(modal, false);

    }

    public void ModalOpen(Modal modal) {
        ModalPushToStack(modal, true);
    }

    public void ModalCloseTop() {
        if(mModalStack.Count > 0) {
            UIData uid = mModalStack.Pop();
            UIController ui = uid.ui;
            ui.OnShow(false);
            ui.OnClose();
            ui.gameObject.SetActive(false);

            if(mModalStack.Count == 0) {
                ModalInactive();
            }
            else {
                //re-show top
                UIData prevUID = mModalStack.Peek();
                UIController prevUI = prevUID.ui;
                if(!prevUI.gameObject.activeSelf) {
                    prevUI.gameObject.SetActive(true);
                    prevUI.OnShow(true);
                }
            }
        }
    }

    public void ModalCloseAll() {
        ModalClearStack(true);
    }

    void ModalPushToStack(Modal modal, bool evokeActive) {
        if(evokeActive && mModalStack.Count == 0) {
            SceneManager.RootBroadcastMessage("OnUIModalActive", null, SendMessageOptions.DontRequireReceiver);
        }



        UIData uid = uis[(int)modal];

        if(uid.exclusive && mModalStack.Count > 0) {
            //hide below
            UIData prevUID = mModalStack.Peek();
            UIController prevUI = prevUID.ui;
            prevUI.OnShow(false);
            prevUI.gameObject.SetActive(false);
        }

        UIController ui = uid.ui;
        ui.gameObject.SetActive(true);
        ui.OnOpen();
        ui.OnShow(true);

        mModalStack.Push(uid);
    }

    void ModalClearStack(bool evokeInactive) {
        if(mModalStack.Count > 0) {
            foreach(UIData uid in mModalStack) {
                UIController ui = uid.ui;
                ui.OnShow(false);
                ui.OnClose();
                ui.gameObject.SetActive(false);
            }

            mModalStack.Clear();

            if(evokeInactive) {
                ModalInactive();
            }
        }
    }

    void ModalInactive() {
        SceneManager.RootBroadcastMessage("OnUIModalInactive", null, SendMessageOptions.DontRequireReceiver);
    }

    void SceneChange() {
        ModalCloseAll();
    }

    void OnDestroy() {
        mInstance = null;
    }

    void Awake() {
        mInstance = this;

        //setup data and deactivate object
        for(int i = 0; i < uis.Length; i++) {
            UIData uid = uis[i];
            UIController ui = uid.ui;
            if(ui != null) {
                ui.gameObject.SetActive(false);
            }

            uid.type = (Modal)i;//System.Enum.Parse(typeof(Modal), uid.name);
        }
    }

    void Start() {
        if(openOnStart != Modal.NumModal) {
            ModalOpen(openOnStart);
        }
    }
}
