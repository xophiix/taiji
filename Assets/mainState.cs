using UnityEngine;
using System.Collections;

public class mainState : MonoBehaviour {

	private GameObject[] _pawns;
	private int _score;
	private int _nextPawn;
	public Object PawnPrefab;

	// Use this for initialization
	void Start () {
		_pawns = new GameObject[0];
		Debug.Log ("fetch pawn prefab " + PawnPrefab);
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.anyKeyDown) {
			Instantiate(PawnPrefab,
			                       new Vector3(Random.Range(-20, 20), 3, Random.Range(-20, 20)),
			                       Quaternion.identity
			                       );
		}
	}

	void OnGUI() {
		GUILayout.Label("test");
	}

	void DrawGrid() {

	}
}
