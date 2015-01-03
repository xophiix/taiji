using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class AchieveUIItem : MonoBehaviour {
	public Color finishedColor;
	public Color notFinishedColor;
	public Color defaultColor;

	private Text _label;
	private PawnDisplayer _firstEliminatedPawn;

	// Use this for initialization
	void Awake() {
		_label = gameObject.GetComponent<Text>();
		_firstEliminatedPawn = gameObject.transform.Find("Pawn1").GetComponent<PawnDisplayer>();
		_firstEliminatedPawn.gameObject.SetActive(false);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void setAchieve(AchievementConfig.AchieveItem achieveItem, bool finished) {
		_label.text = achieveItem.name.ToUpper();
		Color textColor = finished ? finishedColor : notFinishedColor;
		if (achieveItem.initFinished) {
			textColor = defaultColor;
		}

		_label.color = textColor;
		_firstEliminatedPawn.disableSendMessage = true;

		int firstPawnType = PlayerPrefs.GetInt("achieve_progress" + achieveItem.id + "_pawn_type0", -1);
		_firstEliminatedPawn.gameObject.SetActive(!finished && !achieveItem.initFinished && firstPawnType >= 0);
		if (_firstEliminatedPawn.gameObject.activeSelf) {
			_firstEliminatedPawn.pawnType = (PawnType)firstPawnType;
			_firstEliminatedPawn.setNeighborCountMark(achieveItem.parameters[0], true);
		}
	}
}
