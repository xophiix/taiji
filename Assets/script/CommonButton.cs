using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CommonButton : MonoBehaviour {
	public string clickFxName;

	private Button _button;
	public AudioSource clickFx;
	// Use this for initialization
	void Awake () {
		_button = gameObject.GetComponent<Button>();
		_button.onClick.AddListener(onDefaultClick);
	}
	
	void onDefaultClick() {
		if (clickFx == null) {
			clickFx = GameObject.Find(clickFxName).audio;
		}

		clickFx.Play();
	}
	// Update is called once per frame
	void Update () {
	
	}
}
