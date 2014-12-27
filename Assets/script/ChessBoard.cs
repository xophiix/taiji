using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class ChessBoard : MonoBehaviour {
	[Range(1, 20)]
	public int GridWidth = 7;
	[Range(1, 20)]
	public int GridHeight = 7;
	public Vector2 Span = new Vector2(4.0f, 4.0f);
	public GameObject gridPrefab;

	List<Image> _gridImages = new List<Image>();

	Vector2 _originalGridImageSize = new Vector2();
	Image _thisImage;

	Vector2 _gridSize = new Vector2();
	Vector2 _gridSpan = new Vector2();
	Rect _boardBounds = new Rect();

	public int gridCount {
		get { return GridWidth * GridHeight; }
	}

	Vector2 _standardGridSize = new Vector2();

	// Use this for initialization
	void Awake () {
		_thisImage = gameObject.GetComponent<Image>();
		_boardBounds= _thisImage.rectTransform.rect;

		Rect gridRect = gridPrefab.GetComponent<RectTransform>().rect;
		_originalGridImageSize.x = gridRect.width;
		_originalGridImageSize.y = gridRect.height;

		Rect bounds = gameObject.GetComponent<RectTransform>().rect;

		_standardGridSize.x = (bounds.width - (7 + 1) * Span.x) / 7;
		_standardGridSize.y = (bounds.height - (7 + 1) * Span.y) / 7;

		updateGrids();
	}

	void updateGrids() {
		Rect bounds = gameObject.GetComponent<RectTransform>().rect;

		_gridSize.x = (bounds.width - (GridWidth + 1) * Span.x) / GridWidth;
		_gridSize.y = (bounds.height - (GridHeight + 1) * Span.y) / GridHeight;
		_gridSpan.x = _gridSize.x + Span.x;
		_gridSpan.y = _gridSize.y + Span.y;

		Vector2 startPos = new Vector2(Span.x, Span.y);
		int gridCount = this.gridCount;
		if (_gridImages.Count < gridCount) {
			for (int i = _gridImages.Count; i < gridCount; ++i) {
				GameObject gridImage = (GameObject)Instantiate(gridPrefab);
				gridImage.transform.SetParent(gameObject.transform);
				_gridImages.Add(gridImage.GetComponent<Image>());
			}
		} else if (_gridImages.Count > gridCount) {
			for (int i = gridCount; i < _gridImages.Count; ++i) {
				Destroy(_gridImages[i].gameObject);
			}
			
			_gridImages.RemoveRange(gridCount, _gridImages.Count - gridCount);
		}

		float gridScaleX = _gridSize.x / _originalGridImageSize.x;
		float gridScaleY = _gridSize.y / _originalGridImageSize.y;
		Vector3 gridScale = new Vector3(gridScaleX, gridScaleY, 1.0f);
		
		for (int i = 0; i < gridCount; ++i) {
			Image gridImage = _gridImages[i];
			
			int gridX = i % GridWidth;
			int gridY = i / GridWidth;
			
			Vector3 gridPos = new Vector3();
			gridPos.x = startPos.x + gridX * _gridSpan.x;
			gridPos.y = startPos.y + gridY * _gridSpan.y;
			
			gridImage.transform.localPosition = gridPos;
			gridImage.transform.localScale = gridScale;
		}
	}

	public Vector3 screenPosToPosInBoard(Vector3 screenPosition) {
		Vector3 worldPosition = Camera.main.ScreenToWorldPoint(screenPosition);
		return _thisImage.rectTransform.InverseTransformPoint(worldPosition);
	}
	
	public bool getGridIndexByScreenPosition(Vector3 screenPosition, out Vector2 gridIndice) {
		Vector3 worldPosition = Camera.main.ScreenToWorldPoint(screenPosition);
		Vector3 posInBoard = _thisImage.rectTransform.InverseTransformPoint(worldPosition);
		
		gridIndice = new Vector2();
		float offsetX = posInBoard.x;
		float offsetY = posInBoard.y;
		if (offsetX < 0 || offsetY < 0 
		    || offsetX > _boardBounds.width 
		    || offsetY > _boardBounds.height) {
			return false;
		}

		if (offsetX < Span.x) {
			offsetX = Span.x;
		}

		if (offsetY < Span.y) {
			offsetY = Span.y;
		}

		gridIndice.Set(
			Mathf.FloorToInt((offsetX - Span.x) / _gridSpan.x),
			Mathf.FloorToInt((offsetY - Span.y) / _gridSpan.y)
		);
		
		return true;
	}

	public Vector2 gridIndexToPos(int index) {
		int y = index / GridWidth;
		int x = index % GridWidth;
		return new Vector2(x, y);
	}
	
	public int gridPosToIndex(Vector2 gridPos) {
		return gridPosToIndex((int)gridPos.x, (int)gridPos.y);
	}
	
	public int gridPosToIndex(int gridX, int gridY) {
		if (gridX < 0 || gridX >= GridWidth || gridY < 0 || gridY >= GridHeight) {
			return -1;
		}
		
		return (int)(gridX + gridY * GridWidth);
	}
	
	public Vector2 convertIndexToPosInBoard(int gridX, int gridY) {
		return new Vector2(
			Span.x + gridX * _gridSpan.x + 0.5f * _gridSize.x,
			Span.y + gridY * _gridSpan.y + 0.5f * _gridSize.y
		);
	}
	
	public Vector3 convertIndexToWorldPos(int gridX, int gridY) {
		Vector2 posInBoard = convertIndexToPosInBoard(gridX, gridY);
		return gameObject.transform.TransformPoint(new Vector3(posInBoard.x, posInBoard.y, 0));
	}

	private float _originPawnDimension;

	public void adjustPawn(GameObject pawn) {
		pawn.transform.localScale = Vector3.one;
		Image pawnImage = pawn.GetComponent<Image>();
		float originPawnDimension = pawnImage.rectTransform.rect.width;
		_originPawnDimension = originPawnDimension;
		float gridDimension = Mathf.Min(_gridSize.x, _gridSize.y);
		float standardScale = originPawnDimension / _standardGridSize.x;

		float adjustedPawnDimension = gridDimension * standardScale;
		pawnImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, adjustedPawnDimension);
		pawnImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, adjustedPawnDimension);
	}

	public void restorePawn(GameObject pawn) {
		pawn.transform.localScale = Vector3.one;
		Image pawnImage = pawn.GetComponent<Image>();
		pawnImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _originPawnDimension);
		pawnImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _originPawnDimension);
	}
}
