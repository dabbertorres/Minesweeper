using System;
using System.Linq;

namespace Minesweeper
{
	public class MineField
	{
		private Cell[,] cells;

		public int Width
		{
			get { return cells.GetLength(0); }
		}

		public int Height
		{
			get { return cells.GetLength(1); }
		}

		public int MineCount
		{
			get;
			private set;
		}

		public int Flags
		{
			get;
			private set;
		}

		public MineField(int width, int height, int mineCount)
		{
			cells = new Cell[width, height];
			MineCount = mineCount;
			Flags = 0;

			// randomly place mines
			Random rand = new Random();
			
			for(int i = 0; i < mineCount; ++i)
			{
				int x = rand.Next(width);
				int y = rand.Next(height);

				cells[x, y].IsMine = true;

				// incrememnt neighbors' NeighboringMines
				for(int j = Math.Max(0, y - 1); j <= Math.Min(height - 1, y + 1); ++j)
				{
					for(int k = Math.Max(0, x - 1); k <= Math.Min(width - 1, x + 1); ++k)
					{
						++cells[k, j].NeighboringMines;
					}
				}
			}
		}

		public Cell this[int x, int y]
		{
			get { return cells[x, y]; }
			set { cells[x, y] = value; }
		}

		// returns true if safe (not a mine), false if it is a mine
		public bool Clear(int x, int y)
		{
			cells[x, y].Cleared = true;
			return !cells[x, y].IsMine;
		}

		public bool Flag(int x, int y, bool flag = true)
		{
			if(cells[x, y].Flagged = flag)
				++Flags;
			else
				--Flags;

			return flag;
		}

		public bool ToggleFlag(int x, int y)
		{
			return Flag(x, y, !cells[x, y].Flagged);
		}

		public int CountMinesLeft()
		{
			int flaggedMines = (from Cell c in cells.AsParallel() where c.Flagged && c.IsMine select c).Count();
			return MineCount - flaggedMines;
		}
	}
}
