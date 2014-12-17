// ------------------------------------------------------------------------------
//  <autogenerated>
//      This code was generated by a tool.
//      Mono Runtime Version: 4.0.30319.1
// 
//      Changes to this file may cause incorrect behavior and will be lost if 
//      the code is regenerated.
//  </autogenerated>
// ------------------------------------------------------------------------------
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ScreenManager
{
	ScreenManager ()
	{
	}

	private static ScreenManager _instance;

	public static ScreenManager instance() {
		if (_instance == null) {
			_instance = new ScreenManager();
		}

		return _instance;
	}

	public void registerScreen(string name, GameObject screen) {
		_namedScreens[name] = screen;
	}

	public void unregisterScreen(string name) {
		_namedScreens.Remove(name);
	}

	public void releaseAll() {
		_namedScreens.Clear();
	}

	public GameObject get(string name) {
		return (GameObject)_namedScreens[name];
	}

	public void show(string name, bool showOrHide) {
		GameObject screen = (GameObject)_namedScreens[name];
		if (screen != null) {
			show(screen, showOrHide);
		}
	}

	public static void show(GameObject screen, bool showOrHide) {
		screen.SetActive(showOrHide);
		if (showOrHide) {
			screen.GetComponent<RectTransform>().localPosition = Vector3.zero;
		}

		ScreenBase screenBase = screen.GetComponent<ScreenBase>();
		if (screenBase != null) {
			screenBase.onShow(showOrHide);
		}
	}

	private Hashtable _namedScreens = new Hashtable();
}

