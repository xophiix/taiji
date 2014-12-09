using UnityEngine;
using System.Collections;

public class GameInit : MonoBehaviour {

	public GameObject mainUI {
		get { return _mainUI; }
	}

	public GameObject startMenu {
		get { return _startMenu; }
	}

	private GameObject _mainUI;
	private GameObject _startMenu;

	private static GameInit _instance;
	public static GameInit instance() {
		return _instance;
	}

	// Use this for initialization
	void Awake() {
		_instance = this;

		GameObject mainUI = GameObject.Find("GameMainUI");
		if (mainUI != null) {
			mainUI.SetActive(false);
		}

		_mainUI = mainUI;
		_startMenu = GameObject.Find("StartMenu");
	}
}
