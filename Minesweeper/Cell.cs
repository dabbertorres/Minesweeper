namespace Minesweeper
{
	public struct Cell
	{
		public bool IsMine
		{
			get;
			set;
		}

		public int NeighboringMines
		{
			get;
			set;
		}

		public bool Flagged
		{
			get;
			set;
		}

		public bool Cleared
		{
			get;
			set;
		}
	}
}
