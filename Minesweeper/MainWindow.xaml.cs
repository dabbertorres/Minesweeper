using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Minesweeper
{
	public partial class MainWindow : Window
	{
		public static Key GameExit_Key
		{
			get { return Key.Escape; }
		}

		public static string GameExit_KeyStr
		{
			get { return GameExit_Key.ToString(); }
		}

		private BitmapSource mineImage;
		private BitmapSource flagImage;

		private SolidColorBrush cellBackground = new SolidColorBrush(Color.FromArgb(0xff, 0xdd, 0xdd, 0xdd));
		private SolidColorBrush clearedCellBackground = new SolidColorBrush(Color.FromArgb(0xff, 0xee, 0xee, 0xee));

		// time the current game was started
		private DateTime startTime;
		private bool gameOver = false;
		private bool firstClick = true;

		private DispatcherTimer timer;

		public MainWindow()
		{
			InitializeComponent();

			mineImage = App.LoadResource("mine.png", u => new BitmapImage(u));
			flagImage = App.LoadResource("flag.png", u => new BitmapImage(u));

			timer = new DispatcherTimer(new TimeSpan(0, 0, 1), DispatcherPriority.Send, (o, e) => TimerText.Text = (DateTime.Now - startTime).Seconds.ToString(), Dispatcher);
			timer.Stop();
		}

		private void SetupMineFieldUI()
		{
			var field = App.Current.CurrentGame;

			for(int i = 0; i < field.Height; ++i)
				MineFieldGrid.RowDefinitions.Add(new RowDefinition());

			for(int i = 0; i < field.Width; ++i)
				MineFieldGrid.ColumnDefinitions.Add(new ColumnDefinition());

			for(int i = 0; i < field.Height; ++i)
			{
				for(int j = 0; j < field.Width; ++j)
				{
					var lbl = new Label();
					Grid.SetRow(lbl, i);
					Grid.SetColumn(lbl, j);
					lbl.MinWidth = 32;
					lbl.MinHeight = 32;
					lbl.Background = cellBackground;
					lbl.HorizontalContentAlignment = HorizontalAlignment.Center;
					lbl.VerticalContentAlignment = VerticalAlignment.Center;

					lbl.MouseLeftButtonUp += Cell_MouseUp;
					lbl.MouseRightButtonUp += Cell_MouseUp;

					MineFieldGrid.Children.Add(lbl);
				}
			}

			MinesLeft.Text = field.MineCount.ToString();

			startTime = DateTime.Now;
			timer.Start();

			gameOver = false;
			firstClick = true;
		}

		private void Cell_MouseUp(object sender, MouseButtonEventArgs e)
		{
			if(!gameOver)
			{
				var game = App.Current.CurrentGame;

				var lbl = (Label)sender;

				int x = Grid.GetRow(lbl);
				int y = Grid.GetColumn(lbl);

				switch(e.ChangedButton)
				{
					case MouseButton.Left:
						e.Handled = true;

						if(game.Clear(x, y))
						{
							lbl.Content = game[x, y].NeighboringMines.ToString();
							lbl.Background = clearedCellBackground;
							
							if(firstClick)
							{
								// clear all adjacent cells that are not mines
								firstClick = false;
							}
						}
						else
						{
							MessageBox.Show("Game Over!");
							timer.Stop();
							gameOver = true;
						}
						break;

					case MouseButton.Right:
						e.Handled = true;

						if(!game[x, y].Cleared)
						{
							if(game.ToggleFlag(x, y))
								lbl.Background = new ImageBrush(flagImage);
							else
								lbl.Background = cellBackground;
						}

						MinesLeft.Text = (game.MineCount - game.Flags).ToString();
						break;

					case MouseButton.Middle:
						// clear all adjacent cells with zero neighboring mines
						e.Handled = true;
						break;

					default:
						break;
				}
			}
		}

		private void window_KeyUp(object sender, KeyEventArgs e)
		{
			if(e.Key == Key.Escape)
			{
				Close();
				e.Handled = true;
			}
		}

		private void GameNewEasy_Click(object sender, RoutedEventArgs e)
		{
			App.Current.CurrentGame = App.Easy;
			SetupMineFieldUI();
			e.Handled = true;
		}

		private void GameNewMedium_Click(object sender, RoutedEventArgs e)
		{
			App.Current.CurrentGame = App.Medium;
			SetupMineFieldUI();
			e.Handled = true;
		}

		private void GameNewHard_Click(object sender, RoutedEventArgs e)
		{
			App.Current.CurrentGame = App.Hard;
			SetupMineFieldUI();
			e.Handled = true;
		}

		private void GameExit_Click(object sender, RoutedEventArgs e)
		{
			Close();
			e.Handled = true;
		}
	}
}
