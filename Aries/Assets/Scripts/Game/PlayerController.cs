using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {
	public float speed;
	
	private Rigidbody mBody;
	
	void OnDestroy() {
		Main.instance.input.RemoveButtonCall(InputAction.Act, OnAction);
	}
	
	void Awake() {
		mBody = rigidbody;
	}
	
	// Use this for initialization
	void Start () {
		Main.instance.input.AddButtonCall(InputAction.Act, OnAction);
	}
	
	// Update is called once per frame
	void Update () {
		InputManager input = Main.instance.input;
		
		float moveX = input.GetAxis(InputAction.MoveX);
		float moveY = input.GetAxis(InputAction.MoveY);
		
		mBody.velocity = new Vector3(moveX*speed, moveY*speed, 0.0f);
	}
	
	void OnAction(InputManager.Info data) {
		if(data.state == InputManager.State.Pressed) {
			//do something amazing
		}
	}
}
