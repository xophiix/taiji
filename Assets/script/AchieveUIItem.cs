using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class AchieveUIItem : MonoBehaviour {
	public Color finishedColor;
	public Color notFinishedColor;
	public Color defaultColor;

	private Text _label;
	// Use this for initialization
	void Awake() {
		_label = gameObject.GetComponent<Text>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void setAchieve(AchievementConfig.AchieveItem achieveItem, bool finished) {
		_label.text = achieveItem.name.ToUpper();
		Color textColor = finished ? finishedColor : notFinishedColor;
		if (achieveItem.initFinished) {
			textColor = defaultColor;
		}

		_label.color = textColor;
	}
}
