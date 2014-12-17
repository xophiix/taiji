using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class AchieveNotifyUI : ScreenBase {

	private List<int> _finishedAchieves = new List<int>();
	private Text _title;
	private Image _achieveIcon;
	private int _displayIndex;

	// Use this for initialization
	void Awake () {
		base.Awake();

		_title = gameObject.transform.Find("Panel/Title").GetComponent<Text>();
		_achieveIcon = gameObject.transform.Find("Panel/AchieveIcon").GetComponent<Image>();
	}
	
	// Update is called once per frame
	void Update () {
	}

	public override void onShow(bool show) {
		if (show) {
			SoundHub.instance().play("NewAchieve");
		}
	}

	public void hide() {
		ScreenManager.show(gameObject, false);
	}

	public void onGotoAchievement() {
		if (_displayIndex >= _finishedAchieves.Count) {
			hide();
			ScreenManager.instance().show("AchievementUI", true);
			ScreenManager.instance().get("AchievementUI").GetComponent<AchievementUI>().fromScreen = "GameMainUI";
		}
	}

	public void setFinishedAchieveIds(List<int> finishedAchieves) {
		_finishedAchieves.Clear();
		_finishedAchieves.AddRange(finishedAchieves);
		_displayIndex = 0;
		showNext();
	}

	private void showNext() {
		if (_displayIndex < _finishedAchieves.Count) {
			int achieveId = _finishedAchieves[_displayIndex];
			AchievementConfig.AchieveItem achieveItem = AchievementConfig.instance().getAchieveConfigItem(achieveId);
			_title.text = achieveItem.name.ToUpper();
			// TODO: set icon dynamically
			++_displayIndex;
		}
	}
}
