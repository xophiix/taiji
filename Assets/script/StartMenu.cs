﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class StartMenu : MonoBehaviour {
	public GameObject gameMainUIPrefab;

	Toggle _aiModeToggle, _selfModeToggle;

	private void initToggleState() {
		GameObject gameMainUI = GameObject.Find("GameMainUI");
		Transform resumeButton = gameObject.transform.Find("Panel/ButtonLayer/Resume");
		resumeButton.gameObject.SetActive(gameMainUI != null);

		_aiModeToggle = gameObject.transform.Find("Panel/ToggleAIMode").GetComponent<Toggle>();
		_selfModeToggle = gameObject.transform.Find("Panel/ToggleSelfMode").GetComponent<Toggle>();
	
		if (gameMainUI != null) {
			MainState.GameMode gameMode = gameMainUI.GetComponent<MainState>().gameMode;
			if (gameMode == MainState.GameMode.AI) {
				_aiModeToggle.isOn = true;
			} else if (gameMode == MainState.GameMode.Self) {
				_selfModeToggle.isOn = true;
			}
		}
	}

	// Use this for initialization
	void Start() {
		initToggleState();
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

		GameObject gameMainUI = GameObject.Find("GameMainUI");
		if (gameMainUI == null) {
			gameMainUI = (GameObject)Instantiate(gameMainUIPrefab);
			gameMainUI.name = "GameMainUI";
		}

		gameMainUI.GetComponent<MainState>().restart(parameters);
		Destroy(this.gameObject);
	}

	public void onQuit() {
		if (Application.isEditor) {
			onResume();
			return;
		}

		if (Application.platform == RuntimePlatform.IPhonePlayer) {
			// TODO: show confirm on iphone
			Application.Quit();
		} else {
			Application.Quit();
		}
	}

	public void onResume() {
		GameObject gameMainUI = GameObject.Find("GameMainUI");
		if (gameMainUI == null) {
			return;
		}

		gameMainUI.GetComponent<MainState>().pause(false);
		Destroy(this.gameObject);
	}

	public void onSetting() {
		Debug.Log("onSetting");
	}

	public void onAchievement() {
		Debug.Log("onAchievement");
	}

	public void onHighScores() {
		Debug.Log("onHighScores");
	}

	public void onGooglePlus() {
		Debug.Log("onGooglePlus");
	}
}
