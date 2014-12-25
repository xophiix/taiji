﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SettingUI : ScreenBase {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void onBack() {
		ScreenManager.show(gameObject, false, "SlideOutReverse");
		ScreenManager.instance().show("StartMenu", true, "SlideInReverse");
	}

	public void onHowToPlay() {
		ScreenManager.show(gameObject, false);
		ScreenManager.instance().show("HowToPlayUI", true);
	}

	public void onToggleSound() {
		Toggle toggle = gameObject.transform.Find("ContentLayer/SettingLayer/ToggleSound").GetComponent<Toggle>();
		SoundHub.instance().muteSound(!toggle.isOn);
	}

	public void onToggleMusic() {
		Toggle toggle = gameObject.transform.Find("ContentLayer/SettingLayer/ToggleMusic").GetComponent<Toggle>();
		SoundHub.instance().muteMusic(!toggle.isOn);
	}

	public void onSignInGoogle() {
		Debug.Log("onSignInGoogle");
	}

	public void onContactUs() {
		Debug.Log("onContactUs");
	}
}
