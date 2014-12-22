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
		ScreenManager.show(gameObject, false);
		ScreenManager.instance().show("StartMenu", true);
	}

	public void onHowToPlay() {
		ScreenManager.show(gameObject, false);
		ScreenManager.instance().show("HowToPlayUI", true);
	}

	public void onToggleSound() {
		Toggle soundToggle = gameObject.transform.Find("ContentLayer/SettingLayer/ToggleSound").GetComponent<Toggle>();
		AudioListener.volume = soundToggle.isOn ? 1.0f : 0.0f;
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
