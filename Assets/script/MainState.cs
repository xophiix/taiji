﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class MainState : ScreenBase {
	public GameObject PawnPrefab;

	// constants
	public int InitNextPawnCount = 3;
	public int MaxNextPawnCount = 6;

	public enum GameState {
		WaitingPutPawn,
		AIPuttingPawn,
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
	
	public enum Side {
		Self,
		Opposite,
		Count
	};

	public GameMode gameMode {
		get { return _gameMode; }
		set { _gameMode = value; }
	}

	private Side _turn = Side.Opposite;
	public GameState _gameState = GameState.WaitingPutPawn;
	private bool _paused;
	private GameMode _gameMode = GameMode.AI;
	private List<PawnType> _nextPawnTypes = new List<PawnType>();
	private ArrayList[] _lastPutPawns = new ArrayList[(int)Side.Count];
	private int _score;
	private int _exp;
	private int _expNextLevel;
	private int _level = 1;
	private int _combo;
	private int _newPawnCount;
	private int _trashChance; 			// chance to destroy a pawn on board
	private int _backwardsChance; 		// chance to cancel opposite's last pawn
	private int _lastUsedBackwardsLock;
	private Pawn _selectingPawn;
	private Pawn _selectingPawnToTrash;
	private float _selectTimeStamp;
	private bool _comboUpdatedThisTurn;

	class Pawn {
		public Pawn(PawnType type, int gridX, int gridY, Side side, MainState container) {
			_container = container;
			GameObject pawnObj = (GameObject)Instantiate(container.PawnPrefab, Vector3.zero, Quaternion.identity);
			_obj = pawnObj.GetComponent<PawnDisplayer>();
			pawnObj.transform.SetParent(container._chessBoard.transform, true);
			pawnObj.transform.localScale = Vector3.one;
			pawnObj.name = "pawn[" + gridY + "][" + gridX + "]";

			this.type = type;
			this.side = side;
			this.gridIndex = container._chessBoard.gridPosToIndex(gridX, gridY);

			_container._pawns.Add(this);
		}

		public void destroy(bool justUnRef = false, string message = "", object msgParam = null) {
			_container._pawns.Remove(this);
			_container._grids[this.gridIndex] = null;
			
			if (_container._lastPutPawns[(int)Side.Self].Count > 0) {
				if (_container._lastPutPawns[(int)Side.Self][0] == this) {
					_container._lastPutPawns[(int)Side.Self].Clear();
				}
			}
			
			if (_container._lastPutPawns[(int)Side.Opposite].Count > 0) {
				int indexInList = _container._lastPutPawns[(int)Side.Opposite].IndexOf(this);
				if (indexInList >= 0) {
					_container._lastPutPawns[(int)Side.Opposite].RemoveAt(indexInList);
				}
			}

			if (_container._selectingPawn == this) {
				this.selected = false;
				_container._selectingPawn = null;
			}

			if (_container._selectingPawnToTrash == this) {
				this.selected = false;
				_container._selectingPawnToTrash = null;
			}

			if (_obj != null) {
				if (!justUnRef) {
					Destroy(_obj.gameObject);
				} else {
					if (message.Length > 0) {
						_obj.gameObject.SendMessage(message, msgParam);
					}
				}
				
				_obj = null;
			}
		}

		public int gridIndex {
			get { return _gridIndex; }
			set {
				if (_gridIndex >= 0) {
					_container._grids[_gridIndex] = null;
				}

				_gridIndex = value;

				if (_gridIndex >= 0) {
					_container._grids[_gridIndex] = this;
					_gridPos = _container._chessBoard.gridIndexToPos(value);
					posUpdated();
				}
			}
		}

		public Vector2 gridPos {
			get { return _gridPos; }
			set {
				_gridPos = value;
				if (_gridIndex >= 0) {
					_container._grids[_gridIndex] = null;
				}

				_gridIndex = _container._chessBoard.gridPosToIndex(_gridPos);

				if (_gridIndex >= 0) {
					_container._grids[_gridIndex] = this;
					posUpdated();
				}
			}
		}

		private void posUpdated() {
			if (_obj != null) {
				Vector2 posInChessBoard = _container._chessBoard.convertIndexToPosInBoard((int)_gridPos.x, (int)_gridPos.y);
				_obj.gameObject.transform.localPosition = new Vector3(posInChessBoard.x, posInChessBoard.y, 0);
			}
		}

		public int neighborOppositeCount {
			get { return _neighborOppositeCount; }
			set {
				_neighborOppositeCount = value;
				if (_obj != null) {
					_obj.setNeighborCountMark(value, false);
				}
			}
		}

		public bool selected {
			get { return _selected; }
			set {
				_selected = value;
				if (_obj != null) {
					_obj.select(value);
				}
			}
		}

		public PawnType type {
			get { return _type; }
			set {
				_type = value;
				if (_obj != null) {
					_obj.pawnType = type;
				}
			}
		}

		public Side side {
			get; set;
		}

		public bool dragging {
			get; set;
		}

		public void setLocalPosition(Vector3 position) {
			_obj.gameObject.transform.localPosition = position;
		}
				
		private int _neighborOppositeCount;
		private PawnDisplayer _obj;
		private PawnType _type;
		private MainState _container;
		private int _gridIndex = -1;
		private Vector2 _gridPos;
		private bool _selected;
	};

	private List<Pawn> _pawns = new List<Pawn>();
	private List<AdjacentPawnList> _pawnListToEliminate = new List<AdjacentPawnList>();
	private Pawn[] _grids;
	
	private GameRecord _gameRecord = new GameRecord();

	private void updateGameRecord() {
		PlayerPrefs.SetInt("high_score", _gameRecord.highScore);
		PlayerPrefs.SetInt("max_combo", _gameRecord.maxCombo);
		PlayerPrefs.Save();
	}

	// ui
	private ChessBoard _chessBoard;
	private AnimateNumber _scoreText;
	private Text _comboText;
	private Image _nextPawnImage;
	private ExpBar _expBar;
	private Text _trashChanceText;
	private Text _backChanceText;
	private Button _trashButton;
	private Button _backButton;
	private Text _levelText;
	private bool _uiInvalid = true;
	private bool _nextPawnStateInvalid = true;
	private MainTitleLayer _titleLayer;
	private NextPawnBoard _nextPawnBoard;
	private bool _initialUpdateUI = true;

	void Awake() {
		base.Awake();
		preInit();
		init();
	}

	void Start() {

	}

	public void resume() {
		pause(false);
		if (_gameState == GameState.GameOver) {
			gameOver();
		}
	}

	void OnEnable() {
		updateUI(true);
	}

	void OnDisable() {

	}

	public override void onShow(bool show) {
		if (show) {
			pause(false);
		}
	}

	bool _waitingForOppoSide;

	void gameOver() {
		_gameState = GameState.GameOver;
		pause(true);
		GameObject gameOverUI = ScreenManager.instance().show("GameOverUI", true);
		gameOverUI.GetComponent<GameOverUI>().setGameRecord(_gameRecord);
	}


	int _pawnCountOnBoardBeforePut;
	List<Vector2> _gridIndiceForNextPawns = new List<Vector2>();

	void onNextPawnsAllSentToBoard() {
		for (int i = 0; i < _gridIndiceForNextPawns.Count; ++i) {
			Vector2 gridPos = _gridIndiceForNextPawns[i];
			Pawn pawn = putPawn((int)gridPos.x, (int)gridPos.y, (PawnType)_nextPawnTypes[i], _turn, true);
			if (pawn != null) {
				_lastPutPawns[(int)_turn].Add(pawn);
			}
		}

		_chessBoard.transform.SetAsLastSibling();
		prepareNextPawn();
		switchTurn();
		invalidUI();

		updateScenePawnState(true);
	}

	void onNextPawnSentToBoard() {
		++_pawnCountOnBoardBeforePut;
	}

	void stepBegin() {
		//Debug.Log ("step begin: combo=" + _combo + " state " + _gameState + " turn=" + _turn + " step=" + _gameRecord.turns);
		if (!_comboUpdatedThisTurn) {
			_combo = 0;
		}

		_comboUpdatedThisTurn = false;
		resetEliminateStats();
		invalidUI();
	}

	void stepEnd() {
		_gameRecord.turns++;
		switchTurn();
		//Debug.Log ("step end: combo=" + _combo + " state " + _gameState + " turn=" + _turn + " step=" + _gameRecord.turns);
	}

	void switchTurn() {		
		_turn = _turn == Side.Self ? Side.Opposite : Side.Self;
	}

	void performAIMove() {
		_lastPutPawns[(int)_turn].Clear();
		if (_pawns.Count >= _grids.Length) {
			gameOver();
			return;
		}

		bool randomStep = true;
		int maxSearchIteration = 8;
		_gameState = GameState.AIPuttingPawn;
		_pawnCountOnBoardBeforePut = _pawns.Count;
		_gridIndiceForNextPawns.Clear();

		HashSet<int> indiceToPutNextPawns = new HashSet<int>();
		for (int i = 0; i < _nextPawnTypes.Count;) {
			int gridIndex = getRandomEmptyPawnGridIndex(randomStep, maxSearchIteration, indiceToPutNextPawns);
			if (gridIndex < 0) {
				randomStep = false;
				maxSearchIteration = 1;
				gridIndex = getRandomEmptyPawnGridIndex(randomStep, maxSearchIteration, indiceToPutNextPawns);
			}

			if (gridIndex >= 0) {
				Vector2 gridPos = _chessBoard.gridIndexToPos(gridIndex);
				_gridIndiceForNextPawns.Add(gridPos);
				indiceToPutNextPawns.Add(gridIndex);
				++i;
			} else {
				break;
			}
		}

		if (debug) {
			debugSetNextPawnPositions();
		}

		List<Vector3> positions = new List<Vector3>();
		for (int i = 0; i < _gridIndiceForNextPawns.Count; ++i) {
			Vector2 gridPos = _gridIndiceForNextPawns[i];
			positions.Add(_chessBoard.convertIndexToWorldPos((int)gridPos.x, (int)gridPos.y));
		}

		Debug.Log("sendNextPawnsToBoard " + positions.Count);
		_nextPawnBoard.sendNextPawnsToBoard(positions);
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
			updateUI(_initialUpdateUI);
			_uiInvalid = false;
			_initialUpdateUI = false;
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

				stepBegin();
				if (_gameMode == GameMode.AI) {
					performAIMove();
				} else if (_gameMode == GameMode.Self){
					waitInput = true;
				}
			} else {
				waitInput = true;
			}

			if (waitInput) {
				if (_pawns.Count == 0) {
					stepEnd();
					return;
				} else if (_pawns.Count >= _grids.Length) {
					gameOver();
					return;
				}

				if (Input.GetMouseButtonDown(0)) {
					Vector2 gridIndice;
					if (_chessBoard.getGridIndexByScreenPosition(Input.mousePosition, out gridIndice)) {
						Pawn pawn = getPawnAtPos((int)gridIndice.x, (int)gridIndice.y);
						Debug.Log("select grid pos " + gridIndice + ", pawn" + pawn);
						if (pawn != null) {
							if (_selectingPawn == pawn) {
								if (!_selectingPawn.dragging) {
									_selectingPawn.selected = false;
									_selectingPawn = null;
								}
							} else {
								if (_selectingPawn != null) {
									_selectingPawn.selected = false;
									_selectingPawn.dragging = true;
								}

								_selectingPawn = pawn;
								_selectingPawn.selected = true;
								_selectingPawn.dragging = true;
								_selectTimeStamp = Time.time;
								SoundHub.instance().play("MoveBegin");
							}
						} else {
							putSelectingPawn(gridIndice);
						}
					} else {
						if (_selectingPawn != null) {
							_selectingPawn.selected = false;
							_selectingPawn.dragging = false;
							_selectingPawn = null;
						}
					}
				} else if (Input.GetMouseButton(0)) {
					if (Time.time - _selectTimeStamp < 0.1) {
						return;
					}

					if (_selectingPawn != null && _selectingPawn.dragging) {
						Vector3 posInBoard = _chessBoard.screenPosToPosInBoard(Input.mousePosition);
						posInBoard.z = 0;
						_selectingPawn.setLocalPosition(posInBoard);
					}
				} else if (Input.GetMouseButtonUp(0)) {
					if (_selectingPawn != null && _selectingPawn.dragging) {
						Vector2 gridIndice;
						if (_chessBoard.getGridIndexByScreenPosition(Input.mousePosition, out gridIndice)) {
							Pawn pawn = getPawnAtPos((int)gridIndice.x, (int)gridIndice.y);
							if (pawn != null) {
								cancelDrag();
							} else {
								putSelectingPawn(gridIndice);
							}
						} else {
							cancelDrag();
						}
					}
				}
			}
		} else if (_gameState == GameState.SelectingPawnToTrash) {
			if (Input.GetMouseButtonDown(0)) {
				Vector2 gridIndice;
				if (_chessBoard.getGridIndexByScreenPosition(Input.mousePosition, out gridIndice)) {
					Pawn pawn = getPawnAtPos((int)gridIndice.x, (int)gridIndice.y);
					if (pawn != null) {
						if (_selectingPawn == pawn) {
							if (!_selectingPawn.dragging) {
								_selectingPawn.selected = false;
								_selectingPawn = null;
							}
						} else {
							if (_selectingPawn != null) {
								_selectingPawn.selected = false;
								_selectingPawn.dragging = true;
							}
							
							_selectingPawn = pawn;
							_selectingPawn.selected = true;
							_selectingPawn.dragging = true;
							_selectTimeStamp = Time.time;
							SoundHub.instance().play("MoveBegin");
						}
					}
				} else if (_selectingPawn != null) {
					putPawnToTrash();
				}
			} else if (Input.GetMouseButton(0)) {
				if (Time.time - _selectTimeStamp < 0.1) {
					return;
				}
				
				if (_selectingPawn != null && _selectingPawn.dragging) {
					Vector3 posInBoard = _chessBoard.screenPosToPosInBoard(Input.mousePosition);
					posInBoard.z = 0;
					_selectingPawn.setLocalPosition(posInBoard);
				}
			} else if (Input.GetMouseButtonUp(0)) {
				if (_selectingPawn != null && _selectingPawn.dragging) {
					Vector2 gridIndice;
					if (_chessBoard.getGridIndexByScreenPosition(Input.mousePosition, out gridIndice)) {
						cancelDrag();
					} else {
						putPawnToTrash();
					}
				}
			}
		} else if (_gameState == GameState.GameOver) {

		}
	}

	private void putPawnToTrash() {
		if (_gameState != GameState.SelectingPawnToTrash || _selectingPawn == null || _trashChance <= 0) {
			_gameState = GameState.WaitingPutPawn;
			return;
		}

		modifyTrashChance(-1);
		SoundHub.instance().play("Trash");
		invalidUI();
		_selectingPawn.selected = false;
		_selectingPawn.destroy(true, "eliminate");
		_selectingPawn = null;

		_trashButton.animator.ResetTrigger("EnterTrash");
		_trashButton.animator.SetTrigger("Normal");
		updateScenePawnState(true);
	}

	private void cancelDrag() {
		if (_selectingPawn != null) {
			_selectingPawn.gridIndex = _selectingPawn.gridIndex;
			_selectingPawn.dragging = false;
		}
	}

	private void putSelectingPawn(Vector2 gridIndice) {
		// move selecting pawn to here
		if (_selectingPawn != null) {
			int preGridIndex = _selectingPawn.gridIndex;
			_selectingPawn.gridPos = gridIndice;
			_selectingPawn.selected = false;
			_selectingPawn.dragging = false;

			SoundHub.instance().play("MoveEnd");
			
			_lastPutPawns[(int)_turn].Clear();
			_lastPutPawns[(int)_turn].Add(_selectingPawn);
			_lastPutPawns[(int)_turn].Add(preGridIndex);
			
			updateScenePawnState(true);
			_selectingPawn = null;

			if (_turn == Side.Self) {
				if (_lastUsedBackwardsLock > 0) {
					--_lastUsedBackwardsLock;
				}
			}

			stepEnd();
			startWaitingOppoSide(_gameMode == GameMode.AI ? 0.3f : 0.0f);
		}
	}

	private void prepareNextPawn() {
		_nextPawnTypes.Clear();
		for (int i = 0; i < _newPawnCount; ++i) {
			float prob = Random.Range(0.0f, 1.0f);
			_nextPawnTypes.Add(prob > 0.5 ? PawnType.Black : PawnType.White);
		}

		if (debug) {
			debugPreparePawnTypes();
		}

		_nextPawnStateInvalid = true;
		invalidUI();
	}

	public bool paused() {
		return _paused;
	}

	public void pause(bool value) {
		if (_paused == value) {
			return;
		}

		_paused = value;
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
		int side = (int)Side.Self;
		Pawn lastSelfMovePawn = null;
		if (_lastPutPawns[side].Count > 0) {
			lastSelfMovePawn = (Pawn)_lastPutPawns[side][0];
			lastSelfMovePawn.gridIndex = (int)_lastPutPawns[side][1];
			use = true;

			_lastPutPawns[side].Clear();
		}

		side = (int)Side.Opposite;
		if (_lastPutPawns[side].Count > 0) {
			for (int i = 0; i < _lastPutPawns[side].Count; ) {
				Pawn pawn = (Pawn)_lastPutPawns[side][i];
				if (pawn == lastSelfMovePawn) {
					++i;
					continue;
				}

				pawn.destroy(true, "eliminate");
			}

			_lastPutPawns[side].Clear();
			use = true;
		}

		if (use) {
			SoundHub.instance().play("Backwards");
			modifyBackwardsChance(-1);
			_lastUsedBackwardsLock = 2;
		}
	}

	public void onBtnQuit() {
		pause(true);
		ScreenManager.show(gameObject, false);
		ScreenManager.instance().show("StartMenu", true);
	}

	// select and drop a self pawn 
	public void onBtnTrash() {
		if (_gameState == GameState.SelectingPawnToTrash) {
			_gameState = GameState.WaitingPutPawn;
			_trashButton.animator.ResetTrigger("EnterTrash");
			_trashButton.animator.SetTrigger("Normal");
			return;
		}

		if (_gameState != GameState.WaitingPutPawn 
		    || _turn != Side.Self
		    || _trashChance <= 0) {
			return;
		}

		_gameState = GameState.SelectingPawnToTrash;
		_trashButton.animator.ResetTrigger("Normal");
		_trashButton.animator.SetTrigger("EnterTrash");
	}

	private void invalidUI() {
		_uiInvalid = true;
	}

	#region game logic
	private int getRandomEmptyPawnGridIndex(bool randomStep = true, int maxIteration = 8, HashSet<int> excludeIndice = null) {
		float dirProb = Random.Range(0.0f, 1.0f);
		maxIteration = Random.Range(1, maxIteration);
		int interation = 0;
		int index = 0;
		int gridLength = _grids.Length;
		int foundIndex = -1;

		if (dirProb < 0.5) {
			while (index < gridLength) {
				if (_grids[index] == null && !excludeIndice.Contains(index)) {
					++interation;
					if (interation >= maxIteration) {
						foundIndex = index;
						break;
					}
				}
				
				index += randomStep ? Random.Range(1, 5) : 1;
			}
		} else {
			index = gridLength - 1;
			while (index >= 0) {
				if (_grids[index] == null && !excludeIndice.Contains(index)) {
					++interation;
					if (interation >= maxIteration) {
						foundIndex = index;
						break;
					}
				}
				
				index -= randomStep ? Random.Range(1, 5) : 1;
			}
		}

		return foundIndex;
	}

	private Pawn putPawn(int gridX, int gridY, PawnType type, Side side, bool skipUpdateScene = false) {
		Pawn pawnAtPos = getPawnAtPos(gridX, gridY);
		if (pawnAtPos != null) {
			return null;
		}

		Pawn pawn = new Pawn(type, gridX, gridY, side, this);
		if (!skipUpdateScene) {
			updateScenePawnState();
		}

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
		int index = _chessBoard.gridPosToIndex(x, y);
		if (index < 0) {
			return null;
		}

		return getPawnByGridIndex(index);
	}

	private Pawn getPawnByGridIndex(int index) {
		if (index < 0 || index >= _grids.Length) {
			return null;
		}
		
		return _grids[index];
	}

	private int calculateScore(List<Pawn> adjacentPawnList, int combo) {
		// score = 2 ^ (maxNeighborCount + 1) * 2 ^ (N - 3) * combo;
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

		int score = (int)Mathf.Pow(2, maxNeighborCount + 1);
		if (adjacentPawnList.Count > ADJACENT_COUNT_TO_ELIMINATE) {
			score *= (int)Mathf.Pow(2, adjacentPawnList.Count - ADJACENT_COUNT_TO_ELIMINATE);
		}

		if (combo <= 0) {
			combo = 1;
		}

		return score * combo;
	}

	private int calculateExp(int addedScore) {
		return addedScore;
	}

	private int calculateExpNextLevel(int level) {
		return Mathf.FloorToInt(100 * Mathf.Pow(2.0f, level - 1));
	}

	private void modifyExp(int exp, bool noReward = false) {
		_exp += exp;
		while (_exp >= _expNextLevel) {
			_exp -= _expNextLevel;
			_level++;
			_expNextLevel = calculateExpNextLevel(_level);
			_expBar.setExpRatio(1.0f);
			onLevelUp(noReward);
		}

		_expBar.setExpRatio((float)_exp / _expNextLevel);
		invalidUI();
	}

	private void levelUpDirectly(bool noReward = false) {
		modifyExp(_expNextLevel - _exp, noReward);
	}

	private void onLevelUp(bool noReward = false) {
		SoundHub.instance().play("LevelUp");
		FloatTip.instance.addTip("Level up!");

		if (_newPawnCount < MaxNextPawnCount) {
			++_newPawnCount;
		}

		if (!noReward) {
			modifyTrashChance(1);
			modifyBackwardsChance(1);
		}

		invalidUI();
	}

	private void modifyScore(int score, PawnType scorePawnType) {
		_score += score;
		modifyExp(calculateExp(score));
		invalidUI();
		_titleLayer.setScorePawnType(scorePawnType);

		_gameRecord.score = _score;
		if (_score > _gameRecord.highScore) {
			_gameRecord.highScore = _score;
			updateGameRecord();
		}
	}

	private void modifyTrashChance(int value) {
		_trashChance += value;
		if (_trashChance < 0) {
			_trashChance = 0;
		}

		invalidUI();

		if (_trashChance > 0 && value > 0) {
			string tip = string.Format("Discard{0}", "+" + value);
			FloatTip.instance.addTip(tip);
		}
	} 

	private void modifyBackwardsChance(int value) {
		_backwardsChance += value;
		if (_backwardsChance < 0) {
			_backwardsChance = 0;
		}

		invalidUI();

		if (_backwardsChance > 0 && value > 0) {
			string tip = string.Format("Undo{0}", "+" + value);
			FloatTip.instance.addTip(tip);
		}
	}

	private List<int> _newlyFinishedAchieves = new List<int>();

	private void checkAchieve(int eliminateNeighborCount, PawnType pawnType) {
		int[] parameters = new int[]{ eliminateNeighborCount, (int)pawnType };
		List<int> newlyFinishedAchieves = AchievementConfig.instance().checkAchieve(
			AchievementConfig.Condition.CLEAR_BY_TYPE, parameters, GameApp.instance().finishedAchieves);

		if (newlyFinishedAchieves.Count > 0) {
			_newlyFinishedAchieves.AddRange(newlyFinishedAchieves);
			GameApp.instance().saveAchieveChange();
		}
	}

	private IEnumerator eliminateAdjacentPawns() {
		_gameState = GameState.JudgingElimination;

		for (int i = 0; i < _pawnListToEliminate.Count; ++i) {
			AdjacentPawnList pawnList = _pawnListToEliminate[i];
			concludeEliminateStats(pawnList);
			int addedScore = calculateScore(pawnList.finalList, _combo);

			Pawn firstPawn = pawnList.finalList[0];
			modifyScore(addedScore, firstPawn.type);
			checkAchieve(firstPawn.neighborOppositeCount, firstPawn.type);

			int eliminateSoundLevel = firstPawn.neighborOppositeCount / 2 + 1;
			SoundHub.instance().play("Eliminate" + eliminateSoundLevel);

			foreach (Pawn pawn in pawnList.finalList) {
				pawn.destroy(true, "eliminate");
			}

			pawnList.finalList.Clear();
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
	
	public class EliminateStats {
		public int continuousEliminatePawnCount;
		public int continuousEliminatePawnListCount;
		public int comboByLastMove;
		public bool trashChanceGained;
		public bool backwardsChanceGained;
		public bool[] eliminateRowFlags;
		public bool enableStats;

		public EliminateStats(MainState container) {
			eliminateRowFlags = new bool[container._chessBoard.GridHeight];
		}
	};

	public int BACKWARDS_GAIN_ELIMINATE_PAWN_PER_MOVE = 4;
	public int TRASH_GAIN_ELIMINATE_ROW_PER_MOVE = 2;

	EliminateStats _eliminateStats;

	private void resetEliminateStats() {
		invalidUI();

		_eliminateStats.enableStats = true;
		_eliminateStats.continuousEliminatePawnCount = 0;
		_eliminateStats.continuousEliminatePawnListCount = 0;
		_eliminateStats.comboByLastMove = 0;
		_eliminateStats.trashChanceGained = false;
		_eliminateStats.backwardsChanceGained = false;
		for (int i = 0; i < _eliminateStats.eliminateRowFlags.Length; ++i) {
			_eliminateStats.eliminateRowFlags[i] = false;
		}
	}

	private void concludeEliminateStats(AdjacentPawnList pawnList) {
		if (!_eliminateStats.enableStats) {
			return;
		}

		++_eliminateStats.comboByLastMove;
		_eliminateStats.continuousEliminatePawnCount += pawnList.finalList.Count;
		_eliminateStats.continuousEliminatePawnListCount += pawnList.mergedLists.Count;

		if (!_comboUpdatedThisTurn) {
			++_combo;
			_comboUpdatedThisTurn = true;

			if (_combo > _gameRecord.maxCombo) {
				_gameRecord.maxCombo = _combo;
			}
		}

		invalidUI();

		if (pawnList.maxPawnInListBeforeMerge >= BACKWARDS_GAIN_ELIMINATE_PAWN_PER_MOVE) {
			modifyBackwardsChance(1);
		}

		if (!_eliminateStats.trashChanceGained) {
			if (_eliminateStats.continuousEliminatePawnListCount >= TRASH_GAIN_ELIMINATE_ROW_PER_MOVE) {
				modifyTrashChance(1);
				_eliminateStats.trashChanceGained = true;
			}
		}
	}

	private int _updatingPawnCountToWait;

	private void onPawnNeighborMarkUpdateDone(PawnDisplayer pawn) {
		--_updatingPawnCountToWait;
		if (_updatingPawnCountToWait <= 0) {
			collectAndEliminateAdjacentPawns();
		}
	}

	private void collectAndEliminateAdjacentPawns() {
		// check same pawn adjacent	
		_pawnListToEliminate.Clear();
		collectAdjacentPawns();
		
		if (_pawnListToEliminate.Count > 0) {
			Invoke("startEliminateAdjacentPawns", 0.3f);
		} else {
			if (_newlyFinishedAchieves.Count > 0) {
				notifyAchieveGet(_newlyFinishedAchieves);
				_newlyFinishedAchieves.Clear();
			}
						
			_gameState = GameState.WaitingPutPawn;
		}
	}

	private void updateScenePawnState(bool triggerByUser = false) {
		// traverse the map
		_gameState = GameState.UpdatingPawnState;

		if (triggerByUser) {
			_newlyFinishedAchieves.Clear();
			resetEliminateStats();
		}

		foreach (Pawn pawn in _pawns) {
			int preNeighborOppositeCount = pawn.neighborOppositeCount;
			int newNeighborOppositeCount = getNeighborOppoCount(pawn.type, pawn.gridPos);
			if (newNeighborOppositeCount != preNeighborOppositeCount) {
				++_updatingPawnCountToWait;
			}

			pawn.neighborOppositeCount = newNeighborOppositeCount;
		}

		if (_updatingPawnCountToWait > 0) {
			return;
		}
		// only update adjacent grids
		/*Vector2 startPawnPos = startPawn.gridPos;
		for (int i = 0; i < NEIGHBOR_GRID_OFFSETS.Length; ++i) {
			Vector2 offset = NEIGHBOR_GRID_OFFSETS[i];
			Pawn pawn = getPawnAtPos((int)(offset.x + startPawnPos.x), (int)(offset.y + startPawnPos.y));
			if (pawn != null && pawn.type != startPawn.type) {
				++pawn.neighborOppositeCount;
			}
		}*/

		collectAndEliminateAdjacentPawns();
	}

	public void notifyAchieveGet(List<int> newlyFinishedAchieves) {
		pause(true);
		ScreenManager.instance().show("AchieveNotifyUI", true);
		ScreenManager.instance().get("AchieveNotifyUI").GetComponent<AchieveNotifyUI>().setFinishedAchieveIds(newlyFinishedAchieves);
	}

	private void startEliminateAdjacentPawns() {
		StartCoroutine("eliminateAdjacentPawns");
	}

	// stands for a connected adjacent pawn list
	// maybe combined by several atomic adjacent list
	class AdjacentPawnList {
		public List<Pawn> finalList = new List<Pawn>();
		public int maxPawnInListBeforeMerge;

		public List<List<Pawn>> mergedLists = new List<List<Pawn>>();

		override public string ToString() {
			return "{finalListCount=" + finalList.Count 
				+ ", mergedListCount=" + mergedLists.Count
				+ ", maxPawnInListBeforeMerge=" + maxPawnInListBeforeMerge + "}";
		}

		public void removeOverlappedMergedLists() {
			// sort lists by length
			mergedLists.Sort(delegate(List<Pawn> list1, List<Pawn> list2) {
				return list2.Count.CompareTo(list1.Count);
			});

			Dictionary<int, int> gridIndexToPawnOccupyCount = new Dictionary<int, int>();
			for (int i = 0; i < mergedLists.Count; ++i) {
				for (int j = 0; j < mergedLists[i].Count; ++j) {
					int gridIndex = mergedLists[i][j].gridIndex;
					if (gridIndexToPawnOccupyCount.ContainsKey(gridIndex)) {
						gridIndexToPawnOccupyCount[gridIndex]++;
					} else {
						gridIndexToPawnOccupyCount[gridIndex] = 1;
					}
				}
			}

			for (int i = 0; i < mergedLists.Count; ) {
				List<Pawn> mergedList = mergedLists[i];
				int overlappedPawnCount = 0;
				for (int j = 0; j < mergedList.Count; ++j) {
					if (gridIndexToPawnOccupyCount[mergedList[j].gridIndex] > 1) {
						overlappedPawnCount++;
					}
				}

				if (overlappedPawnCount == mergedList.Count) {
					mergedLists.RemoveAt(i);
				} else {
					++i;
				}
			}
		}
	};

	struct GridFlag {
		public AdjacentPawnList adjacentPawnList;
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
					List<Pawn> adjacentPawnList = null;
					Pawn lastPawn = null;

					for (int k = 0; k < indiceOnDim.Length; ++k) {
						int gridIndex = indiceOnDim[k];
						Pawn pawn = getPawnByGridIndex(gridIndex);
						if (pawn != null && pawn.type == checkType) {
							if (lastPawn == null) {
								lastPawn = pawn;
								if (adjacentPawnList == null) {
									adjacentPawnList = new List<Pawn>();
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
										adjacentPawnList = new List<Pawn>();
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

	private bool checkAndAddAdjacentPawnList(PawnType checkType, List<Pawn> adjacentPawnList) {
		if (adjacentPawnList.Count >= ADJACENT_COUNT_TO_ELIMINATE) {

			// set grid flag
			AdjacentPawnList preCollectedAdjacentList = null;

			foreach (Pawn pawnInAdjacent in adjacentPawnList) {
				GridFlag gridFlag = _gridFlags[(int)checkType, pawnInAdjacent.gridIndex];
				if (gridFlag.adjacentPawnList != null) {
					preCollectedAdjacentList = gridFlag.adjacentPawnList;
					break;
				}
			}
			
			if (preCollectedAdjacentList != null) {
				// merge adjacent list available
				HashSet<AdjacentPawnList> differentAdjacentLists = new HashSet<AdjacentPawnList>();

				foreach (Pawn pawnInAdjacent in adjacentPawnList) {
					GridFlag gridFlag = _gridFlags[(int)checkType, pawnInAdjacent.gridIndex];
					if (gridFlag.adjacentPawnList != null) {
						if (gridFlag.adjacentPawnList != preCollectedAdjacentList) {
							mergeAdjacentPawnList(checkType, preCollectedAdjacentList, gridFlag.adjacentPawnList.finalList);
							differentAdjacentLists.Add(gridFlag.adjacentPawnList);
						}
					} else {
						_gridFlags[(int)checkType, pawnInAdjacent.gridIndex].adjacentPawnList = preCollectedAdjacentList;
						preCollectedAdjacentList.finalList.Add(pawnInAdjacent);
						differentAdjacentLists.Add(null);
					}
				}

				foreach (AdjacentPawnList list in differentAdjacentLists) {
					if (list != null) {
						preCollectedAdjacentList.mergedLists.AddRange(list.mergedLists);
					} else {
						preCollectedAdjacentList.mergedLists.Add(adjacentPawnList);
					}
					 
					if (list != null && list.maxPawnInListBeforeMerge > preCollectedAdjacentList.maxPawnInListBeforeMerge) {
						preCollectedAdjacentList.maxPawnInListBeforeMerge = list.maxPawnInListBeforeMerge;
					}
				}

				preCollectedAdjacentList.removeOverlappedMergedLists();

				if (adjacentPawnList.Count > preCollectedAdjacentList.maxPawnInListBeforeMerge) {
					preCollectedAdjacentList.maxPawnInListBeforeMerge = adjacentPawnList.Count;
				}

				Debug.Log ("update eliminate list:" + preCollectedAdjacentList.ToString());
			} else {
				AdjacentPawnList newAdjacentList = new AdjacentPawnList();
				newAdjacentList.finalList.AddRange(adjacentPawnList);
				newAdjacentList.maxPawnInListBeforeMerge = adjacentPawnList.Count;
				newAdjacentList.mergedLists.Add(adjacentPawnList);

				foreach (Pawn pawnInAdjacent in adjacentPawnList) {
					_gridFlags[(int)checkType, pawnInAdjacent.gridIndex].adjacentPawnList = newAdjacentList;
				}
				
				Debug.Log ("add eliminate list:" + newAdjacentList.ToString());
				_pawnListToEliminate.Add(newAdjacentList);
			}

			return true;
		}

		return false;
	}
	
	private void mergeAdjacentPawnList(PawnType type, AdjacentPawnList targetList, List<Pawn> srcList) {
		foreach (Pawn pawn in srcList) {
			_gridFlags[(int)type, pawn.gridIndex].adjacentPawnList = targetList;
			if (!targetList.finalList.Contains(pawn)) {
				targetList.finalList.Add(pawn);
			}
		}
	}
	
	void OnDestroy() {
		_chessBoard.restorePawn(PawnPrefab);
	}

	private void preInit() {	
		_chessBoard = gameObject.transform.Find("ChessBoard").GetComponent<ChessBoard>();
		_chessBoard.adjustPawn(PawnPrefab);

		_eliminateStats = new EliminateStats(this);
	}

	private void init() {
		int gridCount = _chessBoard.gridCount;
		_grids = new Pawn[gridCount];
		_gridFlags = new GridFlag[2, gridCount];

		_lastPutPawns[(int)Side.Self] = new ArrayList();
		_lastPutPawns[(int)Side.Opposite] = new ArrayList();

		_scoreText = gameObject.transform.Find("ScoreLabel/Score").GetComponent<AnimateNumber>();
		_comboText = gameObject.transform.Find("ComboLabel/Combo").GetComponent<Text>();
		_backChanceText = gameObject.transform.Find("TitleLayer/Backwards/Count").GetComponent<Text>();
		_trashChanceText = gameObject.transform.Find("TitleLayer/TrashCount").GetComponent<Text>();
		_backButton = gameObject.transform.Find("TitleLayer/Backwards").GetComponent<Button>();
		_trashButton = gameObject.transform.Find("TitleLayer/Trash").GetComponent<Button>();
		_levelText = gameObject.transform.Find("TitleLayer/Level").GetComponent<Text>();
		_expBar = gameObject.transform.Find("TitleLayer/ExpBar").GetComponent<ExpBar>();
		_titleLayer = gameObject.transform.Find("TitleLayer").GetComponent<MainTitleLayer>();
		_nextPawnBoard = gameObject.transform.Find("NextPawnBoard").GetComponent<NextPawnBoard>();

		initTraverseIndice();
		prepareNextPawn();
	}

	public void restart(Hashtable parameters = null) {
		while (_pawns.Count > 0) {
			((Pawn)_pawns[_pawns.Count - 1]).destroy();
		}

		_pawnListToEliminate.Clear();
		_pawns.Clear();

		_newPawnCount = InitNextPawnCount;
		_backwardsChance = 0;
		_trashChance = 0;
		_lastUsedBackwardsLock = 0;
		_score = 0;
		_combo = 0;
		_exp = 0;
		_level = 1;
		_expNextLevel = calculateExpNextLevel(_level);
		_gameState = GameState.WaitingPutPawn;
		_gameMode = (parameters != null && parameters.Contains("gameMode")) ? 
			(GameMode)parameters["gameMode"] : GameMode.AI;

		_turn = Side.Opposite;

		_gameRecord.highScore = PlayerPrefs.GetInt("high_score", 0);
		if (_titleLayer != null) {
			_titleLayer.setScorePawnType(PawnType.Unknown);
		}

		_nextPawnBoard.reset();
		_expBar.reset();
		_scoreText.reset();

		resetEliminateStats();
		pause(false);
		invalidUI();

		prepareNextPawn();
		Invoke("triggerUpdateUI", 0.3f);
	}

	void triggerUpdateUI() {
		updateUI(true);
	}

	private void initTraverseIndice() {
		int BoardWidth = _chessBoard.GridWidth;
		int BoardHeight = _chessBoard.GridHeight;

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
				List<int> indiceOnThisLine = new List<int>();
				for (int r = row, c = 0; r >= 0 && c < BoardWidth; --r, ++c) {
					int gridIndex = r * BoardWidth + c;
					indiceOnThisLine.Add(gridIndex);
				}

				indice[indexOnIndice++] = indiceOnThisLine.ToArray();
			}

			for (int col = 1; col < BoardWidth - ADJACENT_COUNT_TO_ELIMINATE + 1; ++col) {
				List<int> indiceOnThisLine = new List<int>();
				for (int r = BoardHeight - 1, c = col; r >= 0 && c < BoardWidth; --r, ++c) {
					int gridIndex = r * BoardWidth + c;
					indiceOnThisLine.Add(gridIndex);
				}
				
				indice[indexOnIndice++] = indiceOnThisLine.ToArray();
			}

			indexOnIndice = 0;
			_traverseIndice[(int)TraverType.DiagonalDown] = indice;

			// diagonalUp
			indice = new int[dimension0][];
			for (int col = BoardWidth - ADJACENT_COUNT_TO_ELIMINATE; col >= 0; --col) {
				List<int> indiceOnThisLine = new List<int>();
				for (int r = 0, c = col; r < BoardHeight && c < BoardWidth; ++r, ++c) {
					int gridIndex = r * BoardWidth + c;
					indiceOnThisLine.Add(gridIndex);
				}
				
				indice[indexOnIndice++] = indiceOnThisLine.ToArray();
			}

			for (int row = 1; row < BoardHeight - ADJACENT_COUNT_TO_ELIMINATE + 1; ++row) {
				List<int> indiceOnThisLine = new List<int>();
				for (int r = row, c = 0; r < BoardHeight && c < BoardWidth; ++r, ++c) {
					int gridIndex = r * BoardWidth + c;
					indiceOnThisLine.Add(gridIndex);
				}
				
				indice[indexOnIndice++] = indiceOnThisLine.ToArray();
			}

			_traverseIndice[(int)TraverType.DiagonalUp] = indice;
		}
	}

	private void updateUI(bool initial = false) {
		_scoreText.setValue(_score);
		_comboText.text = _combo.ToString() + " X";

		if (_nextPawnStateInvalid) {
			_nextPawnBoard.setNextPawns(_nextPawnTypes);
			_nextPawnStateInvalid = false;
		}

		updateButtonState(_backButton, _backwardsChance > 0 && 0 == _lastUsedBackwardsLock, initial);
		updateButtonState(_trashButton, _trashChance > 0, initial);

		_backChanceText.text = _backwardsChance.ToString();
		_trashChanceText.text = _trashChance.ToString();
		_levelText.text = "LEVEL " + _level;
	}

	private void updateButtonState(Button button, bool enabled, bool force = false) {
		if (force) {
			button.animator.ResetTrigger("Disabled");
			button.animator.ResetTrigger("Normal");
			button.enabled = enabled;

			if (!enabled) {
				button.animator.SetTrigger("Disabled");
			} else {
				button.animator.SetTrigger("Normal");
			}
		} else {
			bool preEnabled = button.enabled;
			button.enabled = enabled;
			button.animator.ResetTrigger("Disabled");
			button.animator.ResetTrigger("Normal");

			if (preEnabled && !button.enabled) {
				button.animator.SetTrigger("Disabled");
			} else if (!preEnabled && button.enabled) {
				button.animator.SetTrigger("Normal");
			}
		}
	}

	#endregion

	public GameSaveData saveGame() {
		GameSaveData saveData = new GameSaveData();
		foreach (Pawn pawn in _pawns) {
			GameSaveData.PawnState pawnState = new GameSaveData.PawnState();
			pawnState.type = pawn.type;
			pawnState.gridIndex = pawn.gridIndex;
			pawnState.side = pawn.side;

			saveData.pawns.Add(pawnState);
		}

		saveData.combo = _combo;
		saveData.score = _score;
		saveData.exp = _exp;
		saveData.level = _level;
		saveData.turn = _gameRecord.turns;
		saveData.side = _turn;
		saveData.lastScoredPawnType = _titleLayer.getScorePawnType();
		
		for (int i = 0; i < _nextPawnTypes.Count; ++i) {
			saveData.nextPawns.Add(_nextPawnTypes[i]);
		}

		saveData.trashChance = _trashChance;
		saveData.backwardsChance = _backwardsChance;
		return saveData;
	}

	public void loadGame(GameSaveData saveData) {
		foreach (GameSaveData.PawnState pawnState in saveData.pawns) {
			Vector2 gridPos = _chessBoard.gridIndexToPos(pawnState.gridIndex);
			putPawn((int)gridPos.x, (int)gridPos.y, pawnState.type, pawnState.side, true);
		}

		_combo = saveData.combo;
		_score = saveData.score;
		_exp = saveData.exp;
		_level = saveData.level;
		_expNextLevel = calculateExpNextLevel(_level);
		_turn = saveData.side;
		_trashChance = saveData.trashChance;
		_backwardsChance = saveData.backwardsChance;
		_titleLayer.setScorePawnType(saveData.lastScoredPawnType);

		_gameState = GameState.WaitingPutPawn;

		_newPawnCount = saveData.nextPawns.Count;
		_nextPawnTypes.Clear();
		for (int i = 0; i < _newPawnCount; ++i) {
			_nextPawnTypes.Add(saveData.nextPawns[i]);
		}

		_expBar.setExpRatio(_exp / _expNextLevel);

		_nextPawnStateInvalid = true;
		invalidUI();
		updateScenePawnState(false);
	}

	#region debug
	public bool debug = true;
	void debugPreparePawnTypes() {
		_nextPawnTypes.Clear();
		for (int i = 0; i < 10; ++i) {
			_nextPawnTypes.Add(PawnType.Black);
		}
	}

	void debugSetNextPawnPositions() {
		int[] testGridIndice = new int[]{0, 1, 2, 8, 9, 10, 16, 24, 32, 40};
		//int[] testGridIndice = new int[]{0, 8, 10, 16, 24, 32, 40};
		_gridIndiceForNextPawns.Clear();
		for (int i = 0; i < testGridIndice.Length; ++i) {
			Vector2 gridPos = _chessBoard.gridIndexToPos(testGridIndice[i]);
			_gridIndiceForNextPawns.Add(gridPos);
		}
	}
	#endregion
}
