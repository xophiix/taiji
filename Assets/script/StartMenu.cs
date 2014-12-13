using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class StartMenu : ScreenBase {
	public GameObject gameMainUIPrefab;

	Toggle _aiModeToggle, _selfModeToggle;

	private void initToggleState() {
		GameObject gameMainUI = ScreenManager.instance().get("GameMainUI");
		MainState mainState = gameMainUI.GetComponent<MainState>();
		Transform resumeButton = gameObject.transform.Find("ButtonLayer/Resume");
		resumeButton.gameObject.SetActive(gameMainUI.activeSelf && mainState.paused());

		_aiModeToggle = gameObject.transform.Find("ToggleAIMode").GetComponent<Toggle>();
		_selfModeToggle = gameObject.transform.Find("ToggleSelfMode").GetComponent<Toggle>();
	
		if (gameMainUI.activeSelf) {
			MainState.GameMode gameMode = mainState.gameMode;
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

		GameObject gameMainUI = ScreenManager.instance().get("GameMainUI");
		ScreenManager.show (gameMainUI, true);
		gameMainUI.GetComponent<MainState>().restart(parameters);

		ScreenManager.show(gameObject, false);
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
		this.gameObject.SetActive(false);
	}

	public void onSetting() {
		ScreenManager.show(gameObject, false);
		ScreenManager.instance().show("SettingUI", true);
	}

	public void onAchievement() {
		ScreenManager.show(gameObject, false);
		ScreenManager.instance().show("AchievementUI", true);
	}

	public void onHighScores() {
		Debug.Log("onHighScores");
	}

	public void onGooglePlus() {
		Debug.Log("onGooglePlus");
	}
}
