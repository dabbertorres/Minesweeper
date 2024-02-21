using System;
using System.Collections.Generic;
using System.Linq;

namespace Minesweeper
{
    /// <summary>
    /// Represents a grid of <see cref="Cell"/>s, of which a number contain mines.
    /// </summary>
    public class MineField
    {
        /// <summary>
        /// An "easy" difficulty preset. Consists of a 9x9 grid, containing 10 mines.
        /// </summary>
        public static readonly MineField Easy = new MineField(9, 9, 10);

        /// <summary>
        /// A "medium" difficulty preset. Consists of a 16x16 grid, containing 40 mines.
        /// </summary>
        public static readonly MineField Medium = new MineField(16, 16, 40);

        /// <summary>
        /// A "hard" difficulty preset. Consists of a 30x16 grid, containing 100 mines.
        /// </summary>
        public static readonly MineField Hard = new MineField(30, 16, 100);

        /// <summary>
        /// Two dimensional array representing the grid of <see cref="Cell"/>s
        /// </summary>
        private Cell[,] cells;

        /// <summary>
        /// The number of <see cref="Cell"/>s in a row of the <see cref="MineField"/>
        /// </summary>
        public int Width
        {
            get { return cells.GetLength(0); }
        }

        /// <summary>
        /// The number of <see cref="Cell"/>s in a column of the <see cref="MineField"/>
        /// </summary>
        public int Height
        {
            get { return cells.GetLength(1); }
        }

        /// <summary>
        /// The number of mines in the <see cref="MineField"/>
        /// </summary>
        public int MineCount
        {
            get;
            private set;
        }

        /// <summary>
        /// Creates a cleared MineField for simply storing settings.
        /// </summary>
        /// <param name="width">The number of <see cref="Cell"/>s per row</param>
        /// <param name="height">The number of <see cref="Cell"/>s per column</param>
        /// <param name="mineCount">The number of mines to place in the <see cref="MineField"/></param>
        private MineField(int width, int height, int mineCount)
        {
            cells     = new Cell[width, height];
            MineCount = mineCount;
        }

        /// <summary>
        /// Creates a <paramref name="width"/>x<paramref name="height"/> grid of <see cref="Cell"/>s,
        /// and randomly distributes <paramref name="mineCount"/> mines throughout
        /// </summary>
        /// <param name="width">The number of <see cref="Cell"/>s per row</param>
        /// <param name="height">The number of <see cref="Cell"/>s per column</param>
        /// <param name="mineCount">The number of mines to place in the <see cref="MineField"/></param>
        /// <param name="startX">The x coordinate of the starting point</param>
        /// <param name="startY">The y coordinate of the starting point</param>
        public MineField(int width, int height, int mineCount, int startX, int startY)
        {
            cells     = new Cell[width, height];
            MineCount = mineCount;

            // we want mineCount unique coordinates generated. To make sure the same coordinate is not
            // selected more than once, we define the set of possible coordinates to randomly select from
            // removing coordinates as they are selected
            var possibleCoordinates = new List<Coordinate>(width * height);

            // TODO: How can we fill the above List with all of the possible coordinates in this MineField?

            // we don't want the same minefield the same every time
            Random rand = new Random();

            // TODO: How can we select 'mineCount' number of coordinate pairs to be mines?
            //       Remember that we cannot have multiple mines at the same coordinate.
        }

        /// <summary>
        /// Provides access to an enumerator for easy iteration over the neighbors of the <see cref="Cell"/> at (<paramref name="x"/>, <paramref name="y"/>)
        /// </summary>
        /// <remarks>Safely handles out-of-bounds cases, and skips over the <see cref="Cell"/> at (<paramref name="x"/>, <paramref name="y"/>)</remarks>
        /// <param name="x">The 0-based column number of the <see cref="Cell"/> to find the neighbors of</param>
        /// <param name="y">The 0-based row number of the <see cref="Cell"/> to find the neighbors of</param>
        /// <returns>An enumerator for the neighbors of a <see cref="Cell"/></returns>
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

        /// <summary>
        /// Provides access to an enumerator for easy iteration over the neighbors of the <see cref="Cell"/> at (<paramref name="x"/>, <paramref name="y"/>)
        /// </summary>
        /// <remarks>Safely handles out-of-bounds cases, and skips over the <see cref="Cell"/> at <paramref name="c"/></remarks>
        /// <param name="c">The <see cref="Coordinate"/> of a <see cref="Cell"/></param>
        /// <returns>An enumerator for the neighbors of a <see cref="Cell"/></returns>
        public IEnumerable<Coordinate> Neighbors(Coordinate c)
        {
            return Neighbors(c.x, c.y);
        }

        /// <summary>
        /// Provides read access to the <see cref="Cell"/> at (<paramref name="x"/>, <paramref name="y"/>)
        /// </summary>
        /// <remarks>Provides private write access</remarks>
        /// <param name="x">The 0-based column number of a <see cref="Cell"/></param>
        /// <param name="y">The 0-based row number of a <see cref="Cell"/></param>
        /// <returns>The <seealso cref="Cell"/> at (<paramref name="x"/>, <paramref name="y"/>)</returns>
        public Cell this[int x, int y]
        {
            get { return cells[x, y]; }
            private set { cells[x, y] = value; }
        }

        /// <summary>
        /// Provides read access to the <see cref="Cell"/> at <paramref name="c"/>
        /// </summary>
        /// <param name="c">The <see cref="Coordinate"/> of a <see cref="Cell"/></param>
        /// <returns>The <seealso cref="Cell"/> at <paramref name="c"/></returns>
        public Cell this[Coordinate c]
        {
            get { return cells[c.x, c.y]; }
            private set { cells[c.x, c.y] = value; }
        }

