using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class NextPawnBoard : MonoBehaviour {


	private ArrayList _pawns;
	private List<GameObject> _pawnImages;
	public GameObject nextPawnPrefab;
	public Sprite blackPawnSprite;
	public Sprite whitePawnSprite;

	public int GRID_COLUMN = 3;

	private float _gridWidth;
	private float _gridHeight;

	private int _movingCount;
	private int _movingIndex;
	private List<Vector3> _destPositions;

	void Awake() {
		Image bgImage = gameObject.GetComponent<Image>();
		_gridWidth = bgImage.sprite.rect.width / GRID_COLUMN;
		_gridHeight = _gridWidth;
	}

	private void onNextPawnUpdated() {
		if (_pawnImages == null) {
			_pawnImages = new List<GameObject>();
		}
		
		if (_pawns != null) {
			if (_pawnImages.Count < _pawns.Count) {
				int addCount = _pawns.Count - _pawnImages.Count;
				int index = _pawnImages.Count;
				for (int i = 0; i < addCount; ++i, ++index) {
					GameObject newPawnImage = (GameObject)Instantiate(nextPawnPrefab);
					newPawnImage.transform.SetParent(gameObject.transform);
					_pawnImages.Add(newPawnImage);
				}
			}
			
			if (_pawnImages.Count > _pawns.Count) {
				for (int i = _pawns.Count; i < _pawnImages.Count; ++i) {
					GameObject pawnImage = _pawnImages[i];
					Destroy(pawnImage);
				}
				
				_pawnImages.RemoveRange(_pawns.Count, _pawnImages.Count - _pawns.Count);
			}
			
			for (int i = 0; i < _pawnImages.Count; ++i) {
				GameObject pawnImage = _pawnImages[i];
				pawnImage.SetActive(true);

				Vector3 position = new Vector3();
				position.y = (i / GRID_COLUMN) * _gridHeight + _gridHeight / 2;
				position.x = (i % GRID_COLUMN) * _gridWidth + _gridWidth / 2;
				pawnImage.transform.localPosition = position;
				pawnImage.GetComponent<Image>().sprite = (MainState.PawnType)_pawns[i] == MainState.PawnType.Black ? blackPawnSprite : whitePawnSprite;
			}
		}
	}

	void Update () {

	}

	public void setNextPawns(ArrayList pawns) {
		_pawns = (ArrayList)pawns.Clone();
		onNextPawnUpdated();
	}

	void onMovingDone(NextPawnController pawn) {
		--_movingCount;
		gameObject.SendMessageUpwards("onNextPawnSentToBoard");

		if (_movingCount == 0) {
			for (int i = 0; i < _pawnImages.Count; ++i) {
				_pawnImages[i].SetActive(false);
				_pawnImages[i].gameObject.transform.localScale = Vector3.one;
			}

			gameObject.SendMessageUpwards("onNextPawnsAllSentToBoard");
		} else {
			_pawnImages[_movingIndex].GetComponent<NextPawnController>().moveTo(_destPositions[_movingIndex], 500f);
			++_movingIndex;
		}
	}

	public void sendNextPawnsToBoard(List<Vector3> destPositions) {
		gameObject.transform.SetAsLastSibling();
		_movingCount = Mathf.Min(destPositions.Count, _pawnImages.Count);
		_destPositions = destPositions;
		if (_movingCount > 0) {
			_movingIndex = 0;
			_pawnImages[_movingIndex].GetComponent<NextPawnController>().moveTo(_destPositions[_movingIndex], 500f);
			++_movingIndex;
		}
	}
}
