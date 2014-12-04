using UnityEngine;
using System.Collections;

public class MainState : MonoBehaviour {
	public Object BlackPawnPrefab;
	public Object WhitePawnPrefab;
	public GameObject ChessBoard;

	public int BoardWidth = 8;
	public int BoardHeight = 8;

	public enum GameState {
		WaitingPutPawn,
		UpdatingPawnState,
		JudgingElimination,
		GameOver
	};

	public enum PawnType {
		Black,
		White
	};
	
	public enum Side {
		Self,
		Opposite
	};

	private Side _turn = Side.Self;
	private GameState _gameState = GameState.WaitingPutPawn;
	private PawnType _myType;
	private PawnType _oppoType;	
	private int _score;
	private int _nextPawn;

	class Pawn {
		public int gridIndex;
		public Vector2 gridPos = new Vector2();
		public PawnType type;
		public int neighborOppositeCount;
		public GameObject obj;
	};

	private ArrayList _pawns = new ArrayList();
	private ArrayList _pawnListToEliminate = new ArrayList();
	private Pawn[] _grids;

	class BoardLayout {
		public Vector2 origin = new Vector2 ();
		public Vector2 size = new Vector2 ();
		public Vector2 gridSize = new Vector2 ();
	};

	private BoardLayout _boardLayout = new BoardLayout();

	// Use this for initialization
	void Start () {
		initScene();
	}

	// Update is called once per frame
	void Update () {
		if (_gameState == GameState.WaitingPutPawn) {
			if (_turn == Side.Opposite) {
				/*int gridIndex = getRandomEmptyPawnGridIndex();
				Debug.Log("getRandomEmptyPawnGridIndex return " + gridIndex);
				if (gridIndex >= 0) {
					Vector2 gridPos = gridIndexToPos(gridIndex);
					if (putPawn((int)gridPos.x, (int)gridPos.y, _oppoType)) {
						_turn = Side.Self;
					}
				}*/

				if (Input.anyKeyDown) {
					Vector3 worldPos = this.camera.ScreenToWorldPoint(Input.mousePosition);
					Vector2 gridIndice;
					if (convertActualPosToIndex(worldPos, out gridIndice)) {
						if (putPawn((int)gridIndice.x, (int)gridIndice.y, _oppoType)) {
							_turn = Side.Self;
						}
					};
				}
			} else {
				if (Input.anyKeyDown) {
					Vector3 worldPos = this.camera.ScreenToWorldPoint(Input.mousePosition);
					Vector2 gridIndice;
					if (convertActualPosToIndex(worldPos, out gridIndice)) {
						if (putPawn((int)gridIndice.x, (int)gridIndice.y, _myType)) {
							_turn = Side.Opposite;
						}
					};
				}
			}
		}
	}

	void OnGUI() {

	}

	void FixedUpdate() {

	}

	private int getRandomEmptyPawnGridIndex(int maxIteration = 5) {
		maxIteration = Random.Range(1, maxIteration);
		int interation = 0;
		int index = 0;
		int gridLength = _grids.Length;
		int foundIndex = -1;
		while (index < gridLength) {
			if (_grids[index] == null) {
				++interation;
				if (interation >= maxIteration) {
					foundIndex = index;
					break;
				}
			}

			++index;
		}

		return foundIndex;
	}

	private Vector2 gridIndexToPos(int index) {
		int y = index / BoardWidth;
		int x = index % BoardWidth;
		return new Vector2(x, y);
	}

	private bool convertActualPosToIndex(Vector3 pos, out Vector2 gridIndice) {
		gridIndice = new Vector2();
		float offsetX = pos.x - _boardLayout.origin.x;
		float offsetY = pos.y - _boardLayout.origin.y;
		if (offsetX < 0 || offsetY < 0 
		    || offsetX > _boardLayout.size.x 
		    || offsetY > _boardLayout.size.y) {
			return false;
		}

		gridIndice.Set(
			Mathf.FloorToInt(offsetX / _boardLayout.gridSize.x),
			Mathf.FloorToInt(offsetY / _boardLayout.gridSize.y)
		);

		return true;
	}

