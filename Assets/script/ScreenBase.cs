using UnityEngine;
using System.Collections;

public class ScreenBase : MonoBehaviour {
	protected void Awake () {
		ScreenManager.instance().registerScreen(gameObject.name, gameObject);
	}

	void OnDestroy() {
		ScreenManager.instance().unregisterScreen(gameObject.name);
	}

	public virtual void onShow(bool show) {

	}

	// Update is called once per frame
	void Update () {
	
	}
}
