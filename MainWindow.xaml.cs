using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Minesweeper
{
    public partial class MainWindow : Window
    {
        // UI resources
        private readonly SolidColorBrush cellBackground        = new SolidColorBrush(Color.FromRgb(0xdd, 0xdd, 0xdd));
        private readonly SolidColorBrush cellClearedBackground = new SolidColorBrush(Color.FromRgb(0xee, 0xee, 0xee));

        private readonly ImageBrush mineImage;
        private readonly ImageBrush flagImage;
        private readonly ImageBrush happyImage;
        private readonly ImageBrush scaredImage;
        private readonly ImageBrush coolImage;
        private readonly ImageBrush deadImage;

        private readonly SolidColorBrush[] colors = new SolidColorBrush[]{
            new SolidColorBrush(Colors.Blue),
            new SolidColorBrush(Colors.Green),
            new SolidColorBrush(Colors.Red),
            new SolidColorBrush(Colors.Navy),
            new SolidColorBrush(Colors.Maroon),
            new SolidColorBrush(Colors.Teal),
            new SolidColorBrush(Colors.Black),
            new SolidColorBrush(Colors.Gray),
        };

        private const string msgWinText     = "You win! Play again?";
        private const string msgLossText    = "Game over! Play again?";
        private const string msgWinCaption  = "Win!";
        private const string msgLossCaption = "Game Over!";

        /// <summary>
        /// newGame is set whenever a new game is started, and invoked on the first cell clear.
        /// It is used to guarantee a safe start (3x3 area) for the player.
        /// The parameters are the x,y coordinate of the first cell clear.
        /// </summary>
        private Action<int, int>? newGame = null;

        private App currentApp
        {
            get { return (Application.Current as App)!; }
        }

        /// <summary>
        /// Loads resources and sets up event callback functions
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            mineImage   = new ImageBrush(currentApp.LoadResource<BitmapImage>("mine.png"));
            flagImage   = new ImageBrush(currentApp.LoadResource<BitmapImage>("flag.png"));
            happyImage  = new ImageBrush(currentApp.LoadResource<BitmapImage>("happy.png"));
            scaredImage = new ImageBrush(currentApp.LoadResource<BitmapImage>("scared.png"));
            coolImage   = new ImageBrush(currentApp.LoadResource<BitmapImage>("cool.png"));
            deadImage   = new ImageBrush(currentApp.LoadResource<BitmapImage>("dead.png"));

            mineImage.Stretch   = Stretch.Uniform;
            flagImage.Stretch   = Stretch.Uniform;
            happyImage.Stretch  = Stretch.Uniform;
            scaredImage.Stretch = Stretch.Uniform;
            coolImage.Stretch   = Stretch.Uniform;
            deadImage.Stretch   = Stretch.Uniform;

            currentApp.TimerTick += startTime => TimerText.Text = ((int)(DateTime.Now - startTime).TotalSeconds).ToString();
            currentApp.GameEnd   += StopGame;

            statusImage.Background = happyImage;

            // if the user wants to cancel a mouse press by letting go after moving off the grid,
            // we need to change the status image back to happy (so it's not stuck on scared until the next press)
            MouseLeftButtonUp += (s, e) => { statusImage.Background = happyImage; };

            // above isn't called if the mouse left the window
            MouseLeave += (s, e) => { statusImage.Background = happyImage; };
        }

        /// <summary>
        /// Setup the UI for a new <see cref="MineField"/>
        /// </summary>
        /// <param name="width">Width in <see cref="Cell"/>s of the new <see cref="MineField"/></param>
        /// <param name="height">Height in <see cref="Cell"/>s of the new <see cref="MineField"/></param>
        /// <param name="mineCount">Number of mines in the new <see cref="MineField"/></param>
        private void StartNewGame(int width, int height, int mineCount)
        {
            // clear data from previous game
            MineFieldGrid.Children.Clear();
            MineFieldGrid.RowDefinitions.Clear();
            MineFieldGrid.ColumnDefinitions.Clear();

            // setup our rows and columns for cells
            for (int i = 0; i < height; ++i)
                MineFieldGrid.RowDefinitions.Add(new RowDefinition());

            for (int i = 0; i < width; ++i)
                MineFieldGrid.ColumnDefinitions.Add(new ColumnDefinition());

            // create each cell
            // set style data
            // add event handlers
            for (int i = 0; i < width; ++i)
            {
                for (int j = 0; j < height; ++j)
                {
                    var lbl = new Label();

                    Grid.SetColumn(lbl, i);
                    Grid.SetRow(lbl, j);

                    // 32x32 cells looks decent
                    lbl.MinWidth   = 32;
                    lbl.MinHeight  = 32;
                    lbl.Background = cellBackground;
                    lbl.FontSize   = 16;
                    lbl.FontWeight = FontWeights.UltraBold;

                    lbl.HorizontalContentAlignment = HorizontalAlignment.Center;
                    lbl.VerticalContentAlignment   = VerticalAlignment.Center;

                    lbl.MouseLeftButtonDown += Cell_MouseLeftDown;
                    lbl.MouseLeftButtonUp   += Cell_MouseLeftUp;
                    lbl.MouseRightButtonUp  += Cell_MouseRightUp;

                    MineFieldGrid.Children.Add(lbl);
                }
            }

            MinesLeft.Text         = mineCount.ToString();
            statusImage.Background = happyImage;
        }

        /// <summary>
        /// Detach callbacks from events to prevent the user from modifying the field after winning/losing
        /// </summary>
        private void StopGame(bool won)
        {
            foreach (Label lbl in MineFieldGrid.Children)
            {
                lbl.MouseLeftButtonUp  -= Cell_MouseLeftUp;
                lbl.MouseRightButtonUp -= Cell_MouseRightUp;
            }

            string text;
            string caption;

            if (won)
            {
                throw new NotImplementedException("Inform win");
            }
            else
            {
                throw new NotImplementedException("Inform loss");
            }

            var result = MessageBox.Show(this, text, caption, MessageBoxButton.YesNo);

            // start a new game with the same parameters
            if (result == MessageBoxResult.Yes)
                GameNew(currentApp.CurrentField);
        }

        /// <summary>
        /// Called when the user presses down on the mouse but before releasing.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Cell_MouseLeftDown(object sender, RoutedEventArgs e)
        {
            statusImage.Background = scaredImage;
        }

        /// <summary>
        /// Called when the user attempts to clear a <see cref="Cell"/>.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Cell_MouseLeftUp(object sender, RoutedEventArgs e)
        {
            var lbl = (Label)sender;

            int x = Grid.GetColumn(lbl);
            int y = Grid.GetRow(lbl);

            if (newGame != null)
            {
                newGame.Invoke(x, y);
                newGame = null;
            }

            var changedCells = new List<ChangedCell>();

            if (currentApp.TryClearCell(x, y, ref changedCells))
            {
                // we didn't blow up, so let's show the user what changed
                foreach (var cell in changedCells)
                {
                    throw new NotImplementedException("Update changed cells");

                    // TODO: When do we NOT want to change a Cell?

                    // TODO: How do we get the UI element at a specific coordinate?

                    // yay colors
                    if (false /* TODO */)
                    {
                        lbl.Content = cell.neighboringMines.ToString();
                        lbl.Foreground = colors[cell.neighboringMines - 1];
                    }
                    else
                    {
                        lbl.Content = null;
                        lbl.Foreground = null;
                    }

                    lbl.Background = cellClearedBackground;
                }

                statusImage.Background = happyImage;
            }
            else
            {
                // RIP
                // TODO: Why do we not need to do anything here?
            }

            e.Handled = true;
        }

        /// <summary>
        /// Called when the user flags a <see cref="Cell"/>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Cell_MouseRightUp(object sender, MouseButtonEventArgs e)
        {
            var lbl = (Label)sender;

            int x = Grid.GetColumn(lbl);
            int y = Grid.GetRow(lbl);

            int flagsLeft = 0;

            bool cleared = false;

            if (currentApp.ToggleFlagCell(x, y, ref cleared, ref flagsLeft))
                lbl.Background = flagImage;
            else if (!cleared)
                lbl.Background = cellBackground;

            MinesLeft.Text = flagsLeft.ToString();

            e.Handled = true;
        }

        /// <summary>
        /// Helper function to get the UI element at (<paramref name="x"/>, <paramref name="y"/>).
        /// </summary>
        /// <param name="x">The 0-based column number of a <see cref="Cell"/></param>
        /// <param name="y">The 0-based row number of a <see cref="Cell"/></param>
        /// <returns>The <see cref="Label"/> representing the <see cref="Cell"/> at (<paramref name="x"/>, <paramref name="y"/>)</returns>
        private Label GetLabelFromCoord(int x, int y)
        {
            return (from Label lbl in MineFieldGrid.Children
                    where Grid.GetColumn(lbl) == x && Grid.GetRow(lbl) == y
                    select lbl).First();
        }

        /// <summary>
        /// Overload for <see cref="GetLabelFromCoord(int, int)"/>
        /// </summary>
        /// <param name="c">The <see cref="Coordinate"/> of a <see cref="Cell"/></param>
        /// <returns></returns>
        private Label GetLabelFromCoord(Coordinate c)
        {
            return GetLabelFromCoord(c.x, c.y);
        }

        /// <summary>
        /// Called when the user chooses to start a new easy game
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GameNewEasy_Click(object sender, RoutedEventArgs e)
        {
            GameNew(MineField.Easy);
            e.Handled = true;
        }

        /// <summary>
        /// Called when the user chooses to start a new medium game
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GameNewMedium_Click(object sender, RoutedEventArgs e)
        {
            GameNew(MineField.Medium);
            e.Handled = true;
        }

        /// <summary>
        /// Called when the user chooses to start a new hard game
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GameNewHard_Click(object sender, RoutedEventArgs e)
        {
            GameNew(MineField.Hard);
            e.Handled = true;
        }

        /// <summary>
        /// Called when the user gives up (exits)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GameExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
            e.Handled = true;
        }

        private void GameNew(MineField field)
        {
            StartNewGame(field.Width, field.Height, field.MineCount);
            newGame = (x, y) => currentApp.StartNewGame(field, x, y);
        }
    }
}
