using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class AchievementUI : ScreenBase {
	public string fromScreen;
	private List<Image> _achieveIcons = new List<Image>();

	// Use this for initialization
	void Awake() {
		base.Awake();
		initAchieveUI();
	}

	void OnEnable() {
		refresh();
	}

	override public void onShow(bool show) {
		if (show) {
			refresh();
		}
	}

	public void onTapScreen() {
	}

	public void onBack() {
		string screen = fromScreen;
		if (screen == "") {
			screen = "StartMenu";
		}

		string closeAnim = "";
		string openAnim = "";
		if (screen == "StartMenu") {
			closeAnim = "SlideOutReverse";
			openAnim = "SlideInReverse";
		} else {
			closeAnim = "SlideOut";
			openAnim = "SlideIn";
		}

		ScreenManager.show(gameObject, false, closeAnim);
		ScreenManager.instance().show(screen, true, openAnim);
	}

	private void initAchieveUI() {
		AchievementConfig config = AchievementConfig.instance();
		int achieveCount = config.getAchieveItemCount();
		Transform achieveIconLayer = gameObject.transform.Find("AchieveIconLayer");

		for (int i = 0; i < achieveCount; ++i) {
			AchievementConfig.AchieveItem item = config.getAchieveItemByIndex(i);
			bool finished = PlayerPrefs.GetInt("finished_achieve_" + item.id, 0) > 0;

			Transform achieveIcon = achieveIconLayer.Find("Achieve" + i);
			if (achieveIcon) {
				Image icon = achieveIcon.gameObject.GetComponent<Image>();
				_achieveIcons.Add(icon);
			}
		}

		refresh();
	}

	public void refresh() {
		AchievementConfig config = AchievementConfig.instance();
		int achieveCount = config.getAchieveItemCount();

		for (int i = 0; i < achieveCount; ++i) {
			AchievementConfig.AchieveItem item = config.getAchieveItemByIndex(i);
			bool finished = PlayerPrefs.GetInt("finished_achieve_" + item.id, 0) > 0;
			if (i < _achieveIcons.Count) {
				Image icon = _achieveIcons[i];
				icon.enabled = finished;
			}
		}
	}

	public void showDetail() {
		ScreenManager.instance().show("AchieveDetailUI", true);
	}
}
