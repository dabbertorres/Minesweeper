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
        /// Function type for when the current game ends
        /// </summary>
        /// <param name="won">true if user won, false if user lost</param>
        public delegate void GameEndCallback(bool won);

        /// <summary>
        /// Provides access to a event when the game ends. 
        /// </summary>
        public static event GameEndCallback GameEnd;

        /// <summary>
        /// Provides access to a event providing the current time, called every second
        /// </summary>
        public static event Action<DateTime> TimerTick;

        public static MineField CurrentField
        {
            get;
            private set;
        }

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

            if (CurrentField.Clear(coord))
            {
                var cell = CurrentField[coord];

                throw new NotImplementedException("Report changed cells");

                // TODO: What do we do if a cell is safe to clear?
                //       What if the user tries to clear a cell that is flagged?
                //       Does it affect any neighbors at all? If so, under what conditions?

                CheckGameWon();

                return true;
            }
            else
            {
                timer.Stop();
                GameEnd?.Invoke(false);

                return false;
            }
        }

        /// <summary>
        /// Toggles the <see cref="Cell.Flagged"/> state of the <see cref="Cell"/> at (<paramref name="x"/>, <paramref name="y"/>).
        /// </summary>
        /// <param name="x">The 0-based column number of a <see cref="Cell"/></param>
        /// <param name="y">The 0-based row number of a <see cref="Cell"/></param>
        /// <param name="cleared">Assigned to true if the <see cref="Cell"/> at (<paramref name="x"/>, <paramref name="y"/>) is already cleared</param>
        /// <param name="flagsLeft">Assigned to the number of unflagged mines left, assuming all placed flags are correct.</param>
        /// <returns>true if the <see cref="Cell"/> at (<paramref name="x"/>, <paramref name="y"/>) if it is now flagged, false otherwise</returns>
        public static bool ToggleFlagCell(int x, int y, ref bool cleared, ref int flagsLeft)
        {
            // TODO: What do we do if the user tries to flag a cleared cell?

            bool flagged = CurrentField.ToggleFlag(x, y);

            flagsLeft = CurrentField.MineCount - CurrentField.FlagsPlaced();

            CheckGameWon();

            return flagged;
        }

        /// <summary>
        /// Returns a list of the <see cref="Coordinate"/>s containing a mine.
        /// </summary>
        /// <returns>List of <see cref="Coordinate"/>s containing a mine</returns>
        public static List<Coordinate> Mines()
        {
            var mines = new List<Coordinate>(CurrentField.MineCount);

            // TODO: How can we get the coordinate of every Cell containing a mine?

            throw new NotImplementedException("Getting all coordionates containing mines.");

            return mines;
        }

        /// <summary>
        /// Resets game-state data for a new game.
        /// </summary>
        /// <param name="width">The width of the new field</param>
        /// <param name="height">The height of the new field</param>
        /// <param name="mineCount">The number of mines in the new field</param>
        /// <param name="startX">The x coordinate of the starting point</param>
        /// <param name="startY">The y coordinate of the starting point</param>
        public static void StartNewGame(MineField field, int startX, int startY)
        {
            CurrentField = new MineField(field.Width, field.Height, field.MineCount, startX, startY);
            startTime    = DateTime.Now;
            timer.Start();
        }

        /// <summary>
        /// Checks if the current field has been correctly cleared. Stops timer and sets <see cref="GameWon"/> if so
        /// </summary>
        /// <remarks>
        /// Checks if <see cref="CurrentField"/>'s <see cref="MineField.MinesLeft()"/> == 0 and <see cref="CurrentField"/>'s <see cref="MineField.CellsLeft()"/> == 0
        /// </remarks>
        public static void CheckGameWon()
        {
            if (CurrentField.MinesLeft() == 0 && CurrentField.CellsLeft() == 0)
            {
                timer.Stop();
                GameEnd?.Invoke(true);
            }
        }

        /// <summary>
        /// Generic wrapper function for simpler WPF resource loading.
        /// </summary>
        /// <typeparam name="T">The type of resource to load from <paramref name="path"/></typeparam>
        /// <param name="path">The pathname of the resource to load</param>
        /// <returns>A new instance of <typeparamref name="T"/> from the resource at <paramref name="path"/>.</returns>
        public static T LoadResource<T>(string path) where T : class
        {
            return Activator.CreateInstance(typeof(T), ResourceUri(path)) as T;
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
            return constructor(ResourceUri(path));
        }

        private static Uri ResourceUri(string path)
        {
            var sb = new StringBuilder();
            sb.Append(@"pack://application:,,,/");
            sb.Append(ResourceAssembly.GetName().Name);
            sb.Append(";component/Resources/");
            sb.Append(path);
            return new Uri(sb.ToString(), UriKind.Absolute);
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

            foreach (var coord in CurrentField.Neighbors(center))
            {
                var cell = CurrentField[coord];

                // if (c.x, c.y) has no mines nearby, recurse to get its neighbors too
                if (!cell.Cleared && !cell.Flagged)
                {
                    CurrentField.Clear(coord);

                    changedCells.Add(new ChangedCell(coord, cell.NeighboringMines));

                    if (cell.NeighboringMines == 0)
                        changedCells.AddRange(ClearLonelyNeighbors(coord));
                }
            }

            return changedCells;
        }
    }
}
