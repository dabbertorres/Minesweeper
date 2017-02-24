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
        private readonly SolidColorBrush CELL_BACKGROUND = new SolidColorBrush(Color.FromRgb(0xdd, 0xdd, 0xdd));
        private readonly SolidColorBrush CELL_CLEARED_BACKGROUND = new SolidColorBrush(Color.FromRgb(0xee, 0xee, 0xee));

        private readonly ImageBrush mineImage = new ImageBrush();
        private readonly ImageBrush flagImage = new ImageBrush();

        /// <summary>
        /// Loads resources and sets up event callback functions
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            mineImage.ImageSource = App.LoadResource("mine.png", u => new BitmapImage(u));
            flagImage.ImageSource = App.LoadResource("flag.png", u => new BitmapImage(u));

            App.TimerTick += startTime => TimerText.Text = ((int)(DateTime.Now - startTime).TotalSeconds).ToString();
            App.GameEnd += StopGame;
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
                    lbl.MinWidth = 32;
                    lbl.MinHeight = 32;

                    lbl.Background = CELL_BACKGROUND;

                    lbl.HorizontalContentAlignment = HorizontalAlignment.Center;
                    lbl.VerticalContentAlignment = VerticalAlignment.Center;

                    lbl.MouseLeftButtonUp += Cell_MouseLeftUp;
                    lbl.MouseRightButtonUp += Cell_MouseRightUp;

                    MineFieldGrid.Children.Add(lbl);
                }
            }

            MinesLeft.Text = mineCount.ToString();
        }

        /// <summary>
        /// Detach callbacks from events to prevent the user from modifying the field after winning/losing
        /// </summary>
        private void StopGame(bool won)
        {
            foreach (Label lbl in MineFieldGrid.Children)
            {
                lbl.MouseLeftButtonUp -= Cell_MouseLeftUp;
                lbl.MouseRightButtonUp -= Cell_MouseRightUp;
            }

            if (won)
            {
                MessageBox.Show("You win!");
            }
            else
            {
                MessageBox.Show("Game Over!");

                var mines = App.Mines();

                // show user where all the mines were
                foreach (var coord in mines)
                {
                    GetLabelFromCoord(coord).Background = mineImage;
                }
            }
        }

        /// <summary>
        /// Called when the user attempts to clear a <see cref="Cell"/>.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Cell_MouseLeftUp(object sender, RoutedEventArgs e)
        {
            var lbl = (Label)sender;

            var changedCells = new List<ChangedCell>();

            if (App.TryClearCell(Grid.GetColumn(lbl), Grid.GetRow(lbl), ref changedCells))
            {
                // we didn't blow up, so let's show the user what changed
                foreach (var cell in changedCells)
                {
                    lbl = GetLabelFromCoord(cell.coordinate);

                    // if the cell has no neighboring mines, leave the count blank
                    lbl.Content = cell.neighboringMines != 0 ? cell.neighboringMines.ToString() : null;
                    lbl.Background = CELL_CLEARED_BACKGROUND;
                }
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
                lbl.Background = flagImage;
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
            var easy = MineField.Easy;

            StartNewGame(easy.Width, easy.Height, easy.MineCount);
            App.StartNewGame(easy);

            e.Handled = true;
        }

        /// <summary>
        /// Called when the user chooses to start a new medium game
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GameNewMedium_Click(object sender, RoutedEventArgs e)
        {
            var medium = MineField.Medium;

            StartNewGame(medium.Width, medium.Height, medium.MineCount);
            App.StartNewGame(medium);

            e.Handled = true;
        }

        /// <summary>
        /// Called when the user chooses to start a new hard game
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GameNewHard_Click(object sender, RoutedEventArgs e)
        {
            var hard = MineField.Hard;

            StartNewGame(hard.Width, hard.Height, hard.MineCount);
            App.StartNewGame(hard);

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
    }
}
