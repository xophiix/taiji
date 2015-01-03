using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ExitConfirm : ScreenBase {
	override public void onShow(bool show) {
	}

	public void onOk() {
		GameApp.clearSave();
		Application.Quit();
	}

	public void onCancel() {
		ScreenManager.show(gameObject, false);

		GameObject gameMainUI = ScreenManager.instance().get("GameMainUI");
		MainState mainState = gameMainUI.GetComponent<MainState>();
		if (mainState.paused()) {
			mainState.pause(false);
		}
	}
}
