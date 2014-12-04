using UnityEngine;
using System.Collections;

public class Pawn : MonoBehaviour {
	private int _neighborCount;
	private int _destNeighborCount;

	private float _curMarkAngle;
	private float _destMarkAngle;

	void Start () {
	}

	void playEliminateAnim() {
		StartCoroutine("updateElimateAnim");
	}

	IEnumerator updateElimateAnim() {
		float scale = this.transform.localScale.x;
		while (scale > 0) {
			this.transform.localScale = new Vector3(scale, scale, 1);
			scale -= 0.1f;
			yield return null;
		}

		Destroy(this.gameObject);
	}

	void setNeighborCountMark(int neighborCount) {

	}
}
	