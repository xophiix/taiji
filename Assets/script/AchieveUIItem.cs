using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class AchieveUIItem : MonoBehaviour {
	private Text _nameLabel;
	private Toggle _toggle;

	// Use this for initialization
	void Awake() {
		_nameLabel = gameObject.transform.Find("Label").GetComponent<Text>();
		_toggle = gameObject.GetComponent<Toggle>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void setAchieve(AchievementConfig.AchieveItem achieveItem, bool finished) {
		_nameLabel.text = achieveItem.name.ToUpper();
		_toggle.isOn = finished;
	}
}