	private Vector2 convertIndexToActualPos(int gridX, int gridY) {
		return new Vector2(
			_boardLayout.origin.x + (gridX + 0.5f) * _boardLayout.gridSize.x,
			_boardLayout.origin.y + (gridY + 0.5f) * _boardLayout.gridSize.y
		);
	}

	private bool putPawn(int gridX, int gridY, PawnType type) {
		Pawn pawnAtPos = getPawnAtPos(gridX, gridY);
		if (pawnAtPos != null) {
			Debug.LogWarning("already have pawn at pos: " + new Vector2(gridX, gridY));
			return false;
		}

		int gridIndex = gridY * BoardWidth + gridX;
		Pawn pawn = new Pawn();
		pawn.gridIndex = gridIndex;
		pawn.gridPos = gridIndexToPos(gridIndex);
		pawn.type = type;
		pawn.neighborOppositeCount = getNeighborOppoCount(type, pawn.gridPos);

		Object prefab = type == PawnType.Black ? BlackPawnPrefab : WhitePawnPrefab;
		Vector2 posInChessBoard = convertIndexToActualPos(gridX, gridY);
		GameObject pawnObject = (GameObject)Instantiate(prefab, new Vector3(posInChessBoard.x, posInChessBoard.y, 0), Quaternion.identity);
		pawn.obj = pawnObject;
		updatePawnDisplay(pawn);

		_pawns.Add(pawn);
		_grids[gridIndex] = pawn;

		updateScenePawnState(pawn);
		return true;
	}

	Vector2[] NEIGHBOR_GRID_OFFSETS = new Vector2[]{
		new Vector2(-1, -1),
		new Vector2(0, -1),
		new Vector2(1, -1),
		new Vector2(-1, 0),
		new Vector2(1, 0),
		new Vector2(-1, 1),
		new Vector2(0, 1),
		new Vector2(1, 1)
	};

	private int getNeighborOppoCount(PawnType type, Vector2 pos) {
		int result = 0;
		for (int i = 0; i < NEIGHBOR_GRID_OFFSETS.Length; ++i) {
			Vector2 offset = NEIGHBOR_GRID_OFFSETS[i];
			Pawn pawn = getPawnAtPos((int)(offset.x + pos.x), (int)(offset.y + pos.y));
			if (pawn != null && pawn.type != type) {
				++result;
			}
		}

		return result;
	}

	private Pawn getPawnAtPos(int x, int y) {
		if (x < 0 || x >= BoardWidth || y < 0 || y >= BoardHeight) {
			return null;
		}

		int index = y * BoardWidth + x;
		return getPawnByGridIndex(index);
	}

	private Pawn getPawnByGridIndex(int index) {
		if (index < 0 || index >= _grids.Length) {
			return null;
		}
		
		return _grids[index];
	}

	private IEnumerator eliminateAdjacentPawns() {
		_gameState = GameState.JudgingElimination;
		for (int i = 0; i < _pawnListToEliminate.Count; ++i) {
			ArrayList pawnList = (ArrayList)_pawnListToEliminate[i];
			foreach (Pawn pawn in pawnList) {
				pawn.obj.SendMessage("playEliminateAnim");
				destroyPawn(pawn, true);
			}

			pawnList.Clear();
			yield return new WaitForSeconds(1.0f);
		}

		_pawnListToEliminate.Clear();
		clearGridFlags();

		updateScenePawnState();
	}

	private void clearGridFlags() {
		int dim0 = _gridFlags.GetLength(0);
		int dim1 = _gridFlags.GetLength(1);

		for (int i = 0; i < dim0; ++i) {
			for (int j = 0; j < dim1; ++j) {
				_gridFlags[i, j].adjacentPawnList = null;
			}
		}
	}

