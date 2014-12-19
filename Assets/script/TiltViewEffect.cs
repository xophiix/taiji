using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TiltViewEffect : MonoBehaviour {

	[System.Serializable]
	public class TiltParam {
		[HideInInspector]
		public Vector3 center;
		public float radius;
		[HideInInspector]
		public Vector3 initPos;
		public GameObject child;
		public float offsetScaleX = 1.0f;
		public float offsetScaleY = 1.0f;
	};

	public TiltParam[] tiltParams;

	private bool _touchDown;
	private bool _dragging;
	private Vector3 _lastMousePos = new Vector3();
	// Use this for initialization
	void Awake () {
		foreach (TiltParam param in tiltParams) {
			param.initPos = param.child.transform.localPosition;
		}
	}
	
	// Update is called once per frame
	void Update () {
		Vector3 mousePosition = Input.mousePosition;
		if (_lastMousePos.magnitude == 0) {
			_lastMousePos.Set(mousePosition.x, mousePosition.y, mousePosition.z);
		}

		if (!_dragging && _touchDown) {
			Vector3 delta = mousePosition - _lastMousePos;
			if (Mathf.Abs(delta.x) > 2 || Mathf.Abs(delta.y) > 2) {
				_dragging = true;
			}
		}

		_lastMousePos.Set(mousePosition.x, mousePosition.y, mousePosition.z);

		if (_dragging) {
			Vector3 mouseInWorld = Camera.main.ScreenToWorldPoint(mousePosition);
			Vector3 mouseInParent = gameObject.GetComponent<RectTransform>().InverseTransformPoint(mouseInWorld);
			
			Vector3 scalar = new Vector3();
			foreach (TiltParam param in tiltParams) {
				Vector3 fromCenter = mouseInParent - param.center;
				fromCenter.Normalize();
				scalar.Set(param.radius, param.radius, 1);
				fromCenter.Scale(scalar);
				fromCenter.x *= param.offsetScaleX;
				fromCenter.y *= param.offsetScaleY;
				
				Vector3 tiltedPos = param.initPos + fromCenter;
				tiltedPos.z = 0;
				param.child.transform.localPosition = tiltedPos;
			}
		}

		if (Input.GetMouseButtonDown(0)) {
			Vector3 mouseInWorld = Camera.main.ScreenToWorldPoint(mousePosition);
			Vector3 mouseInParent = gameObject.GetComponent<RectTransform>().InverseTransformPoint(mouseInWorld);

			foreach (TiltParam param in tiltParams) {
				param.center = mouseInParent;
				param.initPos = param.child.transform.localPosition;
			}

			_touchDown = true;
		} else if (Input.GetMouseButtonUp(0)) {
			_touchDown = false;
			_dragging = false;
		}
	}
}
