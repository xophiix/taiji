using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TitleLayer : MonoBehaviour {
	public Color blackScoreColor = Color.black;
	public Color whiteScoreColor = Color.white;
	public Color blackScoreTextColor = Color.black;
	public Color whiteScoreTextColor = Color.white;

	private MainState.PawnType _scorePawnType = MainState.PawnType.Black;
	private Image _titleImage;
	private Text _levelText;

	void Awake() {
		_titleImage = gameObject.GetComponent<Image>();
		_levelText = gameObject.transform.Find("Level").GetComponent<Text>();
		setScorePawnType(_scorePawnType);
	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void setScorePawnType(MainState.PawnType type) {
		_scorePawnType = type;
		if (type == MainState.PawnType.Black) {
			_titleImage.color = blackScoreColor;
			_levelText.color = blackScoreTextColor;
		} else {
			_titleImage.color = whiteScoreColor;
			_levelText.color = whiteScoreTextColor;
		}
	}
}
