using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class StartMenu : MonoBehaviour {
	Toggle _aiModeToggle, _selfModeToggle;

	// Use this for initialization
	void Start () {
		_aiModeToggle = gameObject.transform.Find("Panel/ToggleAIMode").GetComponent<Toggle>();
		_selfModeToggle = gameObject.transform.Find("Panel/ToggleSelfMode").GetComponent<Toggle>();

		MainState.GameMode gameMode = GameObject.Find("MainState").GetComponent<MainState>().gameMode;
		if (gameMode == MainState.GameMode.AI) {
			_aiModeToggle.isOn = true;
		} else if (gameMode == MainState.GameMode.Self) {
			_selfModeToggle.isOn = true;
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void onNewGame() {
		Hashtable parameters = new Hashtable();

		MainState.GameMode gameMode = MainState.GameMode.AI;
		if (_aiModeToggle.isOn) {
			gameMode = MainState.GameMode.AI;
		} else if (_selfModeToggle.isOn) {
			gameMode = MainState.GameMode.Self;
		}

		parameters["gameMode"] = gameMode;

		GameObject.Find("MainState").SendMessage("restart", parameters);
		Destroy(this.gameObject);
	}

	public void onQuit() {
		if (Application.isEditor) {
			onContinue();
			return;
		}

		if (Application.platform == RuntimePlatform.IPhonePlayer) {
			// TODO: show confirm on iphone
			Application.Quit();
		} else {
			Application.Quit();
		}
	}

	public void onContinue() {
		GameObject.Find("MainState").SendMessage("pause", false);
		Destroy(this.gameObject);
	}
}
