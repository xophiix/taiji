using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameInit : MonoBehaviour {
	private static GameInit _instance;
	private HashSet<int> _finishedAchieves = new HashSet<int>();
	public static GameInit instance() {
		return _instance;
	}

	// Use this for initialization
	void Awake() {
		_instance = this;
		initPlayerDefaults();
		ScreenManager.instance().show("StartMenu", true);
	}

	void initPlayerDefaults() {
		if (Application.isEditor) {
			PlayerPrefs.DeleteAll();
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
}
