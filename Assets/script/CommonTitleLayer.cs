using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CommonTitleLayer : MonoBehaviour {
	public string title = "";
	private Text _titleText;

	void Awake() {
		_titleText = gameObject.transform.Find("Title").GetComponent<Text>();
		_titleText.text = title;
	}

	public void setTitle(string value) {
		title = value;
		_titleText.text = title;
	}
}
