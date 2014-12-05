using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UIAutoFit : MonoBehaviour {

	private Vector2 referenceResolution;
	private CanvasScaler canvasScaler;
	private float referenceAspect;

	// Use this for initialization
	void Start () {
		canvasScaler = gameObject.GetComponent<CanvasScaler>();
		referenceResolution = canvasScaler.referenceResolution;
		referenceAspect = referenceResolution.x / referenceResolution.y;
	}
	
	// Update is called once per frame
	void Update () {
		if (canvasScaler.uiScaleMode == CanvasScaler.ScaleMode.ScaleWithScreenSize) {
			float curAspect = (float)Screen.width / Screen.height;
			if (curAspect > referenceAspect) {
				canvasScaler.matchWidthOrHeight = 1.0f;
			} else {
				canvasScaler.matchWidthOrHeight = 0.0f;
			}
		}
	}
}
