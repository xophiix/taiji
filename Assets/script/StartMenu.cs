using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class StartMenu : ScreenBase {
	private void updateToggleState() {
		GameObject gameMainUI = ScreenManager.instance().get("GameMainUI");
		MainState mainState = gameMainUI.GetComponent<MainState>();
		Transform resumeButton = gameObject.transform.Find("ButtonLayer/Resume");
		resumeButton.gameObject.SetActive(mainState.paused());
	}

	// Use this for initialization
	void Start() {
		updateToggleState();
	}

	void OnEnable() {
		updateToggleState();
	}

	// Update is called once per frame
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
