using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class AchievementUI : ScreenBase {
	public GameObject achieveItemPrefab;

	private List<AchieveUIItem> _achieveUIItems = new List<AchieveUIItem>();
	private List<Image> _achieveIcons = new List<Image>();

	// Use this for initialization
	void Awake() {
		base.Awake();
		initAchieveUI();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void onBack() {
		ScreenManager.show(gameObject, false);
		ScreenManager.instance().show("StartMenu", true);
	}

	private void initAchieveUI() {
		AchievementConfig config = AchievementConfig.instance();
		int achieveCount = config.getAchieveItemCount();
		Transform achieveItemLayer = gameObject.transform.Find("AchieveItemLayer");
		Transform achieveIconLayer = gameObject.transform.Find("AchieveIconLayer");

		for (int i = 0; i < achieveCount; ++i) {
			AchievementConfig.AchieveItem item = config.getAchieveItemByIndex(i);
			bool finished = PlayerPrefs.GetInt("finished_achieve_" + item.id, 0) > 0;

			GameObject achieveUIItem = (GameObject)Instantiate(achieveItemPrefab);
			achieveUIItem.transform.SetParent(achieveItemLayer);
			achieveUIItem.name = "AchieveUIItem" + i;

			AchieveUIItem uiItem = achieveUIItem.GetComponent<AchieveUIItem>();
			_achieveUIItems.Add(uiItem);

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
			AchieveUIItem uiItem = _achieveUIItems[i];
			bool finished = PlayerPrefs.GetInt("finished_achieve_" + item.id, 0) > 0;
			uiItem.setAchieve(item, finished);

			if (i < _achieveIcons.Count) {
				Image icon = _achieveIcons[i];
				icon.enabled = finished;
			}
		}
	}
}
