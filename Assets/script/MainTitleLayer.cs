using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class MainTitleLayer : MonoBehaviour {
	public Color blackScoreColor = Color.black;
	public Color whiteScoreColor = Color.white;
	public Color blackScoreTextColor = Color.black;
	public Color whiteScoreTextColor = Color.white;
	private Animator _animator;

	private PawnType _curScoredPawnType = PawnType.Unknown;

	void Awake() {
		_animator = gameObject.GetComponent<Animator>();
	}

	void OnEnable() {
		if (_curScoredPawnType != PawnType.Unknown) {
			setScorePawnType(_curScoredPawnType);
		}
	}

	public PawnType getScorePawnType() {
		return _curScoredPawnType;
	}

	public void setScorePawnType(PawnType type) {
		_curScoredPawnType = type;

		_animator.ResetTrigger("Black");
		_animator.ResetTrigger("White");

		if (type == PawnType.Black) {
			_animator.SetTrigger("Black");
		} else if (type == PawnType.White) {
			_animator.SetTrigger("White");
		}
	}
}
