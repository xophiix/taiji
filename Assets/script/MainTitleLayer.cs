using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class MainTitleLayer : MonoBehaviour {
	public Color blackScoreColor = Color.black;
	public Color whiteScoreColor = Color.white;
	public Color blackScoreTextColor = Color.black;
	public Color whiteScoreTextColor = Color.white;

	private MainState.PawnType _scorePawnType = MainState.PawnType.Black;
	private Image _titleImage;
	private List<Text> _textList = new List<Text>();

	void Awake() {
		_titleImage = gameObject.GetComponent<Image>();
		_textList.Add(gameObject.transform.Find("Level").GetComponent<Text>());
		_textList.Add(gameObject.transform.Find("Trash/Count").GetComponent<Text>());
		_textList.Add(gameObject.transform.Find("Backwards/Count").GetComponent<Text>());

		foreach (Text text in _textList) {
			text.color = whiteScoreTextColor;
		}
	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void setScorePawnType(MainState.PawnType type) {
		Debug.Log("setScorePawnType");
		_scorePawnType = type;
		Color textColor;
		if (type == MainState.PawnType.Black) {
			_titleImage.color = blackScoreColor;
			textColor = blackScoreTextColor;
		} else {
			_titleImage.color = whiteScoreColor;
			textColor = whiteScoreTextColor;
		}

		foreach (Text text in _textList) {
			text.color = textColor;
		}
	}
}
