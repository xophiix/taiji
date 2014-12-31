using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class FloatTip : MonoBehaviour {
	public float fadeInDuration = 0.4f;
	public float keepDuration = 1.5f;
	public float fadeOutDuration = 0.4f;

	public static FloatTip instance {
		get {
			return _instance;
		}
	}

	static FloatTip _instance;

	enum FadeStage {
		IN,
		KEEP,
		OUT
	};

	class DisplayItem {
		public string text;
		public float elapsedOnKeep;
		public float alpha;
		public FadeStage stage = FadeStage.IN;

		public DisplayItem(string txt) {
			this.text = txt;
		}
	};

	List<DisplayItem> _displayItems = new List<DisplayItem>();
	int _curDisplayIndex = 0;

	Image _tipContainer;
	Text _tipText;
	CanvasGroup _tipContainerCanvas;

	void Awake() {
		_instance = this;
		_tipContainerCanvas = gameObject.transform.Find("TipContainer").GetComponent<CanvasGroup>();
		_tipContainerCanvas.interactable = false;
		_tipContainerCanvas.blocksRaycasts = false;

		_tipContainer = gameObject.transform.Find("TipContainer").GetComponent<Image>();
		_tipText = gameObject.transform.Find("TipContainer/Tip").GetComponent<Text>();

		gameObject.transform.localPosition = Vector3.zero;
		gameObject.SetActive(false);
		_tipContainerCanvas.alpha = 0;
	}

	void Update () {
		if (0 == _displayItems.Count) {
			return;
		}

		float deltaTime = Time.deltaTime;
		DisplayItem item = _displayItems[_curDisplayIndex];
		
		if (item.stage == FadeStage.IN) {
			item.alpha += 1.0f / fadeInDuration * deltaTime;
			if (item.alpha >= 1.0f) {
				item.alpha = 1.0f;
				item.stage = FadeStage.KEEP;
			}
		} else if (item.stage == FadeStage.KEEP) {
			item.elapsedOnKeep += deltaTime;
			if (item.elapsedOnKeep >= keepDuration) {
				item.stage = FadeStage.OUT;
			}
		} else if (item.stage == FadeStage.OUT) {
			item.alpha -= 1.0f / fadeOutDuration * deltaTime;
			if (item.alpha < 0.0f) {
				item.alpha = 0;
				_curDisplayIndex++;
			}
		}

		Debug.Log("tip alpha " + item.alpha);

		_tipText.text = item.text;
		Color c = _tipText.color;
		c.a = item.alpha;
		_tipText.color = c;

		if (_curDisplayIndex >= _displayItems.Count) {
			_displayItems.Clear();
			StartCoroutine("fadeOutContainer");
		}
	}

	public void addTip(string tip) {
		DisplayItem item = new DisplayItem(tip);
		_displayItems.Add(item);

		if (_curDisplayIndex >= _displayItems.Count) {
			_curDisplayIndex = 0;
		}

		if (!gameObject.activeSelf) {
			gameObject.SetActive(true);
			gameObject.transform.SetAsLastSibling();

			StopCoroutine("fadeInContainer");
			StartCoroutine("fadeInContainer");
		} else {
			StopCoroutine("fadeOutContainer");
			StopCoroutine("fadeInContainer");
			StartCoroutine("fadeInContainer");
		}
	}

	IEnumerator fadeInContainer() {
		while (_tipContainerCanvas.alpha < 1.0f) {
			_tipContainerCanvas.alpha += 0.03f;
			yield return null;
		}

		_tipContainerCanvas.alpha = 1.0f;
	}

	IEnumerator fadeOutContainer() {
		while (_tipContainerCanvas.alpha > 0.0f) {
			_tipContainerCanvas.alpha -= 0.03f;
			yield return null;
		}

		_tipContainerCanvas.alpha = 0;
		gameObject.SetActive(false);
	}
}
