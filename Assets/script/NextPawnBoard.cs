using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class NextPawnBoard : MonoBehaviour {

	private ArrayList _pawns;
	private ArrayList _pawnImages;
	public GameObject nextPawnPrefab;
	public Sprite blackPawnSprite;
	public Sprite whitePawnSprite;

	public int GRID_COLUMN = 3;

	private float _gridWidth;
	private float _gridHeight;
	
	void Awake() {
		Image bgImage = gameObject.GetComponent<Image>();
		_gridWidth = bgImage.sprite.rect.width / GRID_COLUMN;
		_gridHeight = _gridWidth;
	}

	void Update () {
		if (_pawnImages == null) {
			_pawnImages = new ArrayList();
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
					GameObject pawnImage = (GameObject)_pawnImages[i];
					Destroy(pawnImage);
				}
				
				_pawnImages.RemoveRange(_pawns.Count, _pawnImages.Count - _pawns.Count);
			}
			
			for (int i = 0; i < _pawnImages.Count; ++i) {
				GameObject pawnImage = (GameObject)_pawnImages[i];
				Vector3 position = new Vector3();
				position.y = (i / GRID_COLUMN) * _gridHeight + _gridHeight / 2;
				position.x = (i % GRID_COLUMN) * _gridWidth + _gridWidth / 2;
				pawnImage.transform.localPosition = position;
				pawnImage.GetComponent<Image>().sprite = (MainState.PawnType)_pawns[i] == MainState.PawnType.Black ? blackPawnSprite : whitePawnSprite;
			}
		}
	}

	public void setNextPawns(ArrayList pawns) {
		_pawns = (ArrayList)pawns.Clone();
	}
}
