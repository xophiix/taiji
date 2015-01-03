using UnityEngine;
using System.Collections;

public class LogoScene : MonoBehaviour {

	void Awake() {
		// check game save data
		if (System.IO.File.Exists(GameApp.gameTempSavePath)) {
			Application.LoadLevel(1);
		}
	}

	// Use this for initialization
	void Start() {
		ScreenManager.instance().show("TaijiLogo", true);
	}

	// Update is called once per frame
	void Update () {
	
	}
}
