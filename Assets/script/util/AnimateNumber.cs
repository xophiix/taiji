using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class AnimateNumber : MonoBehaviour {
	public int initValue;
	public float changeDuration = 1.5f;

	int _destValue;
	int _curValue;
	float _speed;
	bool _updating;
	Text _text;

	void Awake() {
		_text = gameObject.GetComponent<Text>();
		_text.text = initValue.ToString();
	}

	void OnDisable() {
		_updating = false;
		StopCoroutine("updateAnimation");
	}

	void OnEnable() {
		setValue(_destValue, true);
	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

	}

	void updateText() {
		_text.text = _curValue.ToString();
	}

	IEnumerator updateAnimation() {
		_updating = true;
		while (_curValue != _destValue) {
			int deltaValue = Mathf.CeilToInt(Time.deltaTime * _speed);
			if (_curValue < _destValue) {
				_curValue += deltaValue;
				if (_curValue > _destValue) {
					_curValue = _destValue;
				}
			} else {
				_curValue -= deltaValue;
				if (_curValue < _destValue) {
					_curValue = _destValue;
				}
			}

			updateText();
			yield return null;
		}

		updateText();
		_updating = false;
	}

	public void setValue(int value, bool force = false) {
		if (!force && _destValue == value) {
			return;
		}

		_destValue = value;
		if (_destValue == _curValue) {
			StopCoroutine("updateAnimation");
			updateText();
		} else {
			_speed = Mathf.Abs(_curValue - _destValue) / changeDuration;
			if (!_updating) {
				StartCoroutine("updateAnimation");
			}
		}
	}

	public void reset() {
		_curValue = _destValue = 0;
		updateText();
	}
}
