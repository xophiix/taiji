using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class AchieveDetailUI : ScreenBase {
	public GameObject achieveItemPrefab;
	private List<AchieveUIItem> _achieveUIItems = new List<AchieveUIItem>();

	// Use this for initialization
	void Awake () {
		base.Awake();
		initAchieveUI();
	}

	void OnEnable() {
		refresh();
	}

	public void hide() {
		ScreenManager.show(gameObject, false);
	}

	private void initAchieveUI() {
		Debug.Log("AchieveDetailUI initAchieveUI");

		AchievementConfig config = AchievementConfig.instance();
		int achieveCount = config.getAchieveItemCount();
		Transform achieveItemLayer = gameObject.transform.Find("Panel/AchieveItemLayer");

		for (int i = 0; i < achieveCount; ++i) {
			AchievementConfig.AchieveItem item = config.getAchieveItemByIndex(i);
			bool finished = PlayerPrefs.GetInt("finished_achieve_" + item.id, 0) > 0;
			
			GameObject achieveUIItem = (GameObject)Instantiate(achieveItemPrefab);
			achieveUIItem.transform.SetParent(achieveItemLayer);
			achieveUIItem.transform.localScale = Vector3.one;
			achieveUIItem.name = "AchieveUIItem" + i;
			
			AchieveUIItem uiItem = achieveUIItem.GetComponent<AchieveUIItem>();
			_achieveUIItems.Add(uiItem);
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
		}
	}
}
