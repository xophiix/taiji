using UnityEngine;
using System.Collections;

public class GameInit : MonoBehaviour {
	private static GameInit _instance;
	public static GameInit instance() {
		return _instance;
	}

	// Use this for initialization
	void Awake() {
		_instance = this;
		ScreenManager.instance().show("StartMenu", true);
	}
}
