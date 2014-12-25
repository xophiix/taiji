using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SoundHub : MonoBehaviour {

	private static SoundHub _instance;
	public static SoundHub instance() {
		return _instance;
	}

	private Dictionary<string, AudioSource> _soundFxs = new Dictionary<string, AudioSource>();
	private Dictionary<string, AudioSource> _musics = new Dictionary<string, AudioSource>();

	public void play(string name) {
		if (_soundFxs.ContainsKey(name)) {
			AudioSource audio = _soundFxs[name];
			if (audio) {
				audio.Play();
			}
		}
	}

	// Use this for initialization
	void Awake () {
		_instance = this;

		for (int i = 0; i < transform.childCount; ++i) {
			Transform child = transform.GetChild(i);
			_soundFxs[child.gameObject.name] = child.audio;
		}

		GameObject[] bgMusics = GameObject.FindGameObjectsWithTag("BgMusic");
		foreach (GameObject bgMusic in bgMusics) {
			_musics[bgMusic.name] = bgMusic.audio;
		}
	}

	public void muteSound(bool mute) {
		foreach (KeyValuePair<string, AudioSource> kvp in _soundFxs) {
			kvp.Value.mute = mute;
		}
	}

	public void muteMusic(bool mute) {
		foreach (KeyValuePair<string, AudioSource> kvp in _musics) {
			kvp.Value.mute = mute;
		}
	}
}
