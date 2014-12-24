using UnityEngine;
using System.Collections;

public class ManualLogoUI : ScreenBase {
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {	
		if (Input.anyKeyDown && IsInvoking("closeForInvoke")) {
			CancelInvoke();
			close();
		}
	}

	protected override void onScreenIn() {
		Invoke("closeForInvoke", 1.0f);
	}

	protected override void onScreenOut() {
		Application.LoadLevel(1);
	}

	private void closeForInvoke() {
		base.close();
	}
}