	private void updateScenePawnState(Pawn startPawn = null) {
		// traverse the map
		_gameState = GameState.UpdatingPawnState;
		if (startPawn == null) {
			foreach (Pawn pawn in _pawns) {
				pawn.neighborOppositeCount = getNeighborOppoCount(pawn.type, pawn.gridPos);
				updatePawnDisplay(pawn);
			}
		} else {
			// only update adjacent grids
			Vector2 startPawnPos = startPawn.gridPos;
			for (int i = 0; i < NEIGHBOR_GRID_OFFSETS.Length; ++i) {
				Vector2 offset = NEIGHBOR_GRID_OFFSETS[i];
				Pawn pawn = getPawnAtPos((int)(offset.x + startPawnPos.x), (int)(offset.y + startPawnPos.y));
				if (pawn != null && pawn.type != startPawn.type) {
					++pawn.neighborOppositeCount;
					updatePawnDisplay(pawn);
				}
			}
		}

		// check same pawn adjacent	
		_pawnListToEliminate.Clear();
		collectAdjacentPawns();

		Debug.Log("updateScenePawnState: " + _pawnListToEliminate.Count);

		if (_pawnListToEliminate.Count > 0) {
			Invoke("startEliminateAdjacentPawns", 0.3f);
		} else {
			_gameState = GameState.WaitingPutPawn;
		}
	}

	private void startEliminateAdjacentPawns() {
		StartCoroutine("eliminateAdjacentPawns");
	}

	struct GridFlag {
		public ArrayList adjacentPawnList;
	};

	// record 
	private GridFlag[,] _gridFlags;
	static PawnType[] CHECK_PAWN_TYPES = new PawnType[]{PawnType.Black, PawnType.White};
	public int ADJACENT_COUNT_TO_ELIMINATE = 3;

	enum TraverType {
		Horizon,
		Vertical,
		DiagonalDown,
		DiagonalUp,
		Count
	};

	private int[][][] _traverseIndice = new int[(int)TraverType.Count][][];

	private delegate bool AdjacentPawnJudger(Pawn p1, Pawn p2);
	AdjacentPawnJudger[]  _adjacentJudgers = new AdjacentPawnJudger[]{
		// Horizon
		delegate(Pawn pawn, Pawn lastPawn) {
			return pawn.gridPos.x == lastPawn.gridPos.x + 1 && pawn.neighborOppositeCount == lastPawn.neighborOppositeCount;
		},

		// Vertical
		delegate(Pawn pawn, Pawn lastPawn) {
			return pawn.gridPos.y == lastPawn.gridPos.y + 1 && pawn.neighborOppositeCount == lastPawn.neighborOppositeCount;
		},

		// DiagonalDown
		delegate(Pawn pawn, Pawn lastPawn) {
			return pawn.gridPos.x == lastPawn.gridPos.x + 1 && pawn.gridPos.y == lastPawn.gridPos.y - 1 
				&& pawn.neighborOppositeCount == lastPawn.neighborOppositeCount;
		},

		// DiagonalUp
		delegate(Pawn pawn, Pawn lastPawn) {
			return pawn.gridPos.x == lastPawn.gridPos.x + 1 && pawn.gridPos.y == lastPawn.gridPos.y + 1 
				&& pawn.neighborOppositeCount == lastPawn.neighborOppositeCount;
		}
	};

	private void collectAdjacentPawns() {
		for (int traverseType = 0; traverseType < (int)TraverType.Count; ++traverseType) {
			AdjacentPawnJudger judger = _adjacentJudgers[traverseType];
			int[][] traverseIndice = _traverseIndice[traverseType];

			int dim0 = traverseIndice.GetLength(0);
			for (int i = 0; i < dim0; ++i) {
				int[] indiceOnDim = traverseIndice[i];
				if (indiceOnDim == null) {
					Debug.Log(string.Format("collectAdjacentPawns: indiceOnDim is empty traverseType={0} dim0={1}", traverseType, i));
					continue;
				}

				for (int j = 0; j < CHECK_PAWN_TYPES.Length; ++j) {
					PawnType checkType = CHECK_PAWN_TYPES[j];
					ArrayList adjacentPawnList = null;
					Pawn lastPawn = null;

					for (int k = 0; k < indiceOnDim.Length; ++k) {
						int gridIndex = indiceOnDim[k];
						Pawn pawn = getPawnByGridIndex(gridIndex);
						if (pawn != null && pawn.type == checkType) {
							if (lastPawn == null) {
								lastPawn = pawn;
								if (adjacentPawnList == null) {
									adjacentPawnList = new ArrayList();
								}

								adjacentPawnList.Add(pawn);
							} else {
								// adjacent
								if (judger(pawn, lastPawn)) {
									adjacentPawnList.Add(pawn);
								} else {
									if (adjacentPawnList != null && adjacentPawnList.Count > 0) {
										checkAndAddAdjacentPawnList(checkType, adjacentPawnList);
										adjacentPawnList = null;
									}

									if (adjacentPawnList == null) {
										adjacentPawnList = new ArrayList();
									}

									adjacentPawnList.Add(pawn);
								}
								
								lastPawn = pawn;
							}
						}
					}

					if (adjacentPawnList != null) {
						checkAndAddAdjacentPawnList(checkType, adjacentPawnList);
						adjacentPawnList = null;
					}
				}
			}
		}
	}

