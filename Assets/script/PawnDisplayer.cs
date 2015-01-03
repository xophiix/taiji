using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PawnDisplayer : MonoBehaviour {
	public float alphaChangeSpeed = 0.03f;

	private Transform _mark;
	private Image _markImage;

	private float _destAlpha;
	private float _curAlpha;
	private bool _alphaReached = true;

	private float _curMarkAngle = Mathf.PI / 2;
	private float _destMarkAngle;
	private bool _markAngleReached = true;
	private float _curAngleSpeed;

	private int _neighborCount;

	float MARK_DISTANCE;
	const float MARK_RADIAN_UNIT = Mathf.PI / 4;

	public PawnType pawnType = PawnType.Black;
	public Sprite whitePawn;
	public Sprite whitePawnMark;
	public Sprite blackPawn;
	public Sprite blackPawnMark;

	public bool disableSendMessage {
		get; set;
	}

	void Awake() {
		_destMarkAngle = _curMarkAngle;
		_mark = gameObject.transform.Find("Mark");
		_markImage = _mark.GetComponent<Image>();
		disableSendMessage = false;

		Color color = _markImage.color;
		color.a = _curAlpha;
		_markImage.color = color;
		MARK_DISTANCE = _mark.transform.localPosition.y;
	}

	void Start() {
		Sprite pawnImage = blackPawn;
		Sprite pawnMarkImage = whitePawnMark;
		if (pawnType == PawnType.White) {
			pawnImage = whitePawn;
			pawnMarkImage = blackPawnMark;
		}

		gameObject.GetComponent<Image>().sprite = pawnImage;
		_mark.GetComponent<Image>().sprite = pawnMarkImage;
	}

	void eliminate() {
		StartCoroutine("updateElimateAnim");
	}

	void Update() {
		bool preAlphaReached = _alphaReached;
		bool preMarkAngleReached = _markAngleReached;

		bool angleChanged = false;
		bool markAngleReached = false;

		if (!_markAngleReached) {
			if (_curMarkAngle < _destMarkAngle) {
				_curMarkAngle += _curAngleSpeed * Time.deltaTime;
				if (_curMarkAngle > _destMarkAngle) {
					_curMarkAngle = _destMarkAngle;
				}
				angleChanged = true;
			} else if (_curMarkAngle > _destMarkAngle) {
				_curMarkAngle -= _curAngleSpeed * Time.deltaTime;
				if (_curMarkAngle < _destMarkAngle) {
					_curMarkAngle = _destMarkAngle;
				}
				angleChanged = true;
			}
			
			if (angleChanged) {
				Vector2 markPos = new Vector2();
				markPos.x = MARK_DISTANCE * Mathf.Cos(_curMarkAngle);
				markPos.y = MARK_DISTANCE * Mathf.Sin(_curMarkAngle);
				_mark.transform.localPosition = new Vector3(markPos.x, markPos.y, 0);
				if (_curMarkAngle == _destMarkAngle) {
					_markAngleReached = true;
				}
			}
		}


		bool alphaChanged = false;
		if (!_alphaReached) {
			if (_curAlpha < _destAlpha) {
				_curAlpha += alphaChangeSpeed;
				if (_curAlpha > _destAlpha) {
					_curAlpha = _destAlpha;
				}
				alphaChanged = true;
			} else if (_curAlpha > _destAlpha) {
				_curAlpha -= alphaChangeSpeed;
				if (_curAlpha < _destAlpha) {
					_curAlpha = _destAlpha;
				}
				alphaChanged = true;
			}
			
			if (alphaChanged) {
				Color color = _markImage.color;
				color.a = _curAlpha;
				_markImage.color = color;
			}

			if (_curAlpha == _destAlpha) {
				_alphaReached = true;
			}
		}

		if ((!preAlphaReached || !preMarkAngleReached) && _alphaReached && _markAngleReached) {
			if (!disableSendMessage) {
				gameObject.SendMessageUpwards("onPawnNeighborMarkUpdateDone", this);
			}
		}
	}

	IEnumerator updateElimateAnim() {
		float scale = this.transform.localScale.x;
		while (scale > 0) {
			this.transform.localScale = new Vector3(scale, scale, 1);
			scale -= 0.1f;
			yield return null;
		}

		Destroy(this.gameObject);
	}

	public void setNeighborCountMark(int neighborCount, bool force = false) {
		if (!force && _neighborCount == neighborCount) {
			return;
		}

		_neighborCount = neighborCount;
		_destAlpha = neighborCount > 0 ? 1.0f : 0.0f;
		if (neighborCount > 0) {
			_destMarkAngle = Mathf.PI / 2 - MARK_RADIAN_UNIT * (neighborCount - 1);
		}

		if (_destMarkAngle != _curMarkAngle) {
			_curAngleSpeed = Mathf.Abs(_destMarkAngle - _curMarkAngle) / 0.6f;
		}

		_alphaReached = _curAlpha == _destAlpha;
		_markAngleReached = _curMarkAngle == _destMarkAngle;
	}

	public void select(bool value) {
		transform.localScale = value ? new Vector3(1.2f, 1.2f, 1f) : Vector3.one;
		if (value) {
			transform.SetAsLastSibling();
		}
	}
}
	