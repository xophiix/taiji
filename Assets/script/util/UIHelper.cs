using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIHelper {
	public static void show(GameObject screen) {
		screen.SetActive(true);
		screen.GetComponent<RectTransform>().localPosition = Vector3.zero;
	}

	public static void hide(GameObject screen) {
		screen.SetActive(false);
	}
}
