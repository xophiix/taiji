using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class XMas : MonoBehaviour {

	GameObject[] _characters;
	GameObject _snowFx;
	GameObject _tree;
	bool _playingAnim;
	// Use this for initialization
	void Awake () {
		_characters = GameObject.FindGameObjectsWithTag("JumpChar");
		foreach (GameObject c in _characters) {
			c.SetActive(false);
		}

		_snowFx = gameObject.transform.Find("Snow").gameObject;
		_snowFx.SetActive(false);

		_tree = gameObject.transform.Find("Tree").gameObject;
		_tree.SetActive(false);

		//gameObject.transform.parent.gameObject.GetComponentInParent<Image>().enabled = false;
	}

	void Start() {
		StartCoroutine(showCharSequence());
	}

	IEnumerator showCharSequence() {
		for (int i = _characters.Length - 1; i >= 0; --i) {
			_characters[i].SetActive(true);
			float delay = Random.Range(0.3f, 0.7f);

			yield return new WaitForSeconds(delay);
		}

		_snowFx.SetActive(true);
		_tree.SetActive(true);
		_playingAnim = true;
	}
	
	// Update is called once per frame
	void Update () {
		if (_playingAnim && _tree != null && _tree.activeSelf 
		    && _tree.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("End")) {
			if (Input.anyKeyDown) {
				//gameObject.transform.parent.gameObject.GetComponentInParent<Image>().enabled = true;
				_playingAnim = false;
				gameObject.GetComponent<Animator>().SetTrigger("Xmas");
				Invoke("xmasDone", 1.1f);
			}
		}
	}

	void xmasDone() {
		SendMessageUpwards("XmasDone");
	}
}
