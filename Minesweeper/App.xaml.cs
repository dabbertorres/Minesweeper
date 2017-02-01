using System;
using System.Text;
using System.Windows;
using System.Windows.Threading;

namespace Minesweeper
{
    public partial class App : Application
    {
        public static bool GameOver
        {
            get;
            private set;
        }

        public static event Action<DateTime> TimerTick;
        
        private static MineField currentField;
        private static DispatcherTimer timer;
        private static DateTime startTime;
        private static bool firstClick;

        // never-changing data initialization
        public App()
        {
            timer = new DispatcherTimer(new TimeSpan(0, 0, 1), DispatcherPriority.Send, (s, e) => TimerTick?.Invoke(startTime), Dispatcher);
            timer.IsEnabled = false;
        }

        // returns true if cell was not a mine, false otherwise
        // if the cell was not a mine, sets neighboringMines
        //      but, if the cell was already flagged, sets neighboringMines to -1
        public static bool TryClearCell(int x, int y, ref int neighboringMines)
        {
            if (!GameOver)
            {
                if (currentField.Clear(x, y))
                {
                    if (firstClick)
                    {
                        firstClick = false;

                        // TODO: clear neighbors of adjacent cells with zero neighboring mines
                    }

                    if (!currentField[x, y].Flagged)
                        neighboringMines = currentField[x, y].NeighboringMines;
                    else
                        neighboringMines = -1;

                    return true;
                }
                else
                {
                    timer.Stop();
                    GameOver = true;

                    return false;
                }
            }

            return false;
        }

        // returns true if targeted cell is now flagged, false otherwise
        // assigns flagsLeft to the total number of mines minus the number of flags
        public static bool ToggleFlagCell(int x, int y, ref int flagsLeft)
        {
            if (!GameOver)
            {
                bool flagged = currentField.ToggleFlag(x, y);

                flagsLeft = currentField.MineCount - currentField.Flags;

                return flagged;
            }

            return false;
        }

        // setup game-specific data
        public static void StartNewGame(MineField field)
        {
            currentField = field;

            GameOver   = false;
            firstClick = true;

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
    }
}