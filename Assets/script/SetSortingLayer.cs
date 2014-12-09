using UnityEngine;
using System.Collections;

public class SetSortingLayer : MonoBehaviour {
	public string sortingLayerName;
	public int sortingOrder = 0;

	// Use this for initialization
	void Awake () {
		Renderer renderer = GetComponent<Renderer>();
		if (renderer != null) {
			renderer.sortingLayerName = sortingLayerName;
			renderer.sortingOrder = sortingOrder;
		}

		ParticleSystem particleSystem = GetComponent<ParticleSystem>();
		if (particleSystem != null) {
			particleSystem.renderer.sortingLayerName = sortingLayerName;
			particleSystem.renderer.sortingOrder = sortingOrder;
		}
	}
}
