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
        /* UI resources */
        private readonly SolidColorBrush CELL_BACKGROUND         = new SolidColorBrush(Color.FromRgb(0xdd, 0xdd, 0xdd));
        private readonly SolidColorBrush CELL_CLEARED_BACKGROUND = new SolidColorBrush(Color.FromRgb(0xee, 0xee, 0xee));

        private readonly ImageBrush mineImage = new ImageBrush();
        private readonly ImageBrush flagImage = new ImageBrush();

        public MainWindow()
        {
            InitializeComponent();

            mineImage.ImageSource = App.LoadResource("mine.png", u => new BitmapImage(u));
            flagImage.ImageSource = App.LoadResource("flag.png", u => new BitmapImage(u));

            App.TimerTick += startTime => TimerText.Text = (DateTime.Now - startTime).Seconds.ToString();
        }

        private void SetupMineFieldUI(int width, int height, int mineCount)
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
                    lbl.MinWidth  = 32;
                    lbl.MinHeight = 32;

                    lbl.Background = CELL_BACKGROUND;

                    lbl.HorizontalContentAlignment = HorizontalAlignment.Center;
                    lbl.VerticalContentAlignment   = VerticalAlignment.Center;

                    lbl.MouseLeftButtonUp  += Cell_MouseLeftUp;
                    lbl.MouseRightButtonUp += Cell_MouseRightUp;

                    MineFieldGrid.Children.Add(lbl);
                }
            }

            MinesLeft.Text = mineCount.ToString();
        }

        // User is attempting to clear a cell
        private void Cell_MouseLeftUp(object sender, MouseButtonEventArgs e)
        {
            var label = (Label)sender;

            List<App.ChangedCell> changedCells = new List<App.ChangedCell>();

            if (App.TryClearCell(Grid.GetColumn(label), Grid.GetRow(label), ref changedCells))
            {
                foreach (var cell in changedCells)
                {
                    // if the cell was already flagged, don't touch it
                    if (cell.neighboringMines != App.FLAGGED)
                    {
                        label = GetLabelFromCoord(cell.coordinate);

                        // if the cell has no neighboring mines, leave the count blank
                        label.Content = cell.neighboringMines != 0 ? cell.neighboringMines.ToString() : null;
                        label.Background = CELL_CLEARED_BACKGROUND;
                    }
                }

                if(App.GameWon)
                    MessageBox.Show("You win!");
            }
            else
            {
                var mines = App.Mines();

                foreach (var coord in mines)
                {
                    GetLabelFromCoord(coord).Background = mineImage;
                }

                MessageBox.Show("Game Over!");
            }

            e.Handled = true;
        }

        // User is flagging a cell as a mine
        private void Cell_MouseRightUp(object sender, MouseButtonEventArgs e)
        {
            var lbl = (Label)sender;

            int flagsLeft = 0;

            if (App.ToggleFlagCell(Grid.GetColumn(lbl), Grid.GetRow(lbl), ref flagsLeft))
                lbl.Background = flagImage;
            else
                lbl.Background = CELL_BACKGROUND;

            MinesLeft.Text = flagsLeft.ToString();

            e.Handled = true;
        }

        private Label GetLabelFromCoord(int x, int y)
        {
            return MineFieldGrid.Children.OfType<Label>().Where(lbl => Grid.GetColumn(lbl) == x && Grid.GetRow(lbl) == y).First();
        }

        private Label GetLabelFromCoord(MineField.Coordinate c)
        {
            return GetLabelFromCoord(c.x, c.y);
        }

        private void GameNewEasy_Click(object sender, RoutedEventArgs e)
        {
            var easy = MineField.Easy;

            SetupMineFieldUI(easy.Width, easy.Height, easy.MineCount);
            App.StartNewGame(easy);

            e.Handled = true;
        }

        private void GameNewMedium_Click(object sender, RoutedEventArgs e)
        {
            var medium = MineField.Medium;

            SetupMineFieldUI(medium.Width, medium.Height, medium.MineCount);
            App.StartNewGame(medium);

            e.Handled = true;
        }

        private void GameNewHard_Click(object sender, RoutedEventArgs e)
        {
            var hard = MineField.Hard;

            SetupMineFieldUI(hard.Width, hard.Height, hard.MineCount);
            App.StartNewGame(hard);

            e.Handled = true;
        }

        private void GameExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
            e.Handled = true;
        }
    }
}
