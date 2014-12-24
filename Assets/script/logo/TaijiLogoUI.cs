using UnityEngine;
using System.Collections;

public class TaijiLogoUI : ScreenBase {

	Animator _pawnAnimator;
	// Use this for initialization
	void Awake() {
		base.Awake();

		GameObject pawnContainer = gameObject.transform.Find("PawnContainer").gameObject;
		_pawnAnimator = pawnContainer.GetComponent<Animator>();
		pawnContainer.SetActive(false);
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.anyKeyDown && IsInvoking("closeForInvoke")) {
			CancelInvoke();
			close();
		}
	}

	protected override void onScreenIn() {
		_pawnAnimator.gameObject.SetActive(true);
		_pawnAnimator.SetTrigger("LogoStart");
		StartCoroutine(checkAnimDone());
	}

	IEnumerator checkAnimDone() {
		bool animDone = false;
		while (!animDone) {
			yield return new WaitForEndOfFrame();
			animDone = _pawnAnimator.GetCurrentAnimatorStateInfo(0).IsName("LogoNormal");
		}

		Invoke("closeForInvoke", 1.5f);
	}

	private void closeForInvoke() {
		base.close();
	}

	protected override void onScreenOut() {
		ScreenManager.instance().show("ManualLogo", true);
	}
}
