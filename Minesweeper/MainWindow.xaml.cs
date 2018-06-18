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
        private readonly SolidColorBrush CELL_BACKGROUND         = new SolidColorBrush(Color.FromRgb(0xdd, 0xdd, 0xdd));
        private readonly SolidColorBrush CELL_CLEARED_BACKGROUND = new SolidColorBrush(Color.FromRgb(0xee, 0xee, 0xee));

        private readonly ImageBrush MINE_IMAGE   = new ImageBrush(App.LoadResource<BitmapImage>("mine.png"));
        private readonly ImageBrush FLAG_IMAGE   = new ImageBrush(App.LoadResource<BitmapImage>("flag.png"));
        private readonly ImageBrush HAPPY_IMAGE  = new ImageBrush(App.LoadResource<BitmapImage>("happy.png"));
        private readonly ImageBrush SCARED_IMAGE = new ImageBrush(App.LoadResource<BitmapImage>("scared.png"));
        private readonly ImageBrush COOL_IMAGE   = new ImageBrush(App.LoadResource<BitmapImage>("cool.png"));
        private readonly ImageBrush DEAD_IMAGE   = new ImageBrush(App.LoadResource<BitmapImage>("dead.png"));

        private readonly SolidColorBrush ONE_MINE_COLOR   = new SolidColorBrush(Colors.Blue);
        private readonly SolidColorBrush TWO_MINE_COLOR   = new SolidColorBrush(Colors.Green);
        private readonly SolidColorBrush THREE_MINE_COLOR = new SolidColorBrush(Colors.Red);
        private readonly SolidColorBrush FOUR_MINE_COLOR  = new SolidColorBrush(Colors.Navy);
        private readonly SolidColorBrush FIVE_MINE_COLOR  = new SolidColorBrush(Colors.Maroon);
        private readonly SolidColorBrush SIX_MINE_COLOR   = new SolidColorBrush(Colors.Teal);
        private readonly SolidColorBrush SEVEN_MINE_COLOR = new SolidColorBrush(Colors.Black);
        private readonly SolidColorBrush EIGHT_MINE_COLOR = new SolidColorBrush(Colors.Gray);

        private const string MSG_WIN_TEXT = "You win! Play again?";
        private const string MSG_LOS_TEXT = "Game over! Play again?";
        private const string MSG_WIN_CAP  = "Win!";
        private const string MSG_LOS_CAP  = "Game Over!";

        /// <summary>
        /// newGame is set whenever a new game is started, and invoked on the first cell clear.
        /// It is used to guarantee a safe start (3x3 area) for the player.
        /// The parameters are the x,y coordinate of the first cell clear.
        /// </summary>
        private Action<int, int> newGame = null;

        /// <summary>
        /// Loads resources and sets up event callback functions
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            MINE_IMAGE.Stretch   = Stretch.Uniform;
            FLAG_IMAGE.Stretch   = Stretch.Uniform;
            HAPPY_IMAGE.Stretch  = Stretch.Uniform;
            SCARED_IMAGE.Stretch = Stretch.Uniform;
            COOL_IMAGE.Stretch   = Stretch.Uniform;
            DEAD_IMAGE.Stretch   = Stretch.Uniform;

            App.TimerTick += startTime => TimerText.Text = ((int)(DateTime.Now - startTime).TotalSeconds).ToString();
            App.GameEnd   += StopGame;
            
            statusImage.Background = HAPPY_IMAGE;

            // if the user wants to cancel a mouse press by letting go after moving off the grid,
            // we need to change the status image back to happy (so it's not stuck on scared until the next press)
            MouseLeftButtonUp += (s, e) => { statusImage.Background = HAPPY_IMAGE; };

            // above isn't called if the mouse left the window
            MouseLeave += (s, e) => { statusImage.Background = HAPPY_IMAGE; };
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

            /* setup our rows and columns for cells */
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
                    lbl.Background = CELL_BACKGROUND;
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
            statusImage.Background = HAPPY_IMAGE;
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
                statusImage.Background = COOL_IMAGE;

                text    = MSG_WIN_TEXT;
                caption = MSG_WIN_CAP;
            }
            else
            {
                statusImage.Background = DEAD_IMAGE;

                text    = MSG_LOS_TEXT;
                caption = MSG_LOS_CAP;

                // show user where all the mines were
                foreach (var coord in App.Mines())
                {
                    GetLabelFromCoord(coord).Background = MINE_IMAGE;
                }
            }

            var result = MessageBox.Show(this, text, caption, MessageBoxButton.YesNo);

            // start a new game with the same parameters
            if (result == MessageBoxResult.Yes)
                GameNew(App.CurrentField);
        }

        /// <summary>
        /// Called when the user presses down on the mouse but before releasing.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Cell_MouseLeftDown(object sender, RoutedEventArgs e)
        {
            statusImage.Background = SCARED_IMAGE; 
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

            if (App.TryClearCell(x, y, ref changedCells))
            {
                // we didn't blow up, so let's show the user what changed
                foreach (var cell in changedCells)
                {
                    lbl = GetLabelFromCoord(cell.coordinate);

                    // set the text first, so we don't need to duplicate the line so many times
                    // if the mine count is 0, it will get set to null
                    lbl.Content = cell.neighboringMines.ToString();

                    // yay colors
                    switch (cell.neighboringMines)
                    {
                        case 1:
                            lbl.Foreground = ONE_MINE_COLOR;
                            break;

                        case 2:
                            lbl.Foreground = TWO_MINE_COLOR;
                            break;

                        case 3:
                            lbl.Foreground = THREE_MINE_COLOR;
                            break;

                        case 4:
                            lbl.Foreground = FOUR_MINE_COLOR;
                            break;

                        case 5:
                            lbl.Foreground = FIVE_MINE_COLOR;
                            break;

                        case 6:
                            lbl.Foreground = SIX_MINE_COLOR;
                            break;

                        case 7:
                            lbl.Foreground = SEVEN_MINE_COLOR;
                            break;

                        case 8:
                            lbl.Foreground = EIGHT_MINE_COLOR;
                            break;

                        default:
                            lbl.Content = null;
                            lbl.Foreground = null;
                            break;
                    }
                    
                    lbl.Background = CELL_CLEARED_BACKGROUND;
                }
                
                statusImage.Background = HAPPY_IMAGE;
            }
            else
            {
                // RIP
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

            if (App.ToggleFlagCell(x, y, ref cleared, ref flagsLeft))
                lbl.Background = FLAG_IMAGE;
            else if (!cleared)
                lbl.Background = CELL_BACKGROUND;

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
            newGame = (x, y) => App.StartNewGame(field, x, y);
        }
    }
}
