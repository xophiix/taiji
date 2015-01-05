using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GameOverUI : ScreenBase {
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

	public void hide() {
		ScreenManager.show(gameObject, false);
	}

	public void setGameRecord(GameRecord record) {
		gameObject.transform.Find("Panel/StatsLayer/HighScoreTitle/HighScore").GetComponent<Text>().text = record.highScore.ToString();
		gameObject.transform.Find("Panel/StatsLayer/ScoreTitle/Score").GetComponent<Text>().text = record.score.ToString();
		gameObject.transform.Find("Panel/StatsLayer/MaxComboTitle/MaxCombo").GetComponent<Text>().text = record.maxCombo.ToString();
		gameObject.transform.Find("Panel/StatsLayer/TurnsTitle/Turns").GetComponent<Text>().text = record.turns.ToString();
	}
}
