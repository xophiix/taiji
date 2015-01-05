using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ExpBar : MonoBehaviour {
	struct FrameKey {
		public float value;
	}

	List<FrameKey> _keys = new List<FrameKey>();
	int _curKeyIndex;

	float _curRatio;
	float _orgWidth;
	float _speed;

	public float durationFromStartToEnd = 1.5f;

	RectTransform _rectTransform;
	void Awake() {
		_curRatio = 0;
		_rectTransform = gameObject.GetComponent<RectTransform>();
		_orgWidth = _rectTransform.rect.width;
		_speed = 1.0f / durationFromStartToEnd;
	}

	// Use this for initialization
	void Start () {
	
	}

	// Update is called once per frame
	void Update () {
	}

	IEnumerator startAnimation() {
		while (_curKeyIndex < _keys.Count) {
			FrameKey curKey = _keys[_curKeyIndex];
			if (_curRatio >= curKey.value) {
				++_curKeyIndex;
			}

			_curRatio += Time.deltaTime * _speed;
			updateWidth();
			yield return null;
		}

		_curRatio = _keys[_keys.Count - 1].value;
		_keys.Clear();
		_curKeyIndex = 0;
		updateWidth();
	}

	void updateWidth() {
		float fractPartOfRatio = _curRatio - Mathf.Floor(_curRatio);
		float width = Mathf.Max(0.0f, fractPartOfRatio * _orgWidth);
		_rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
	}

	public void setExpRatio(float targetRatio) {
		targetRatio = Mathf.Clamp(targetRatio, 0.0f, 1.0f);

		float lastRatio = _keys.Count == 0 ? _curRatio : _keys[_keys.Count - 1].value;

		float intPartOfRatio = Mathf.Floor(lastRatio);
		float fractPartOfRatio = lastRatio - intPartOfRatio;

		FrameKey key = new FrameKey();
		if (targetRatio < fractPartOfRatio) {
			key.value = intPartOfRatio + 1.0f + targetRatio;
		} else {
			key.value = intPartOfRatio + targetRatio;
		}

		int preKeyCount = _keys.Count;
		_keys.Add(key);

		if (0 == preKeyCount) {
			StartCoroutine("startAnimation");
		}
	}

	public void reset() {
		_curRatio = 0;
		_keys.Clear();
		_curKeyIndex = 0;
		StopCoroutine("startAnimation");
		updateWidth();
	}
}
