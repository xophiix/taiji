using UnityEngine;
using System.Collections;

public class Pawn : MonoBehaviour {
	private int _neighborCount;
	private int _destNeighborCount;

	private float _curMarkAngle = Mathf.PI / 2;
	private float _destMarkAngle;
	public float rotateAngleSpeed = 0.03f;
	public float alphaChangeSpeed = 0.03f;

	private Transform _mark;
	private float _destAlpha;
	private float _curAlpha;
	
	float MARK_DISTANCE;
	const float MARK_RADIAN_UNIT = Mathf.PI / 4;

	void Awake() {
		_destMarkAngle = _curMarkAngle;
		_mark = gameObject.transform.Find("mark");

		Color color = _mark.renderer.material.color;
		color.a = _curAlpha;
		_mark.renderer.material.color = color;
		MARK_DISTANCE = _mark.transform.localPosition.y;
	}

	void eliminate() {
		StartCoroutine("updateElimateAnim");
	}

	void FixedUpdate() {
		bool angleChanged = false;
		if (_curMarkAngle < _destMarkAngle) {
			_curMarkAngle += rotateAngleSpeed;
			if (_curMarkAngle > _destMarkAngle) {
				_curMarkAngle = _destMarkAngle;
			}
			angleChanged = true;
		} else if (_curMarkAngle > _destMarkAngle) {
			_curMarkAngle -= rotateAngleSpeed;
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
		}

		bool alphaChanged = false;
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
			Color color = _mark.renderer.material.color;
			color.a = _curAlpha;
			_mark.renderer.material.color = color;
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

	public void setNeighborCountMark(int neighborCount) {
		_destAlpha = neighborCount > 0 ? 1.0f : 0.0f;
		if (neighborCount > 0) {
			_destMarkAngle = Mathf.PI / 2 - MARK_RADIAN_UNIT * (neighborCount - 1);
		}
	}
}
	