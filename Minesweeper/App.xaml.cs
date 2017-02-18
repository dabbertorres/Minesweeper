using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Threading;

namespace Minesweeper
{
    public partial class App : Application
    {
        /// <summary>
        /// Status value indicating a <see cref="Cell"/> is flagged for <see cref="ChangedCell.neighboringMines"/>
        /// </summary>
        public const int FLAGGED = -1;

        /// <summary>
        /// Indicates if the player has lost the game
        /// </summary>
        public static bool GameLost
        {
            get;
            private set;
        }

        /// <summary>
        /// Indicates if the player has won the game
        /// </summary>
        public static bool GameWon
        {
            get;
            private set;
        }

        /// <summary>
        /// Provides access to a callback providing the current time, called every second
        /// </summary>
        public static event Action<DateTime> TimerTick;

        private static MineField currentField;
        private static DispatcherTimer timer;
        private static DateTime startTime;

        /// <summary>
        /// One-time data initialization, since we only ever create one <see cref="App"/> instance!
        /// </summary>
        public App()
        {
            timer = new DispatcherTimer(new TimeSpan(0, 0, 1), DispatcherPriority.Send, (s, e) => TimerTick?.Invoke(startTime), Dispatcher);
            timer.Stop();
        }

        /// <summary>
        /// Tries to clear the <see cref="Cell"/> at (<paramref name="x"/>, <paramref name="y"/>).
        /// </summary>
        /// <param name="x">The 0-based column number of a <see cref="Cell"/></param>
        /// <param name="y">The 0-based row number of a <see cref="Cell"/></param>
        /// <param name="changedCells">The list of <see cref="Cell"/>s affected by this clear action. Data about changed <see cref="Cell"/>s is provided via <see cref="ChangedCell"/>.</param>
        /// <returns>true if the <see cref="Cell"/> at (<paramref name="x"/>, <paramref name="y"/>) is safe to clear (not a mine), false otherwise</returns>
        public static bool TryClearCell(int x, int y, ref List<ChangedCell> changedCells)
        {
            var coord = new Coordinate(x, y);

            if (currentField.Clear(coord))
            {
                changedCells.Add(new ChangedCell(coord, !currentField[x, y].Flagged ? currentField[x, y].NeighboringMines : FLAGGED));

                if (currentField[coord].NeighboringMines == 0)
                    changedCells.AddRange(ClearLonelyNeighbors(coord));

                if (currentField.MinesLeft() == 0 && currentField.CellsLeft() == 0)
                {
                    timer.Stop();
                    GameWon = true;
                }

                return true;
            }
            else
            {
                timer.Stop();
                GameLost = true;

                return false;
            }
        }

        /// <summary>
        /// Toggles the <see cref="Cell.Flagged"/> state of the <see cref="Cell"/> at (<paramref name="x"/>, <paramref name="y"/>).
        /// </summary>
        /// <param name="x">The 0-based column number of a <see cref="Cell"/></param>
        /// <param name="y">The 0-based row number of a <see cref="Cell"/></param>
        /// <param name="flagsLeft">Assigned to the number of unflagged mines left, assuming all placed flags are correct.</param>
        /// <returns>true if the <see cref="Cell"/> at (<paramref name="x"/>, <paramref name="y"/>) if it is now flagged, false otherwise</returns>
        public static bool ToggleFlagCell(int x, int y, ref int flagsLeft)
        {
            if (currentField[x, y].Cleared)
                return false;

            bool flagged = currentField.ToggleFlag(x, y);

            flagsLeft = currentField.MineCount - currentField.FlagsPlaced();

            return flagged;
        }

        /// <summary>
        /// Returns a list of the <see cref="Coordinate"/>s containing a mine.
        /// </summary>
        /// <returns>List of <see cref="Coordinate"/>s containing a mine</returns>
        public static List<Coordinate> Mines()
        {
            var mines = new List<Coordinate>(currentField.MineCount);

            for (int i = 0; i < currentField.Width; ++i)
            {
                for (int j = 0; j < currentField.Height; ++j)
                {
                    if (currentField[i, j].IsMine)
                        mines.Add(new Coordinate(i, j));
                }
            }

            return mines;
        }

        /// <summary>
        /// Resets game-state data for a new game.
        /// </summary>
        /// <param name="field">The <see cref="MineField"/> of the new game.</param>
        public static void StartNewGame(MineField field)
        {
            currentField = field;

            GameLost = false;
            GameWon  = false;

            startTime = DateTime.Now;
            timer.Start();
        }

        /// <summary>
        /// Generic wrapper function for simpler WPF resource loading.
        /// </summary>
        /// <typeparam name="T">The type of resource to load from <paramref name="path"/></typeparam>
        /// <param name="path">The pathname of the resource to load</param>
        /// <param name="constructor">Function wrapping a constructor for <typeparamref name="T"/> that takes a <see cref="Uri"/> as an argument.</param>
        /// <returns>A new instance of <typeparamref name="T"/> from the resource at <paramref name="path"/>.</returns>
        public static T LoadResource<T>(string path, Func<Uri, T> constructor)
        {
            var sb = new StringBuilder();
            sb.Append(@"pack://application:,,,/");
            sb.Append(ResourceAssembly.GetName().Name);
            sb.Append(";component/Resources/");
            sb.Append(path);

            return constructor(new Uri(sb.ToString(), UriKind.Absolute));
        }

        /// <summary>
        /// Clears all adjacent cells with 0 neighboring mines.
        /// </summary>
        /// <remarks>
        /// If the <see cref="Cell"/> at <paramref name="center"/> has 0 neighboring mines, clears all of its neighbors.
        /// Recurses for each neighbor.
        /// </remarks>
        /// <param name="center">The <see cref="Coordinate"/> to start clearing at.</param>
        /// <returns>A <see cref="List{ChangedCell}"/> of all cleared <see cref="Cell"/>s.</returns>
        private static List<ChangedCell> ClearLonelyNeighbors(Coordinate center)
        {
            List<ChangedCell> changedCells = new List<ChangedCell>();

            foreach (var coord in currentField.Neighbors(center))
            {
                var cell = currentField[coord];

                // if (c.x, c.y) has no mines nearby, recurse to get its neighbors too
                if (!cell.Cleared && !cell.Flagged)
                {
                    currentField.Clear(coord);

                    changedCells.Add(new ChangedCell(coord, cell.NeighboringMines));

                    if (cell.NeighboringMines == 0)
                        changedCells.AddRange(ClearLonelyNeighbors(coord));
                }
            }

            return changedCells;
        }
    }
}
