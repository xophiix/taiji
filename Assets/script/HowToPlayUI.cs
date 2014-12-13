using UnityEngine;
using System.Collections;

public class HowToPlayUI : ScreenBase {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void onBack() {
		ScreenManager.show(gameObject, false);
		ScreenManager.instance().show("SettingUI", true);
	}
}
