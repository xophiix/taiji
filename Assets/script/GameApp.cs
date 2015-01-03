using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameApp : MonoBehaviour {
	private static GameApp _instance;
	private HashSet<int> _finishedAchieves = new HashSet<int>();
	public static GameApp instance() {
		return _instance;
	}

	// Use this for initialization
	void Awake() {
		_instance = this;
		initPlayerDefaults();
	}

	void Start() {
		string saveDataFile = gameTempSavePath;
		if (System.IO.File.Exists(saveDataFile)) {
			// load game save and goto game main ui directly
			GameObject gameMainUI = ScreenManager.instance().get("GameMainUI");
			MainState mainState = gameMainUI.GetComponent<MainState>();
			try {
				GameSaveData saveData = (GameSaveData)PlatformUtils.readObject(saveDataFile, typeof(GameSaveData));
				ScreenManager.show(gameMainUI, true);
				mainState.restart();
				mainState.loadGame(saveData);

				clearSave();
				return;
			} catch (System.Exception e) {
				if (Application.isEditor) {
					throw e;
				}

				Debug.Log(e.ToString());
				ScreenManager.show(gameMainUI, false);
			} finally {
				System.IO.File.Delete(saveDataFile);
			}
		}

		ScreenManager.instance().show("StartMenu", true);
	}

	void initPlayerDefaults() {
		if (Application.isEditor) {
			PlayerPrefs.DeleteAll();

			/*long now = System.DateTime.Now.Ticks;
			PlayerPrefs.SetString("achieve_finish_time1", now.ToString());
			PlayerPrefs.SetString("achieve_finish_time2", now.ToString());
			PlayerPrefs.SetInt("achieve_progress" + 1 + "_pawn_type0", 0);
			PlayerPrefs.SetInt("achieve_progress" + 1 + "_pawn_type1", 1);
			PlayerPrefs.SetInt("achieve_progress" + 2 + "_pawn_type0", 1);
			PlayerPrefs.SetInt("achieve_progress" + 2 + "_pawn_type1", 0);
			PlayerPrefs.SetInt("achieve_progress" + 1, 3);
			PlayerPrefs.SetInt("achieve_progress" + 2, 3);

			PlayerPrefs.SetInt("achieve_progress" + 3, 1);
			PlayerPrefs.SetInt("achieve_progress" + 3 + "_pawn_type0", 0);

			PlayerPrefs.SetInt("finished_achieve_" + 1, 1);
			PlayerPrefs.SetInt("finished_achieve_" + 2, 1);*/

			PlayerPrefs.Save();
		}

		if (0 == PlayerPrefs.GetInt("first_create", 0)) {
			PlayerPrefs.SetInt("first_create", 1);
			PlayerPrefs.SetInt("finished_achieve_" + 7, 1);
			PlayerPrefs.SetInt("finished_achieve_" + 8, 1);
			PlayerPrefs.Save();
		}

		AchievementConfig config = AchievementConfig.instance();
		int achieveCount = config.getAchieveItemCount();

		for (int i = 0; i < achieveCount; ++i) {
			AchievementConfig.AchieveItem item = config.getAchieveItemByIndex(i);
			bool finished = PlayerPrefs.GetInt("finished_achieve_" + item.id, 0) > 0;
			if (finished) {
				_finishedAchieves.Add(item.id);
			}
		}
	}

	public HashSet<int> finishedAchieves {
		get { 
			return _finishedAchieves;
		}
	}

	public void saveAchieveChange() {
		foreach (int id in _finishedAchieves) {
			PlayerPrefs.SetInt("finished_achieve_" + id, 1);
		}

		PlayerPrefs.Save();
	}

	void Update() {
		if (Input.GetKeyDown(KeyCode.Escape)) {
			quit();
		}
	}

	void OnApplicationQuit() {
		Debug.Log("on application quit");
		if (!Application.isMobilePlatform) {
			saveGame();
		}
	}

	void OnApplicationPause() {
		Debug.Log("on application pause");
		if (Application.isMobilePlatform) {
			saveGame();
		}
	}

	void OnApplicationFocus(bool focusStatus) {
		Debug.Log("on application focus " + focusStatus);
	}

	public static void saveGame() {
		GameObject gameMainUI = ScreenManager.instance().get("GameMainUI");
		if (gameMainUI != null) {
			MainState mainState = gameMainUI.GetComponent<MainState>();
			if (gameMainUI.activeSelf || mainState.paused()) {
				GameSaveData saveData = mainState.saveGame();
				Debug.Log("save game " + saveData);
				PlatformUtils.writeObject(gameTempSavePath, saveData);
			}
		}
	}

	public static void clearSave() {
		System.IO.File.Delete(gameTempSavePath);
	}

	public static string gameTempSavePath {
		get {
			return Application.persistentDataPath + "/game_temp_save";
		}
	}

	public static void quit() {
		if (Application.isEditor) {
			return;
		}

		ScreenManager.instance().show("ExitConfirm", true);
	}
}
