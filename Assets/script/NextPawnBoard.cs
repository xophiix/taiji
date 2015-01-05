using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class NextPawnBoard : MonoBehaviour {
	private List<PawnType> _pawns = new List<PawnType>();
	private List<GameObject> _pawnImages;
	public GameObject nextPawnPrefab;
	public Sprite blackPawnSprite;
	public Sprite whitePawnSprite;

	public GameObject upNextGridPrefab;
	public int GRID_COLUMN = 3;
	public int GRID_ROW = 2;
	public Vector2 Span = new Vector2(2.0f, 2.0f);

	private Vector2 _gridSize;
	private Vector2 _boardSize;

	private int _movingCount;
	private int _movingIndex;
	private List<Vector3> _destPositions;

	void Awake() {
		initGrids();
	}

	void initGrids() {
		Image gridImage = upNextGridPrefab.GetComponent<Image>();
		_gridSize = gridImage.rectTransform.rect.size;

		_boardSize.x = GRID_COLUMN * (_gridSize.x + Span.x) + Span.x;
		_boardSize.y = GRID_ROW * (_gridSize.y + Span.y) + Span.y;

		RectTransform container = gameObject.GetComponent<RectTransform>();
		container.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _boardSize.x);
		container.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _boardSize.y);

		for (int row = 0; row < GRID_ROW; ++row) {
			for (int col = 0; col < GRID_COLUMN; ++col) {
				GameObject upNextGrid = (GameObject)Instantiate(upNextGridPrefab);
				upNextGrid.transform.SetParent(gameObject.transform);

				Vector3 gridPos = new Vector3();
				gridPos.x = Span.x + col * (_gridSize.x + Span.x);
				gridPos.y = Span.y + row * (_gridSize.y + Span.y);
				upNextGrid.transform.localPosition = gridPos;
			}
		}
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
					newPawnImage.name = "nextPawn" + i;
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

			Vector2 startPos = new Vector2();
			startPos.y = _boardSize.y;

			for (int i = 0; i < _pawnImages.Count; ++i) {
				GameObject pawnImage = _pawnImages[i];
				pawnImage.SetActive(true);

				int col = i % GRID_COLUMN;
				int row = i / GRID_COLUMN;

				Vector3 position = new Vector3();
				position.y = _boardSize.y - Span.y - row * (_gridSize.y + Span.y) - _gridSize.y / 2;
				position.x = Span.x + col * (_gridSize.x + Span.x) + _gridSize.x / 2;

				pawnImage.transform.localPosition = position;
				pawnImage.GetComponent<Image>().sprite = (PawnType)_pawns[i] == PawnType.Black ? blackPawnSprite : whitePawnSprite;
			}
		}
	}

	void Update () {

	}

	public void setNextPawns(List<PawnType> pawns) {
		_pawns.Clear();
		_pawns.AddRange(pawns);
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

	public void reset() {
		_pawns.Clear();

		_movingIndex = 0;
		_movingCount = 0;
		if (_pawnImages != null) {
			for (int i = 0; i < _pawnImages.Count; ++i) {
				Destroy(_pawnImages[i]);
			}

			_pawnImages.Clear();
		}

		_destPositions = null;
	}
}
