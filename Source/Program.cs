using System;
using System.Windows.Forms;

namespace Quadris.Source;

public static class Program
{
	[STAThread]
	public static void Main()
	{
		Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
		using var quadris = new Quadris();
		quadris.Run();
	}
}
