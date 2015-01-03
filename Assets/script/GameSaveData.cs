using System;
using System.Collections.Generic;

public class GameSaveData {
	public class PawnState {
		public PawnType type;
		public int gridIndex;
		public MainState.Side side;
	}

	public List<PawnState> pawns = new List<PawnState>();
	public List<PawnType> nextPawns = new List<PawnType>();
	public MainState.Side side {
		get; set;
	}

	public int turn {
		get; set;
	}

	public int score {
		get; set;
	}

	public int combo {
		get; set;
	}

	public int exp {
		get; set;
	}

	public int level {
		get; set;
	}

	public int trashChance {
		get; set;
	}

	public int backwardsChance {
		get; set;
	}

	public PawnType lastScoredPawnType {
		get; set;
	}
}
