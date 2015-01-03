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

	void Update () {

	}

	public override void onShow(bool show) {
		if (show) {
			SoundHub.instance().play("NewAchieve");
		}
	}

	public void hide() {
		ScreenManager.instance().get("GameMainUI").GetComponent<MainState>().pause(false);
		ScreenManager.show(gameObject, false);
	}

	public void onGotoAchievement() {
		if (_displayIndex >= _finishedAchieves.Count) {
			ScreenManager.show(gameObject, false);
			ScreenManager.instance().show("AchievementUI", true, "SlideIn");
			AchievementUI achievementUI = ScreenManager.instance().get("AchievementUI").GetComponent<AchievementUI>();
			achievementUI.fromScreen = "GameMainUI";
			achievementUI.showAchieveInfo(_finishedAchieves[_finishedAchieves.Count - 1]);
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

			Texture2D achieveIconTex = Resources.Load<Texture2D>("texture/achievement0" + (achieveId - 1));
			_achieveIcon.sprite = Sprite.Create(achieveIconTex,
			                                    new Rect(0, 0, achieveIconTex.width, achieveIconTex.height),
			                                    new Vector2(0.5f, 0.5f));

			++_displayIndex;
		}
	}
}
