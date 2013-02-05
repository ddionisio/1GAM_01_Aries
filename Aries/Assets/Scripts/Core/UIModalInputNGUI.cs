using UnityEngine;
using System.Collections;

/// <summary>
/// Allow InputManager to handle input for NGUI.  Put this component along with ui modal manager. Use in conjuction with UIButtonKeys for each item.
/// </summary>

[RequireComponent(typeof(Collider))]
public class UIModalInputNGUI : MonoBehaviour {

    public InputAction axisX;
    public InputAction axisY;
    public InputAction[] enter;
    public InputAction[] cancel;

    private bool mInputActive = false;

    void OnEnable() {
        if(mInputActive) {
            StartCoroutine(AxisCheck());
        }
    }

    void OnDestroy() {
        OnUIModalInactive();
    }

    void OnInputEnter(InputManager.Info data) {
        if(UICamera.selectedObject != null) {
            UICamera.Notify(UICamera.selectedObject, "OnKey", KeyCode.Return);
        }
    }

    void OnInputCancel(InputManager.Info data) {
        if(UICamera.selectedObject != null) {
            UICamera.Notify(UICamera.selectedObject, "OnKey", KeyCode.Escape);
        }
    }

    IEnumerator AxisCheck() {
        while(mInputActive) {
            if(UICamera.selectedObject != null) {
                InputManager input = Main.instance.input;

                float x = input.GetAxis(axisX);
                if(x < 0.0f) {
                    UICamera.Notify(UICamera.selectedObject, "OnKey", KeyCode.LeftArrow);
                }
                else if(x > 0.0f) {
                    UICamera.Notify(UICamera.selectedObject, "OnKey", KeyCode.RightArrow);
                }

                float y = input.GetAxis(axisY);
                if(y < 0.0f) {
                    UICamera.Notify(UICamera.selectedObject, "OnKey", KeyCode.UpArrow);
                }
                else if(y > 0.0f) {
                    UICamera.Notify(UICamera.selectedObject, "OnKey", KeyCode.DownArrow);
                }
            }

            yield return new WaitForFixedUpdate();
        }

        yield break;
    }

    void OnUIModalActive() {
        if(!mInputActive) {
            //bind callbacks
            InputManager input = Main.instance.input;

            foreach(InputAction a in enter)
                input.AddButtonCall(a, OnInputEnter);

            foreach(InputAction a in cancel)
                input.AddButtonCall(a, OnInputCancel);

            mInputActive = true;

            if(gameObject.activeInHierarchy) {
                StartCoroutine(AxisCheck());
            }
        }
    }

    void OnUIModalInactive() {
        if(mInputActive) {
            //bind callbacks
            InputManager input = Main.instance != null ? Main.instance.input : null;

            if(input != null) {
                foreach(InputAction a in enter)
                    input.RemoveButtonCall(a, OnInputEnter);

                foreach(InputAction a in cancel)
                    input.RemoveButtonCall(a, OnInputCancel);
            }

            mInputActive = false;
        }
    }
}