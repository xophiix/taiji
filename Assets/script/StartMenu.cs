using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class StartMenu : ScreenBase {
	private void updateToggleState() {
		GameObject gameMainUI = ScreenManager.instance().get("GameMainUI");
		bool showResume = false;

		if (gameMainUI != null) {
			MainState mainState = gameMainUI.GetComponent<MainState>();
			showResume = mainState != null && mainState.paused();
		}

		Transform resumeButton = gameObject.transform.Find("ButtonLayer/Resume");
		resumeButton.gameObject.SetActive(showResume);
	}

	// Use this for initialization
	void Start() {
		updateToggleState();
	}

	void OnEnable() {
		updateToggleState();
	}

	void Update () {
	}

	public void onNewGame() {
		ScreenManager.show(gameObject, false, "", true);

		Hashtable parameters = new Hashtable();

		MainState.GameMode gameMode = MainState.GameMode.AI;
		parameters["gameMode"] = gameMode;

		GameObject gameMainUI = ScreenManager.instance().get("GameMainUI");
		ScreenManager.show(gameMainUI, true);
		gameMainUI.GetComponent<MainState>().restart(parameters);
		GameApp.clearSave();
	}

	public void onResume() {
		ScreenManager.show(gameObject, false, "", true);
		GameObject gameMainUI = ScreenManager.instance().get("GameMainUI");
		ScreenManager.show(gameMainUI, true);
		gameMainUI.GetComponent<MainState>().pause(false);
	}

	public void onSetting() {
		ScreenManager.show(gameObject, false, "SlideOut");
		ScreenManager.instance().show("SettingUI", true);
	}

	public void onAchievement() {
		ScreenManager.show(gameObject, false, "SlideOut");
		ScreenManager.instance().show("AchievementUI", true);
	}

	public void onHighScores() {
		Debug.Log("onHighScores");
	}

	public void onGooglePlus() {
		Debug.Log("onGooglePlus");
	}
}
