using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TaijiLogoUI : ScreenBase {
	GameObject _xmasLayer;

	Animator _pawnAnimator;
	// Use this for initialization
	void Awake() {
		base.Awake();

		GameObject pawnContainer = gameObject.transform.Find("PawnContainer").gameObject;
		_pawnAnimator = pawnContainer.GetComponent<Animator>();
		pawnContainer.SetActive(false);

		_xmasLayer = gameObject.transform.Find("Xmas").gameObject;
		_xmasLayer.SetActive(false);
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.anyKeyDown && IsInvoking("closeForInvoke")) {
			CancelInvoke();
			close();
		}
	}

	protected override void onScreenIn() {
		System.DateTime date = System.DateTime.Now;
		if (date.Month == 12 && date.Day >= 24 && date.Day <= 28) {
			gameObject.GetComponent<Image>().color = new Color32(102, 23, 30, 255);
			_xmasLayer.SetActive(true);
		} else {
			XmasDone();
		}
	}

	void XmasDone() {
		_xmasLayer.SetActive(false);

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
