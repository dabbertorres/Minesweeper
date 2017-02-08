using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Threading;

namespace Minesweeper
{
    public partial class App : Application
    {
        public struct ChangedCell
        {
            public ChangedCell(MineField.Coordinate coord, int mines)
            {
                coordinate = coord;
                neighboringMines = mines;
            }

            public MineField.Coordinate coordinate;
            public int neighboringMines;
        }

        public const int FLAGGED = -1;

        public static bool GameLost
        {
            get;
            private set;
        }

        public static bool GameWon
        {
            get;
            private set;
        }

        public static event Action<DateTime> TimerTick;

        private static MineField currentField;
        private static DispatcherTimer timer;
        private static DateTime startTime;

        // never-changing data initialization
        public App()
        {
            timer = new DispatcherTimer(new TimeSpan(0, 0, 1), DispatcherPriority.Send, (s, e) => TimerTick?.Invoke(startTime), Dispatcher);
            timer.Stop();
        }

        // returns true if cell was not a mine, false otherwise
        // fills changedCells with affected cells
        //     if the cell was not a mine, sets int to the number of neighboring mines
        //     but, if cell is flagged, sets neighboringMines to -1
        public static bool TryClearCell(int x, int y, ref List<ChangedCell> changedCells)
        {
            if (!GameLost && !GameWon)
            {
                var coord = new MineField.Coordinate(x, y);

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

            return false;
        }

        // returns true if targeted cell is now flagged, false otherwise
        // assigns flagsLeft to the total number of mines minus the number of flags
        public static bool ToggleFlagCell(int x, int y, ref int flagsLeft)
        {
            if (!GameLost && !GameWon)
            {
                if (currentField[x, y].Cleared)
                    return false;

                bool flagged = currentField.ToggleFlag(x, y);

                flagsLeft = currentField.MineCount - currentField.FlagsPlaced();

                return flagged;
            }

            return false;
        }

        public static List<MineField.Coordinate> Mines()
        {
            var mines = new List<MineField.Coordinate>(currentField.MineCount);

            for (int i = 0; i < currentField.Width; ++i)
            {
                for (int j = 0; j < currentField.Height; ++j)
                {
                    if (currentField[i, j].IsMine)
                        mines.Add(new MineField.Coordinate(i, j));
                }
            }

            return mines;
        }

        // setup game-specific data
        public static void StartNewGame(MineField field)
        {
            currentField = field;

            GameLost = false;
            GameWon  = false;

            startTime = DateTime.Now;
            timer.Start();
        }

        // WPF library specific resource loading, nothing interesting
        public static T LoadResource<T>(string path, Func<Uri, T> constructor)
        {
            var sb = new StringBuilder();
            sb.Append(@"pack://application:,,,/");
            sb.Append(ResourceAssembly.GetName().Name);
            sb.Append(";component/Resources/");
            sb.Append(path);

            return constructor(new Uri(sb.ToString(), UriKind.Absolute));
        }

        // if the cell at (x, y) has no neighboring mines, clear all of it's neighbors
        // if any of those neighbors have no neighboring mines, clear them as well
        // returns all affected cells
        private static List<ChangedCell> ClearLonelyNeighbors(MineField.Coordinate center)
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