	private bool checkAndAddAdjacentPawnList(PawnType checkType, ArrayList adjacentPawnList) {
		if (adjacentPawnList.Count >= ADJACENT_COUNT_TO_ELIMINATE) {
			// set grid flag
			ArrayList preCollectedAdjacentList = null;
			foreach (Pawn pawnInAdjacent in adjacentPawnList) {
				GridFlag gridFlag = _gridFlags[(int)checkType, pawnInAdjacent.gridIndex];
				if (gridFlag.adjacentPawnList != null) {
					preCollectedAdjacentList = gridFlag.adjacentPawnList;
					break;
				}
			}
			
			if (preCollectedAdjacentList != null) {
				// merge adjacent list available
				foreach (Pawn pawnInAdjacent in adjacentPawnList) {
					GridFlag gridFlag = _gridFlags[(int)checkType, pawnInAdjacent.gridIndex];
					if (gridFlag.adjacentPawnList != null) {
						mergeAdjacentPawnList(checkType, preCollectedAdjacentList, gridFlag.adjacentPawnList);
					} else {
						_gridFlags[(int)checkType, pawnInAdjacent.gridIndex].adjacentPawnList = adjacentPawnList ;
					}
				}
			} else {
				foreach (Pawn pawnInAdjacent in adjacentPawnList) {
					_gridFlags[(int)checkType, pawnInAdjacent.gridIndex].adjacentPawnList = adjacentPawnList;
				}
				
				Debug.Log ("add to eliminate:" + adjacentPawnList.Count + " type:" + checkType);
				_pawnListToEliminate.Add(adjacentPawnList);
			}

			return true;
		}

		return false;
	}
	
	private void mergeAdjacentPawnList(PawnType type, ArrayList targetList, ArrayList srcList) {
		foreach (Pawn pawn in srcList) {
			_gridFlags[(int)type, pawn.gridIndex].adjacentPawnList = targetList;
			if (!targetList.Contains(pawn)) {
				targetList.Add(pawn);
			}
		}
	}

	private void initScene() {
		Bounds boardBounds = ChessBoard.renderer.bounds;
		_boardLayout.origin.Set(boardBounds.min.x, boardBounds.min.y);
		_boardLayout.size.Set(boardBounds.size.x, boardBounds.size.y);
		_boardLayout.gridSize.x = _boardLayout.size.x / BoardWidth;
		_boardLayout.gridSize.y = _boardLayout.size.y / BoardHeight;

		int gridSize = BoardWidth * BoardHeight;
		_grids = new Pawn[gridSize];
		_gridFlags = new GridFlag[2, gridSize];

		initTraverseIndice();

		_myType = PawnType.Black;
		_oppoType = PawnType.White;
	}

