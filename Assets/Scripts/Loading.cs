using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Loading : MonoBehaviour {
	

	// Use this for initialization
	void Start () {
		Cursor.visible = false;
		// Screen.SetResolution(854, 480, false);
		Screen.fullScreen = true;
		SceneManager.LoadSceneAsync (1);
	}

	void Update() {
	}


}
