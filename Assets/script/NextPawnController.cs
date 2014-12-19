using UnityEngine;
using System.Collections;

public class NextPawnController : MonoBehaviour {
	Vector3 _destMovePos = new Vector3();
	Vector3 _startPos = new Vector3();
	Vector3 _movDir;
	bool _reached = true;
	float _speed;
	float _accl = 50f;
	float _destScale = 1;

	// Update is called once per frame
	void Update () {
		if (!_reached) {
			Vector3 position = gameObject.transform.position;
			position += _movDir * Time.deltaTime * _speed;
			_speed += _accl;

			gameObject.transform.position = position;

			float scale = gameObject.transform.localScale.x;
			if (scale < _destScale) {
				scale += 10.0f * Time.deltaTime;
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
		_destScale = 100.0f / 44;
		_destMovePos = destPos;
		_speed = speed;
		_reached = false;

		SoundHub.instance().play("MoveBegin");
	}
}
