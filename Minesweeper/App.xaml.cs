using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Minesweeper
{
	public partial class App : Application
	{
		public static MineField Easy
		{
			get { return new MineField(9, 9, 10); }
		}

		public static MineField Medium
		{
			get { return new MineField(16, 16, 40); }
		}

		public static MineField Hard
		{
			get { return new MineField(30, 16, 100); }
		}

		public static new App Current
		{
			get { return (App)Application.Current; }
		}

		public MineField CurrentGame
		{
			get;
			set;
		}

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