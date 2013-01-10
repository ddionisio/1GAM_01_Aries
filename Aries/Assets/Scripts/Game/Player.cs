using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {

	// Use this for initialization
	void Start () {
		CameraController camCtrl = CameraController.instance;
		
		camCtrl.attachTo = transform;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
