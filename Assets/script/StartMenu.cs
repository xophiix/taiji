using UnityEngine;
using System.Collections;

public class StartMenu : MonoBehaviour {
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void onNewGame() {
		Hashtable parameters = new Hashtable();
		GameObject.Find("MainState").SendMessage("restart", parameters);
		Destroy(this.gameObject);
	}

	public void onQuit() {
		if (Application.isEditor) {
			onContinue();
			return;
		}

		if (Application.platform == RuntimePlatform.IPhonePlayer) {
			// TODO: show confirm on iphone
			Application.Quit();
		} else {
			Application.Quit();
		}
	}

	public void onContinue() {
		GameObject.Find("MainState").SendMessage("pause", false);
		Destroy(this.gameObject);
	}
}
