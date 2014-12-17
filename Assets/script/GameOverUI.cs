using UnityEngine;
using System.Collections;

public class GameOverUI : ScreenBase {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	override public void onShow(bool show) {
		if (show) {
			SoundHub.instance().play("GameOver");
		}
	}

	public void onShare() {
		Debug.Log("onShare");
	}

	public void onPlayAgain() {
		ScreenManager.show(gameObject, false);
		GameObject gameMainUI = ScreenManager.instance().get("GameMainUI");
		MainState mainState = gameMainUI.GetComponent<MainState>();
		mainState.restart();
	}
}
