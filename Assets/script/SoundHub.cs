using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SoundHub : MonoBehaviour {

	private static SoundHub _instance;
	public static SoundHub instance() {
		return _instance;
	}

	private Dictionary<string, AudioSource> _audioSources = new Dictionary<string, AudioSource>();

	public void play(string name) {
		if (_audioSources.ContainsKey(name)) {
			AudioSource audio = _audioSources[name];
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
			_audioSources[child.gameObject.name] = child.audio;
		}
	}
}
