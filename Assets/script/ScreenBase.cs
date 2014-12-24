using UnityEngine;
using System.Collections;

public class ScreenBase : MonoBehaviour {
	public string openAnim;
	public string closeAnim;
	public string normalState;

	private Animator _screenAnimator;
	private string _curOpenAnim;
	private string _curCloseAnim;

	public Animator screenAnimator {
		get { return _screenAnimator; }
	}

	protected void Awake () {
		if (openAnim == "") {
			openAnim = "ScreenIn";
		}

		if (closeAnim == "") {
			closeAnim = "ScreenOut";
		}

		if (normalState == "") {
			normalState = "Normal";
		}

		_curOpenAnim = openAnim;
		_curCloseAnim = closeAnim;

		_screenAnimator = gameObject.GetComponent<Animator>();
		ScreenManager.instance().registerScreen(gameObject.name, gameObject);
	}

	void OnDestroy() {
		ScreenManager.instance().unregisterScreen(gameObject.name);
	}

	public virtual void onShow(bool show) {

	}

	protected virtual void onScreenIn() {
		
	}

	protected virtual void onScreenOut() {
		
	}
	
	protected void Update () {
	}

	public void open(string anim = "", bool directly = false) {
		Debug.Log("open screen " + gameObject.name + " " + Time.time);
		gameObject.SetActive(true);
		gameObject.GetComponent<RectTransform>().localPosition = Vector3.zero;
		//gameObject.transform.SetAsLastSibling();
		if (_screenAnimator != null && !directly) {
			if (anim == "") {
				anim = openAnim;
			}

			_curOpenAnim = anim;
			_screenAnimator.SetTrigger(_curOpenAnim);
			StartCoroutine("startCheckOpen");
		} else {
			onOpened();
		}
	}

	private void onOpened() {
		Debug.Log("open screen done " + gameObject.name);
		onScreenIn();
		gameObject.SendMessageUpwards("onScreenIn", this);
	}

	IEnumerator startCheckOpen() {
		yield return new WaitForEndOfFrame();
		StartCoroutine(checkOpened());
	}

	IEnumerator checkOpened() {
		bool opened = false;
		while (!opened) {
			opened = _screenAnimator.GetCurrentAnimatorStateInfo(0).IsName(normalState);
			if (opened) {
				Debug.Log("opened on checkOpened " + gameObject.name);
				break;
			}

			yield return null;
		}

		onOpened();
	}

	public void close(string anim = "", bool directly = false) {
		Debug.Log("close screen " + gameObject.name + " " + Time.time);
		if (_screenAnimator != null && !directly) {
			if (anim == "") {
				anim = closeAnim;
			}

			_curCloseAnim = anim;
			_screenAnimator.SetTrigger(_curCloseAnim);
			StartCoroutine("startCheckClose");
		} else {
			onClosed();
		}
	}

	private void onClosed() {
		Debug.Log("close screen done " + gameObject.name + " " + Time.time);
		onScreenOut();
		gameObject.SendMessageUpwards("onScreenOut", this);
		gameObject.SetActive(false);
	}

	IEnumerator startCheckClose() {
		yield return new WaitForEndOfFrame();
		StartCoroutine(checkClosed());
	}

	IEnumerator checkClosed() {
		bool closed = false;
		while (!closed) {
			AnimatorStateInfo stateInfo = _screenAnimator.GetCurrentAnimatorStateInfo(0);
			closed = (stateInfo.IsName(_curCloseAnim) && stateInfo.normalizedTime >= 0.95f) || stateInfo.IsName(normalState);
			if (closed) {
				Debug.Log("close on checkClosed " + gameObject.name);
				break;
			}

			yield return null;
		}

		onClosed();
	}
}
