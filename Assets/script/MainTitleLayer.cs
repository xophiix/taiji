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

	void Awake() {
		_animator = gameObject.GetComponent<Animator>();
	}

	public void setScorePawnType(MainState.PawnType type) {
		_animator.ResetTrigger("Black");
		_animator.ResetTrigger("White");

		if (type == MainState.PawnType.Black) {
			_animator.SetTrigger("Black");
		} else {
			_animator.SetTrigger("White");
		}
	}
}
