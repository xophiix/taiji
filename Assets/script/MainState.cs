using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MainState : MonoBehaviour {
	public GameObject PawnPrefab;

	public int BoardWidth = 8;
	public int BoardHeight = 8;

	public enum GameState {
		WaitingPutPawn,
		UpdatingPawnState,
		JudgingElimination,
		SelectingPawnToTrash,
		GameOver
	};

	public enum GameMode {
		AI,
		Self,
		Player,
		Count
	};

	public enum PawnType {
		Black,
		White
	};
	
	public enum Side {
		Self,
		Opposite,
		Count
	};

	public GameMode gameMode {
		get { return _gameMode; }
		set { _gameMode = value; }
	}

	private Side _turn = Side.Self;
	private GameState _gameState = GameState.WaitingPutPawn;
	private bool _paused;
	private GameMode _gameMode = GameMode.AI;
	private PawnType[] _nextPawnTypes = new PawnType[(int)Side.Count];
	private Pawn[] _lastPutPawns = new Pawn[(int)Side.Count];
	private int _score;
	private int _exp = 0;
	private int _expNextLevel = 100;
	private int _level = 1;
	private int _combo;
	private int _trashChance; 			// chance to cancel opposite's last pawn
	private int _backwardsChance; 		// chance to cancel opposite's last pawn
	private int _lastUsedBackwardsLock;

	class Pawn {
		public int gridIndex;
		public Vector2 gridPos = new Vector2();
		public PawnType type;
		private int _neighborOppositeCount;

		public int neighborOppositeCount {
			get { return _neighborOppositeCount; }
			set {
				_neighborOppositeCount = value;
				if (this.obj != null) {
					this.obj.SendMessage("setNeighborCountMark", value);
				}
			}
		}

		public GameObject obj;
		public Side side;
	};

	private ArrayList _pawns = new ArrayList();
	private ArrayList _pawnListToEliminate = new ArrayList();
	private Pawn[] _grids;

	class BoardLayout {
		public Vector2 size = new Vector2();
		public Vector2 gridSize = new Vector2();
	};

	private BoardLayout _boardLayout = new BoardLayout();

	// ui
	private Image _chessBoard;
	private Text _scoreText;
	private Text _comboText;
	private Image _nextPawnImage;
	private Image _expBar;
	private Text _trashChanceText;
	private Text _backChanceText;
	private Text _levelText;
	private bool _uiInvalid = true;
	private bool _nextPawnStateInvalid = true;
	private TitleLayer _titleLayer;
	private NextPawnBoard _nextPawnBoard;

	public GameObject startMenuPrefab;
	
	void Awake() {
		preInit();
	}

	void Start() {
		init();
	}

	bool _waitingForOppoSide;

	void performAIMove() {
		int gridIndex = getRandomEmptyPawnGridIndex();
		if (gridIndex >= 0) {
			Vector2 gridPos = gridIndexToPos(gridIndex);
			Pawn pawn = putPawn((int)gridPos.x, (int)gridPos.y, _nextPawnTypes[(int)_turn], _turn);
			if (pawn != null) {
				_lastPutPawns[(int)_turn] = pawn;
				prepareNextPawn(_turn);
				_turn = Side.Self;
			}
		}
	}

	void startWaitingOppoSide(float delay) {
		_waitingForOppoSide = true;
		Invoke("stopWaitingOppoSide", delay);
	}

	void stopWaitingOppoSide() {
		_waitingForOppoSide = false;
	}

	void Update() {
		if (_uiInvalid) {
			updateUI();
			_uiInvalid = false;
		}

		if (_paused) {
			return;
		}

		if (_gameState == GameState.WaitingPutPawn) {
			bool waitInput = false;
			if (_turn == Side.Opposite) {
				if (_waitingForOppoSide) {
					return;
				}

				if (_gameMode == GameMode.AI) {
					performAIMove();
				} else if (_gameMode == GameMode.Self){
					waitInput = true;
				}
			} else {
				waitInput = true;
			}

			if (waitInput) {
				if (Input.anyKeyDown) {
					Vector2 gridIndice;
					if (getGridIndexByScreenPosition(Input.mousePosition, out gridIndice)) {
						Pawn pawn = putPawn((int)gridIndice.x, (int)gridIndice.y, _nextPawnTypes[(int)_turn], _turn);
						if (pawn != null) {
							if (_turn == Side.Self) {
								if (_lastUsedBackwardsLock > 0) {
									--_lastUsedBackwardsLock;
								}
							}

							_lastPutPawns[(int)_turn] = pawn;
							prepareNextPawn(_turn);
							startWaitingOppoSide(_gameMode == GameMode.AI ? 1.0f : 0.0f);
							_turn = _turn == Side.Self ? Side.Opposite : Side.Self;
						}
					};
				}
			}
		} else if (_gameState == GameState.SelectingPawnToTrash) {
			if (Input.anyKeyDown && _turn == Side.Self) {
				Vector2 gridIndice;
				if (getGridIndexByScreenPosition(Input.mousePosition, out gridIndice)) {
					Pawn pawn = getPawnAtPos((int)gridIndice.x, (int)gridIndice.y);
					if (pawn != null && pawn.side == _turn) {
						--_trashChance;
						invalidUI();
						destroyPawn(pawn, true, "eliminate");
						_gameState = GameState.WaitingPutPawn;
					} else if (pawn == null) {
						_gameState = GameState.WaitingPutPawn;
					}
				};
			}
		}
	}

	private bool getGridIndexByScreenPosition(Vector3 screenPosition, out Vector2 gridIndice) {
		Vector3 worldPosition = Camera.main.ScreenToWorldPoint(screenPosition);
		Vector3 posInBoard = _chessBoard.rectTransform.InverseTransformPoint(worldPosition);

		gridIndice = new Vector2();
		float offsetX = posInBoard.x;
		float offsetY = posInBoard.y;
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

	private void prepareNextPawn(Side side) {
		float prob = Random.Range(0.0f, 1.0f);
		_nextPawnTypes[(int)side] = prob > 0.5 ? PawnType.Black : PawnType.White;
		_nextPawnStateInvalid = true;
		invalidUI();
	}

	public void pause(bool value) {
		if (_paused == value) {
			return;
		}

		_paused = value;
		gameObject.GetComponent<GraphicRaycaster>().enabled = !value;
	}

	// revert AI and self last pawn and reput my pawn
	public void onBtnBackwards() {
		if (_turn != Side.Self 
		    || _lastUsedBackwardsLock > 0 
		    || _gameMode != GameMode.AI 
		    || _backwardsChance <= 0
		    || _gameState != GameState.WaitingPutPawn) {
			return;
		}

		bool use = false;
		int side = (int)Side.Opposite;
		if (_lastPutPawns[side] != null) {
			destroyPawn(_lastPutPawns[side], true, "eliminate");
			use = true;
		}

		side = (int)Side.Self;
		if (_lastPutPawns[side] != null) {
			destroyPawn(_lastPutPawns[side], true, "eliminate");
			use = true;
		}

		if (use) {
			--_backwardsChance;
			invalidUI();
			_lastUsedBackwardsLock = 2;
		}
	}

	public void onBtnQuit() {
		pause (true);
		GameInit.instance().startMenu.SetActive(true);
		//Instantiate(startMenuPrefab);
	}

	// select and drop a self pawn 
	public void onBtnTrash() {
		if (_gameState != GameState.WaitingPutPawn 
		    || _turn != Side.Self
		    || _trashChance <= 0) {
			return;
		}

		_gameState = GameState.SelectingPawnToTrash;
	}

	private void invalidUI() {
		_uiInvalid = true;
	}

	#region game logic
	private int getRandomEmptyPawnGridIndex(int maxIteration = 8) {
		float dirProb = Random.Range(0.0f, 1.0f);
		maxIteration = Random.Range(1, maxIteration);
		int interation = 0;
		int index = 0;
		int gridLength = _grids.Length;
		int foundIndex = -1;

		if (dirProb < 0.5) {
			while (index < gridLength) {
				if (_grids[index] == null) {
					++interation;
					if (interation >= maxIteration) {
						foundIndex = index;
						break;
					}
				}
				
				index += Random.Range(1, 5);
			}
		} else {
			index = gridLength - 1;
			while (index >= 0) {
				if (_grids[index] == null) {
					++interation;
					if (interation >= maxIteration) {
						foundIndex = index;
						break;
					}
				}
				
				index -= Random.Range(1, 5);
			}
		}

		return foundIndex;
	}

	private Vector2 gridIndexToPos(int index) {
		int y = index / BoardWidth;
		int x = index % BoardWidth;
		return new Vector2(x, y);
	}

	private Vector2 convertIndexToPosInBoard(int gridX, int gridY) {
		return new Vector2(
			(gridX + 0.5f) * _boardLayout.gridSize.x,
			(gridY + 0.5f) * _boardLayout.gridSize.y
		);
	}

	private Pawn putPawn(int gridX, int gridY, PawnType type, Side side) {
		Pawn pawnAtPos = getPawnAtPos(gridX, gridY);
		if (pawnAtPos != null) {
			return null;
		}

		int gridIndex = gridY * BoardWidth + gridX;
		Pawn pawn = new Pawn();
		pawn.gridIndex = gridIndex;
		pawn.gridPos = gridIndexToPos(gridIndex);
		pawn.type = type;
		pawn.side = side;

		Vector2 posInChessBoard = convertIndexToPosInBoard(gridX, gridY);
		GameObject pawnObject = (GameObject)Instantiate(PawnPrefab, Vector3.zero, Quaternion.identity);

		pawnObject.transform.SetParent(_chessBoard.transform, true);
		pawnObject.transform.localPosition = new Vector3(posInChessBoard.x, posInChessBoard.y, 0);
		pawnObject.transform.localScale = Vector3.one;

		pawnObject.GetComponent<PawnDisplayer>().pawnType = type;
		pawn.obj = pawnObject;
		pawn.neighborOppositeCount = getNeighborOppoCount(type, pawn.gridPos);

		_pawns.Add(pawn);
		_grids[gridIndex] = pawn;

		updateScenePawnState(pawn);
		return pawn;
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

	private int calculateScore(ArrayList adjacentPawnList, int combo) {
		// score = 2 ^ maxNeighborCount * 2 ^ (N - 3);
		bool[] typeFlag = new bool[9];
		int typeCount = 0;
		int maxNeighborCount = 0;
		foreach (Pawn pawn in adjacentPawnList) {
			int neighborOppositeCount = pawn.neighborOppositeCount;
			if (neighborOppositeCount > maxNeighborCount) {
				maxNeighborCount = neighborOppositeCount;
			}

			if (!typeFlag[neighborOppositeCount]) {
				typeFlag[neighborOppositeCount] = true;
				++typeCount;
			}
		}

		int score = (int)Mathf.Pow(2, maxNeighborCount);
		if (adjacentPawnList.Count > ADJACENT_COUNT_TO_ELIMINATE) {
			score *= (int)Mathf.Pow(2, adjacentPawnList.Count - ADJACENT_COUNT_TO_ELIMINATE);
		}

		int comboScale = (int)Mathf.Pow(2, combo);
		return score * comboScale;
	}

	private int calculateExp(int addedScore) {
		return addedScore;
	}

	private void modifyExp(int exp) {
		_exp += exp;
		if (_exp >= _expNextLevel) {
			_exp -= _expNextLevel;
			_level++;
			onLevelUp();
		}

		invalidUI();
	}

	private void onLevelUp() {
		if (_level < 6) {
			// TODO: more up next chess
		}

		_trashChance++;
		_backwardsChance++;

		invalidUI();
	}

	private void modifyScore(int score, PawnType scorePawnType) {
		_score += score;
		modifyExp(calculateExp(score));
		invalidUI();
		_titleLayer.setScorePawnType(scorePawnType);
	}

	private IEnumerator eliminateAdjacentPawns() {
		_gameState = GameState.JudgingElimination;

		for (int i = 0; i < _pawnListToEliminate.Count; ++i) {
			ArrayList pawnList = (ArrayList)_pawnListToEliminate[i];
			concludeEliminateStats(pawnList);
			int addedScore = calculateScore(pawnList, _combo);
			modifyScore(addedScore, ((Pawn)pawnList[0]).type);

			foreach (Pawn pawn in pawnList) {
				destroyPawn(pawn, true, "eliminate");
			}

			pawnList.Clear();
			yield return new WaitForSeconds(0.3f);
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

	class EliminateStats {
		public int continuousEliminatePawnCount;
		public int comboByLastMove;
		public bool trashChanceGained;
		public bool backwardsChanceGained;
		public bool[] eliminateRowFlags;
		public bool enableStats;

		public EliminateStats(MainState container) {
			eliminateRowFlags = new bool[container.BoardHeight];
		}
	};

	public int TRASH_GAIN_ELIMINATE_PAWN_PER_MOVE = 4;
	public int BACKWARDS_GAIN_ELIMINATE_ROW_PER_MOVE = 2;

	EliminateStats _eliminateStats;

	private void resetEliminateStats(Side turn) {
		_combo = 0;
		invalidUI();

		_eliminateStats.enableStats = turn == Side.Self;
		_eliminateStats.continuousEliminatePawnCount = 0;
		_eliminateStats.comboByLastMove = 0;
		_eliminateStats.trashChanceGained = false;
		_eliminateStats.backwardsChanceGained = false;
		for (int i = 0; i < _eliminateStats.eliminateRowFlags.Length; ++i) {
			_eliminateStats.eliminateRowFlags[i] = false;
		}
	}

	private void concludeEliminateStats(ArrayList pawnList) {
		if (!_eliminateStats.enableStats) {
			return;
		}

		++_eliminateStats.comboByLastMove;
		_eliminateStats.continuousEliminatePawnCount += pawnList.Count;

		if (_eliminateStats.comboByLastMove > 1) {
			_combo = _eliminateStats.comboByLastMove - 1;
			invalidUI();
		}

		if (!_eliminateStats.trashChanceGained && _eliminateStats.continuousEliminatePawnCount >= TRASH_GAIN_ELIMINATE_PAWN_PER_MOVE) {
			++_trashChance;
			invalidUI();
			_eliminateStats.trashChanceGained = true;
		}

		if (!_eliminateStats.backwardsChanceGained) {
			// check two row
			int[] pawnCountOnRow = new int[BoardHeight];
			foreach (Pawn pawn in pawnList) {
				++pawnCountOnRow[(int)pawn.gridPos.y];
			}

			for (int row = 0; row < pawnCountOnRow.Length; ++row) {
				if (pawnCountOnRow[row] >= 3) {
					_eliminateStats.eliminateRowFlags[row] = true;
				}
			}

			int eliminateRowCount = 0;
			for (int row = 0; row < _eliminateStats.eliminateRowFlags.Length; ++row) {
				if (_eliminateStats.eliminateRowFlags[row]) {
					++eliminateRowCount;
				}
			}

			if (eliminateRowCount >= BACKWARDS_GAIN_ELIMINATE_ROW_PER_MOVE) {
				++_backwardsChance;
				invalidUI();
				_eliminateStats.backwardsChanceGained = true;
			}
		}
	}

	private void updateScenePawnState(Pawn startPawn = null) {
		// traverse the map
		_gameState = GameState.UpdatingPawnState;
		if (startPawn == null) {
			foreach (Pawn pawn in _pawns) {
				pawn.neighborOppositeCount = getNeighborOppoCount(pawn.type, pawn.gridPos);
			}
		} else {
			resetEliminateStats(_turn);

			// only update adjacent grids
			Vector2 startPawnPos = startPawn.gridPos;
			for (int i = 0; i < NEIGHBOR_GRID_OFFSETS.Length; ++i) {
				Vector2 offset = NEIGHBOR_GRID_OFFSETS[i];
				Pawn pawn = getPawnAtPos((int)(offset.x + startPawnPos.x), (int)(offset.y + startPawnPos.y));
				if (pawn != null && pawn.type != startPawn.type) {
					++pawn.neighborOppositeCount;
				}
			}
		}

		// check same pawn adjacent	
		_pawnListToEliminate.Clear();
		collectAdjacentPawns();

		if (_pawnListToEliminate.Count > 0) {
			Invoke("startEliminateAdjacentPawns", 0.3f);
		} else {
			_gameState = GameState.WaitingPutPawn;
			invalidUI();
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

	private void preInit() {		
		_eliminateStats = new EliminateStats(this);
	}

	private void init() {
		_chessBoard = gameObject.transform.Find("Background/ChessBoard").GetComponent<Image>();
		_boardLayout.size = _chessBoard.rectTransform.rect.size;
		_boardLayout.gridSize.x = _boardLayout.size.x / BoardWidth;
		_boardLayout.gridSize.y = _boardLayout.size.y / BoardHeight;

		int gridSize = BoardWidth * BoardHeight;
		_grids = new Pawn[gridSize];
		_gridFlags = new GridFlag[2, gridSize];
	
		_scoreText = gameObject.transform.Find("Background/ScoreLabel/Score").GetComponent<Text>();
		_comboText = gameObject.transform.Find("Background/ComboLabel/Combo").GetComponent<Text>();
		_backChanceText = gameObject.transform.Find("Background/TitleLayer/BtnBack/Count").GetComponent<Text>();
		_trashChanceText = gameObject.transform.Find("Background/TitleLayer/BtnTrash/Count").GetComponent<Text>();
		_levelText = gameObject.transform.Find("Background/TitleLayer/Level").GetComponent<Text>();
		_expBar = gameObject.transform.Find("Background/TitleLayer/ExpBar").GetComponent<Image>();
		_titleLayer = gameObject.transform.Find("Background/TitleLayer").GetComponent<TitleLayer>();
		_nextPawnBoard = gameObject.transform.Find("Background/NextPawnBoard").GetComponent<NextPawnBoard>();

		initTraverseIndice();
		prepareNextPawn(Side.Self);
		prepareNextPawn(Side.Opposite);
	}

	public void restart(Hashtable parameters) {
		while (_pawns.Count > 0) {
			destroyPawn((Pawn)_pawns[_pawns.Count - 1]);
		}

		_pawnListToEliminate.Clear();
		_pawns.Clear();

		_backwardsChance = 0;
		_trashChance = 0;
		_score = 0;
		_combo = 0;
		_gameState = GameState.WaitingPutPawn;
		_gameMode = parameters.Contains("gameMode") ? (GameMode)parameters["gameMode"] : GameMode.AI;
		_turn = Side.Self;

		resetEliminateStats(_turn);
		pause(false);
		invalidUI();

		prepareNextPawn(Side.Self);
		prepareNextPawn(Side.Opposite);
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

	private void destroyPawn(Pawn pawn, bool justUnRef = false, string message = "", object msgParam = null) {
		if (pawn == null) {
			return;
		}

		_pawns.Remove(pawn);
		_grids[pawn.gridIndex] = null;

		if (_lastPutPawns[(int)Side.Self] == pawn) {
			_lastPutPawns[(int)Side.Self] = null;
		}

		if (_lastPutPawns[(int)Side.Opposite] == pawn) {
			_lastPutPawns[(int)Side.Opposite] = null;
		}

		if (!justUnRef) {
			Destroy(pawn.obj);
		} else {
			if (message.Length > 0) {
				pawn.obj.SendMessage(message, msgParam);
			}
		}

		pawn.obj = null;
	}

	private void updateUI() {
		_scoreText.text = _score.ToString();
		_comboText.text = _combo.ToString();

		PawnType nextPawnType;
		if (_gameMode == GameMode.Self) {
			nextPawnType = _nextPawnTypes[(int)_turn];
		} else {
			nextPawnType = _nextPawnTypes[(int)Side.Self];
		}

		if (_nextPawnStateInvalid) {
			ArrayList nextPawnTypes = new ArrayList();
			nextPawnTypes.Add(nextPawnType);
			_nextPawnBoard.setNextPawns(nextPawnTypes);
			_nextPawnStateInvalid = false;
		}

		_backChanceText.text = _backwardsChance.ToString();
		_trashChanceText.text = _trashChance.ToString();
		_levelText.text = "LEVEL " + _level;

		float expScale = (float)_exp / _expNextLevel;
		_expBar.transform.localScale = new Vector3(expScale, 1, 1);
	}

	#endregion
}
