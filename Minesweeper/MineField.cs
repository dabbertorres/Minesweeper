using System;
using System.Collections.Generic;
using System.Linq;

namespace Minesweeper
{
    public class MineField
    {
        // convenience struct for passing coordinates around
        public struct Coordinate
        {
            public Coordinate(int x, int y)
            {
                this.x = x;
                this.y = y;
            }

            public int x;
            public int y;
        }

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

        public MineField(int width, int height, int mineCount)
        {
            cells = new Cell[width, height];
            MineCount = mineCount;

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

                foreach (var c in Neighbors(coord.x, coord.y))
                {
                    ++cells[c.x, c.y].NeighboringMines;
                }
            }
        }

        // returns each neighboring Cell to (x, y)
        public IEnumerable<Coordinate> Neighbors(int x, int y)
        {
            for (int i = Math.Max(0, x - 1); i <= Math.Min(Width - 1, x + 1); ++i)
            {
                for (int j = Math.Max(0, y - 1); j <= Math.Min(Height - 1, y + 1); ++j)
                {
                    if (!(i == x && j == y))
                        yield return new Coordinate(i, j);
                    else
                        continue;
                }
            }
        }

        public IEnumerable<Coordinate> Neighbors(Coordinate c)
        {
            return Neighbors(c.x, c.y);
        }

        public Cell this[int x, int y]
        {
            get { return cells[x, y]; }
            set { cells[x, y] = value; }
        }

        public Cell this[Coordinate c]
        {
            get { return cells[c.x, c.y]; }
            set { cells[c.x, c.y] = value; }
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

        public bool Clear(Coordinate c)
        {
            return Clear(c.x, c.y);
        }

        public bool Flag(int x, int y, bool flag = true)
        {
            cells[x, y].Flagged = flag;

            return flag;
        }

        public bool Flag(Coordinate c, bool flag = true)
        {
            return Flag(c.x, c.y, flag);
        }

        public bool ToggleFlag(int x, int y)
        {
            return Flag(x, y, !cells[x, y].Flagged);
        }

        public bool ToggleFlag(Coordinate c)
        {
            return ToggleFlag(c.x, c.y);
        }

        public int FlagsPlaced()
        {
            int placed = (from Cell c in cells
                          where c.Flagged
                          select c).Count();

            return placed;
        }

        // returns the true amount of mines left
        public int MinesLeft()
        {
            int flaggedMines = (from Cell c in cells
                                where c.Flagged && c.IsMine
                                select c).Count();
            return MineCount - flaggedMines;
        }

        public int CellsLeft()
        {
            int notCleared = (from Cell c in cells
                              where !c.Cleared && !c.Flagged
                              select c).Count();

            return notCleared;
        }
    }
}
