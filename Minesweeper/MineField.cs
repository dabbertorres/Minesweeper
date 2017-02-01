using System;
using System.Collections.Generic;
using System.Linq;

namespace Minesweeper
{
    public class MineField
    {
        /* provide a couple of presets */
        public static MineField Easy
        {
            get { return new MineField(9, 9, 10); }
        }

        public static MineField Medium
        {
            get { return new MineField(16, 16, 40); }
        }

        public static MineField Hard
        {
            get { return new MineField(30, 16, 100); }
        }

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

        // stores the amount of flags placed
        public int Flags
        {
            get;
            private set;
        }

        // just for convenience in the constructor
        private struct Coordinate
        {
            public Coordinate(int x, int y)
            {
                this.x = x;
                this.y = y;
            }

            public int x;
            public int y;
        }

        public MineField(int width, int height, int mineCount)
        {
            cells = new Cell[width, height];
            MineCount = mineCount;
            Flags = 0;

            // we want mineCount unique coordinates generated. To make sure the same coordinate is not
            // selected more than once, we define the set of possible coordinates to randomly select from
            // removing coordinates as they are selected
            var possibleCoordinates = new List<Coordinate>(width * height);

            // fill our set of coordinates
            for (int x = 0; x < width; ++x)
            {
                for (int y = 0; y < height; ++y)
                {
                    possibleCoordinates.Add(new Coordinate(x, y));
                }
            }

            // we don't want the same minefield the same every time
            Random rand = new Random();

            for (int i = 0; i < mineCount; ++i)
            {
                int coordIdx = rand.Next(possibleCoordinates.Count);

                var coord = possibleCoordinates[coordIdx];

                // we don't want to generate this coordinate again
                possibleCoordinates.RemoveAt(coordIdx);

                // don't recommend stepping foot here
                cells[coord.x, coord.y].IsMine = true;

                // incrememnt neighbors' NeighboringMines value
                for (int j = Math.Max(0, coord.x - 1); j <= Math.Min(width - 1, coord.x + 1); ++j)
                {
                    for (int k = Math.Max(0, coord.y - 1); k <= Math.Min(height - 1, coord.y + 1); ++k)
                    {
                        ++cells[j, k].NeighboringMines;
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
            if (!cells[x, y].Flagged)
            {
                cells[x, y].Cleared = true;
                return !cells[x, y].IsMine;
            }
            else
            {
                return true;
            }
        }

        public bool Flag(int x, int y, bool flag = true)
        {
            cells[x, y].Flagged = flag;

            if (flag)
                ++Flags;
            else
                --Flags;

            return flag;
        }

        public bool ToggleFlag(int x, int y)
        {
            return Flag(x, y, !cells[x, y].Flagged);
        }

        // returns the true amount of mines left
        public int MinesLeft()
        {
            int flaggedMines = (from Cell c in cells
                                where c.Flagged && c.IsMine
                                select c).Count();
            return MineCount - flaggedMines;
        }
    }
}
