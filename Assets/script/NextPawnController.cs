using UnityEngine;
using System.Collections;

public class NextPawnController : MonoBehaviour {
	public GameObject PawnPrefab;

	Vector3 _destMovePos = new Vector3();
	Vector3 _startPos = new Vector3();
	Vector3 _movDir;
	bool _reached = true;
	float _speed;
	float _accl = 50f;
	float _scaleAccl = 1;
	float _destScale = 1;
	float _scaleSpeed = 1;

	void Awake() {
		Rect pawnOnBoardRect = PawnPrefab.GetComponent<RectTransform>().rect;
		Rect thisRect = gameObject.GetComponent<RectTransform>().rect;
		_destScale = pawnOnBoardRect.width / thisRect.width;
	}

	// Update is called once per frame
	void Update () {
		if (!_reached) {
			Vector3 position = gameObject.transform.position;
			position += _movDir * Time.deltaTime * _speed;
			_speed += _accl;

			gameObject.transform.position = position;

			float scale = gameObject.transform.localScale.x;
			if (scale < _destScale) {
				scale += _scaleSpeed * Time.deltaTime;
				_scaleSpeed += _scaleAccl;
				gameObject.transform.localScale = new Vector3(scale, scale, 1);
			} 

			if (scale > _destScale) {
				gameObject.transform.localScale = new Vector3(_destScale, _destScale, 1);
			}

			Vector3 errorness = _destMovePos - position;
			if (Vector3.Dot(errorness, _movDir) < 0 || errorness.magnitude < 5) {
				_reached = true;
				gameObject.transform.position = _destMovePos;
				gameObject.transform.parent.SendMessage("onMovingDone", this);
				SoundHub.instance().play("MoveEnd");
			}
		}	
	}

	public void moveTo(Vector3 destPos, float speed) {
		_startPos = gameObject.transform.position;
		_movDir = destPos - gameObject.transform.position;
		_movDir.Normalize();
		_destMovePos = destPos;
		_speed = speed;
		_reached = false;

		float estimateTime = Vector3.Distance(_destMovePos, _startPos) / speed;
		float curScale = gameObject.transform.localScale.x;
		_scaleSpeed = (_destScale - curScale) / estimateTime;
		_scaleAccl = _scaleSpeed * _accl / _speed;

		SoundHub.instance().play("MoveBegin");
	}
}
