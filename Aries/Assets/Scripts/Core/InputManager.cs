using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//generalized input handling, useful for porting to non-unity conventions (e.g. Ouya)
public class InputManager : MonoBehaviour {
	public delegate void OnButton(Info data);
	
	public enum State {
		None,
		Pressed,
		Released
	}
	
	public enum Control {
		Button,
		Axis
	}
	
	public enum ButtonAxis {
		None,
		Plus,
		Minus
	}
	
	public struct Info {
		public State state;
		public float axis;
		public int index;
	}
	
	public class Key {
		public string input = ""; //for use with unity's input
		public KeyCode code = KeyCode.None; //unity
		public InputKeyMap map = InputKeyMap.None; //for external (like ouya!)
		public ButtonAxis axis = ButtonAxis.None; //for buttons as axis
		public int index = 0; //which index this key refers to
		
		public float GetAxisValue() {
			float ret = 0.0f;
			
			switch(axis) {
			case ButtonAxis.Plus:
				ret = 1.0f;
				break;
			case ButtonAxis.Minus:
				ret = -1.0f;
				break;
			}
			
			return ret;
		}
	}
	
	public class Bind {
		public InputAction action = (InputAction)0;
		public Control control = InputManager.Control.Button;
		
		public Key[] keys;
	}
	
	public TextAsset config;
	
	private class BindData {
		public Info info;
		
		public Control control;
		
		public Key[] keys;
		
		public event OnButton callback;
		
		public BindData(Bind bind) {
			control = bind.control;
			keys = bind.keys;
		}
		
		public void ClearCallback() {
			callback = null;
		}
		
		public void Call() {
			if(callback != null) {
				callback(info);
			}
		}
	}
	
	private BindData[] mBinds = new BindData[(int)InputAction.NumAction];
	
	//interfaces (available after awake)
	
	public bool CheckBind(InputAction action) {
		return mBinds[(int)action] != null;
	}
			
	public float GetAxis(InputAction action) {
		return mBinds[(int)action].info.axis;
	}
	
	public State GetState(InputAction action) {
		return mBinds[(int)action].info.state;
	}
	
	public bool IsDown(InputAction action) {
		bool ret = false;
		
		foreach(Key key in mBinds[(int)action].keys) {
			if(ProcessButtonDown(key)) {
				ret = true;
				break;
			}
		}
		
		return ret;
	}
	
	public int GetIndex(InputAction action) {
		return mBinds[(int)action].info.index;
	}
	
	public void AddButtonCall(InputAction action, OnButton callback) {
		mBinds[(int)action].callback += callback;
	}
	
	public void RemoveButtonCall(InputAction action, OnButton callback) {
		mBinds[(int)action].callback -= callback;
	}
	
	public void ClearButtonCall(InputAction action) {
		mBinds[(int)action].ClearCallback();
	}
	
	public void ClearAllButtonCalls() {
		for(int i = 0; i < (int)InputAction.NumAction; i++) {
			mBinds[i].ClearCallback();
		}
	}
	
	//implements
	
	protected virtual float ProcessAxis(Key key) {
		if(key.input.Length > 0) {
			return Input.GetAxis(key.input);
		}
		else if(key.code != KeyCode.None) {
			if(Input.GetKey(key.code)) {
				return key.GetAxisValue();
			}
		}
		
		return 0.0f;
	}
	
	protected virtual State ProcessButton(Key key) {
		State ret = State.None;
		
		if(key.input.Length > 0) {
			if(Input.GetButtonDown(key.input)) {
				ret = State.Pressed;
			}
			else if(Input.GetButtonUp(key.input)) {
				ret = State.Released;
			}
		}
		else if(key.code != KeyCode.None) {
			if(Input.GetKeyDown(key.code)) {
				ret = State.Pressed;
			}
			else if(Input.GetKeyUp(key.code)) {
				ret = State.Released;
			}
		}
		
		return ret;
	}
	
	protected virtual bool ProcessButtonDown(Key key) {
		return 
			key.input.Length > 0 ? Input.GetButton(key.input) :
			key.code != KeyCode.None ? Input.GetKey(key.code) :
			false;
	}
	
	//internal
	
	void OnDestroy() {
		foreach(BindData bind in mBinds) {
			if(bind != null) {
				bind.ClearCallback();
			}
		}
	}
	
	void Awake() {
		if(config != null) {
			fastJSON.JSON.Instance.Parameters.UseExtensions = false;
			List<Bind> keys = fastJSON.JSON.Instance.ToObject<List<Bind>>(config.text);
			
			foreach(Bind key in keys) {
				mBinds[(int)key.action] = new BindData(key);
			}
		}
	}
			
	void Update() {
		foreach(BindData bindData in mBinds) {
			if(bindData != null && bindData.keys != null) {
				switch(bindData.control) {
				case Control.Axis:
					bindData.info.axis = 0.0f;
					
					foreach(Key key in bindData.keys) {
						float axis = ProcessAxis(key);
						if(axis != 0.0f) {
							bindData.info.axis = axis;
							break;
						}
					}
					break;
					
				case Control.Button:
					bindData.info.state = State.None;
					
					foreach(Key key in bindData.keys) {
						State state = ProcessButton(key);
						if(state != State.None) {
							bindData.info.axis = state == State.Pressed ? key.GetAxisValue() : 0.0f;
							bindData.info.state = state;
							bindData.info.index = key.index;
							
							bindData.Call();
							break;
						}
					}
					break;
				}
			}
		}
	}
}
