using UnityEngine;
using System.Collections;

public class SettingUI : ScreenBase {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void onBack() {
		ScreenManager.show(gameObject, false);
		ScreenManager.instance().show("StartMenu", true);
	}

	public void onHowToPlay() {
		ScreenManager.show(gameObject, false);
		ScreenManager.instance().show("HowToPlayUI", true);
	}

	public void onToggleSound() {

	}

	public void onToggleMusic() {
		
	}

	public void onSignInGoogle() {
		Debug.Log("onSignInGoogle");
	}

	public void onContactUs() {
		Debug.Log("onContactUs");
	}
}
