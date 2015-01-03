using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class AchievementUI : ScreenBase {
	public string fromScreen;
	private List<Image> _achieveIcons = new List<Image>();

	Text _achieveNameText;
	Text _achieveFinishTimeText;
	Text _achieveInfoText;
	PawnDisplayer _achieveDetailFirstPawn;
	PawnDisplayer _achieveDetailSecondPawn;
	Image _achieveInfoLayer;
	Image _achieveIconGlow;
	CanvasGroup _achieveInfoLayerCanvas;
	int _selectedAchieve = -1;
	float _startShowInfoTime;

	public int achieveInfoShowDuration = 5;
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
		_achieveInfoLayer = gameObject.transform.Find("AchieveInfoLayer").gameObject.GetComponent<Image>();
		_achieveInfoLayerCanvas = _achieveInfoLayer.gameObject.GetComponent<CanvasGroup>();
		_achieveInfoLayerCanvas.alpha = 0;

		_achieveNameText = _achieveInfoLayer.transform.Find("AchieveName").gameObject.GetComponent<Text>();
		_achieveFinishTimeText = _achieveInfoLayer.transform.Find("AchieveFinishTime").gameObject.GetComponent<Text>();
		_achieveInfoText = _achieveInfoLayer.transform.Find("AchieveInfo").gameObject.GetComponent<Text>();
		_achieveDetailFirstPawn = _achieveInfoLayer.transform.Find("AchieveInfo/Pawn1").gameObject.GetComponent<PawnDisplayer>();
		_achieveDetailSecondPawn = _achieveInfoLayer.transform.Find("AchieveInfo/Pawn2").gameObject.GetComponent<PawnDisplayer>();
		_achieveDetailFirstPawn.disableSendMessage = true;
		_achieveDetailSecondPawn.disableSendMessage = true;

		_achieveInfoLayer.gameObject.SetActive(false);

		AchievementConfig config = AchievementConfig.instance();
		int achieveCount = config.getAchieveItemCount();
		Transform achieveIconLayer = gameObject.transform.Find("AchieveIconLayer");
		_achieveIconGlow = achieveIconLayer.Find("IconGlow").GetComponent<Image>();
		_achieveIconGlow.gameObject.SetActive(false);

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

	public void showAchieveInfo(int achieveId) {
		if (_selectedAchieve == achieveId && _achieveInfoLayer.gameObject.activeSelf) {
			return;
		}

		_achieveInfoLayer.gameObject.SetActive(true);

		bool fadingOut = false;
		if (_selectedAchieve > 0 && _selectedAchieve != achieveId) {
			Image lastIcon = _achieveIcons[_selectedAchieve - 1];
			lastIcon.transform.localScale = Vector3.one;

			StopCoroutine("fadeOutAchieveInfoLayer");
			StartCoroutine("fadeOutAchieveInfoLayer", true);
			fadingOut = true;
		}

		_selectedAchieve = achieveId;

		Image icon = _achieveIcons[achieveId - 1];
		_achieveIconGlow.gameObject.SetActive(true);
		_achieveIconGlow.transform.localPosition = icon.gameObject.transform.localPosition;
		// TODO: glow effect

		if (!fadingOut) {
			fillAchieveInfo(achieveId);
			StopCoroutine("fadeInAchieveInfoLayer");
			StartCoroutine("fadeInAchieveInfoLayer");
		}
	}

	void fillAchieveInfo(int achieveId) {
		AchievementConfig config = AchievementConfig.instance();
		AchievementConfig.AchieveItem achieveItem = config.getAchieveConfigItem(achieveId);
		if (achieveItem == null) {
			return;
		}

		_achieveNameText.text = achieveItem.name.ToUpper();
		_achieveInfoText.gameObject.SetActive(!achieveItem.initFinished);
		if (achieveItem.initFinished) {
			_achieveFinishTimeText.text = "\nOh my sweetheart\nThe future's not ours to see";
		} else {
			string detail = "";
			long finishTimeStamp = System.Convert.ToInt64(PlayerPrefs.GetString("achieve_finish_time" + achieveId, "0"));
			
			System.DateTime finishDate = new System.DateTime(finishTimeStamp);
			_achieveFinishTimeText.text = finishDate.ToString("yyyy.MM.dd\nH:mm");

			int firstPawnType = PlayerPrefs.GetInt("achieve_progress" + achieveId + "_pawn_type0");
			int secondPawnType = PlayerPrefs.GetInt("achieve_progress" + achieveId + "_pawn_type1");
			
			int neighborMark = achieveItem.parameters[0];
			_achieveDetailFirstPawn.pawnType = (MainState.PawnType)firstPawnType;
			_achieveDetailFirstPawn.setNeighborCountMark(neighborMark, true);
			_achieveDetailSecondPawn.pawnType = (MainState.PawnType)secondPawnType;
			_achieveDetailSecondPawn.setNeighborCountMark(neighborMark, true);
		}
	}

	IEnumerator fadeInAchieveInfoLayer() {
		while (_achieveInfoLayerCanvas.alpha < 1) {
			_achieveInfoLayerCanvas.alpha += 0.1f;
			yield return null;
		}

		_achieveInfoLayerCanvas.alpha = 1;
		_startShowInfoTime = Time.realtimeSinceStartup;
		while (Time.realtimeSinceStartup - _startShowInfoTime < achieveInfoShowDuration) {
			yield return new WaitForEndOfFrame();
		}

		StopCoroutine("fadeOutAchieveInfoLayer");
		StartCoroutine("fadeOutAchieveInfoLayer", false);
	}

	IEnumerator fadeOutAchieveInfoLayer(bool needFadeInNew) {
		while (_achieveInfoLayerCanvas.alpha > 0) {
			_achieveInfoLayerCanvas.alpha -= needFadeInNew ? 0.1f : 0.03f;
			yield return null;
		}
		
		_achieveInfoLayerCanvas.alpha = 0;

		if (needFadeInNew) {
			fillAchieveInfo(_selectedAchieve);
			StopCoroutine("fadeInAchieveInfoLayer");
			StartCoroutine("fadeInAchieveInfoLayer");
		} else {
			_achieveIconGlow.gameObject.SetActive(false);
			_selectedAchieve = -1;
			_achieveInfoLayer.gameObject.SetActive(false);
		}
	}
}