	private void initTraverseIndice() {
		int[][] indice = new int[BoardHeight][];
		_traverseIndice[(int)TraverType.Horizon] = indice;
		for (int row = 0; row < BoardHeight; ++row) {
			indice[row] = new int[BoardWidth];
			for (int col = 0; col < BoardWidth; ++col) {
				indice[row][col] = row * BoardWidth + col;		
			}
		}

		indice = new int[BoardWidth][];
		for (int col = 0; col < BoardWidth; ++col) {
			indice[col] = new int[BoardHeight];
			for (int row = 0; row < BoardHeight; ++row) {
				indice[col][row] = row * BoardWidth + col;
			}
		}

		_traverseIndice[(int)TraverType.Vertical] = indice;

		if (BoardWidth < ADJACENT_COUNT_TO_ELIMINATE || BoardHeight < ADJACENT_COUNT_TO_ELIMINATE) {
			_traverseIndice[(int)TraverType.DiagonalDown] = new int[0][];
			_traverseIndice[(int)TraverType.DiagonalUp] = new int[0][];
		} else {
			// diagonalDown
			int dimension0 = 1 + (BoardHeight - ADJACENT_COUNT_TO_ELIMINATE) + (BoardWidth - ADJACENT_COUNT_TO_ELIMINATE);
			indice = new int[dimension0][];
			int indexOnIndice = 0;
			for (int row = ADJACENT_COUNT_TO_ELIMINATE - 1; row < BoardHeight; ++row) {
				ArrayList indiceOnThisLine = new ArrayList();
				for (int r = row, c = 0; r >= 0 && c < BoardWidth; --r, ++c) {
					int gridIndex = r * BoardWidth + c;
					indiceOnThisLine.Add(gridIndex);
				}

				indice[indexOnIndice++] = (int[])indiceOnThisLine.ToArray(typeof(int));
			}

			for (int col = 1; col < BoardWidth - ADJACENT_COUNT_TO_ELIMINATE + 1; ++col) {
				ArrayList indiceOnThisLine = new ArrayList();
				for (int r = BoardHeight - 1, c = col; r >= 0 && c < BoardWidth; --r, ++c) {
					int gridIndex = r * BoardWidth + c;
					indiceOnThisLine.Add(gridIndex);
				}
				
				indice[indexOnIndice++] = (int[])indiceOnThisLine.ToArray(typeof(int));
			}

			indexOnIndice = 0;
			_traverseIndice[(int)TraverType.DiagonalDown] = indice;

			// diagonalUp
			indice = new int[dimension0][];
			for (int col = BoardWidth - ADJACENT_COUNT_TO_ELIMINATE; col >= 0; --col) {
				ArrayList indiceOnThisLine = new ArrayList();
				for (int r = 0, c = col; r < BoardHeight && c < BoardWidth; ++r, ++c) {
					int gridIndex = r * BoardWidth + c;
					indiceOnThisLine.Add(gridIndex);
				}
				
				indice[indexOnIndice++] = (int[])indiceOnThisLine.ToArray(typeof(int));
			}

			for (int row = 1; row < BoardHeight - ADJACENT_COUNT_TO_ELIMINATE + 1; ++row) {
				ArrayList indiceOnThisLine = new ArrayList();
				for (int r = row, c = 0; r < BoardHeight && c < BoardWidth; ++r, ++c) {
					int gridIndex = r * BoardWidth + c;
					indiceOnThisLine.Add(gridIndex);
				}
				
				indice[indexOnIndice++] = (int[])indiceOnThisLine.ToArray(typeof(int));
			}

			_traverseIndice[(int)TraverType.DiagonalUp] = indice;
		}
	}

	private void destroyPawn(Pawn pawn, bool justUnRef = false) {
		if (pawn == null) {
			return;
		}

		_pawns.Remove(pawn);
		_grids[pawn.gridIndex] = null;
		if (!justUnRef) {
			Destroy (pawn.obj);
		}

		pawn.obj = null;
	}

	const float MARK_DISTANCE = 0.256f;
	const float MARK_RADIAN_UNIT = Mathf.PI / 4;

	private void updatePawnDisplay(Pawn pawn) {
		Transform mark = pawn.obj.transform.Find("mark");
		mark.renderer.enabled = pawn.neighborOppositeCount > 0;
		if (pawn.neighborOppositeCount > 0) {
			float angle = Mathf.PI / 2 - MARK_RADIAN_UNIT * (pawn.neighborOppositeCount - 1);
			Vector2 markPos = new Vector2();
			markPos.x = MARK_DISTANCE * Mathf.Cos(angle);
			markPos.y = MARK_DISTANCE * Mathf.Sin(angle);
			mark.transform.localPosition = new Vector3(markPos.x, markPos.y, 0);
		}
	}
}