        /// <summary>
        /// Attempts to clear the <see cref="Cell"/> at (<paramref name="x"/>, <paramref name="y"/>)
        /// </summary>
        /// <param name="x">The 0-based column number of a <see cref="Cell"/></param>
        /// <param name="y">The 0-based row number of a <see cref="Cell"/></param>
        /// <returns>true if the <see cref="Cell"/> does not contain a mine, otherwise false</returns>
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

        /// <summary>
        /// Attempts to clear the <see cref="Cell"/> at <paramref name="c"/>
        /// </summary>
        /// <param name="c">The <see cref="Coordinate"/> of a <see cref="Cell"/></param>
        /// <returns>true if the <see cref="Cell"/> does not contain a mine, otherwise false</returns>
        public bool Clear(Coordinate c)
        {
            return Clear(c.x, c.y);
        }

        /// <summary>
        /// Places or removes a flag on the <see cref="Cell"/> at (<paramref name="x"/>, <paramref name="y"/>)
        /// </summary>
        /// <param name="x">The 0-based column number of a <see cref="Cell"/></param>
        /// <param name="y">The 0-based row number of a <see cref="Cell"/></param>
        /// <param name="flag">Signifies whether to place (true) or remove (false) a flag at the <see cref="Cell"/></param>
        /// <returns>The new flag status of the <see cref="Cell"/></returns>
        public bool Flag(int x, int y, bool flag = true)
        {
            cells[x, y].Flagged = flag;

            return flag;
        }

        /// <summary>
        /// Places or removes a flag on the <see cref="Cell"/> at <paramref name="c"/>
        /// </summary>
        /// <param name="c">The <see cref="Coordinate"/> of a <see cref="Cell"/></param>
        /// <param name="flag">Signifies whether to place (true) or remove (false) a flag at the <see cref="Cell"/></param>
        /// <returns>The new flag status of the <see cref="Cell"/></returns>
        public bool Flag(Coordinate c, bool flag = true)
        {
            return Flag(c.x, c.y, flag);
        }

        /// <summary>
        /// Flips the <see cref="Cell.Flagged"/> value of the <see cref="Cell"/> at (<paramref name="x"/>, <paramref name="y"/>)
        /// </summary>
        /// <param name="x">The 0-based column number of a <see cref="Cell"/></param>
        /// <param name="y">The 0-based row number of a <see cref="Cell"/></param>
        /// <returns>The new flag status of the <see cref="Cell"/></returns>
        public bool ToggleFlag(int x, int y)
        {
            return Flag(x, y, !cells[x, y].Flagged);
        }

        /// <summary>
        /// Flips the <see cref="Cell.Flagged"/> value of the <see cref="Cell"/> at <paramref name="c"/>
        /// </summary>
        /// <param name="c">The <see cref="Coordinate"/> of a <see cref="Cell"/></param>
        /// <returns>The new flag status of the <see cref="Cell"/></returns>
        public bool ToggleFlag(Coordinate c)
        {
            return ToggleFlag(c.x, c.y);
        }

        /// <summary>
        /// Calculates the number of <see cref="Cell"/>s with a flag
        /// </summary>
        /// <returns>The number of flagged <see cref="Cell"/>s</returns>
        public int FlagsPlaced()
        {
            throw new NotImplementedException("Pick a method for counting placed flags");

            // TODO: Which version do you like better? They have the same result.
            // NOTE: Change the "true" on the next line to "false" to switch implemementation.
#if true
            int placedFlags = (from Cell c in cells
                               where c.Flagged
                               select c).Count();
#else
            int placedFlags = 0;

            for (int i = 0; i < Width; ++i)
            {
                for (int j = 0; j < Height; ++j)
                {
                    if (cells[i, j].Flagged)
                        ++placedFlags;
                }
            }
#endif

            return placedFlags;
        }

        /// <summary>
        /// Calculates the number of mined <see cref="Cell"/>s without a flag</code>
        /// </summary>
        /// <returns>The number of unflagged <see cref="Cell"/>s containing a mine</returns>
        public int MinesLeft()
        {
            throw new NotImplementedException("Pick a method for counting the number of mines left");

            int flaggedMines = 0;

            // TODO: Which version do you like better? They have the same result.
            // NOTE: Change the "true" on the next line to "false" to switch implemementation.
#if true
            flaggedMines = (from Cell c in cells
                            where c.Flagged && c.IsMine
                            select c).Count();
#else
            for (int i = 0; i < Width; ++i)
            {
                for (int j = 0; j < Height; ++j)
                {
                    var c = cells[i, j];
                    if (c.Flagged && c.IsMine)
                        ++flaggedMines;
                }
            }
#endif

            return MineCount - flaggedMines;
        }

        /// <summary>
        /// Calculates the number of <see cref="Cell"/>s that are either uncleared or unflagged
        /// </summary>
        /// <returns>The number of unflagged and uncleared <see cref="Cell"/>s in the <see cref="MineField"/></returns>
        public int CellsLeft()
        {
            throw new NotImplementedException("Pick a method for counting the number of uncleared and unflagged cells");

            // TODO: Which version do you like better? They have the same result.
            // NOTE: Change the "true" on the next line to "false" to switch implemementation.
#if true
            int notCleared = (from Cell c in cells
                              where !c.Cleared && !c.Flagged
                              select c).Count();
#else
            int notCleared = 0;

            for (int i = 0; i < Width; ++i)
            {
                for (int j = 0; j < Height; ++j)
                {
                    var c = cells[i, j];
                    if (!c.Cleared && !c.Flagged)
                        ++notCleared;
                }
            }
#endif

            return notCleared;
        }
    }
}
