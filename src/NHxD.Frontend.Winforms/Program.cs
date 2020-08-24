using Ash.System.Diagnostics;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace NHxD.Frontend.Winforms
{
	public static class Program
	{
		public static Configuration.StartupSettings StartupSettings { get; private set; }
		public static Configuration.Settings Settings { get; private set; }
		public static Logger Logger { get; private set; }
		public static string ApplicationPath { get; private set; }
		public static string SourcePath { get; private set; }
		public static int InstanceIndex { get; private set; }

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		private static void Main()
		{
			try
			{
				string assemblyLocation = Assembly.GetEntryAssembly().Location;
				string assemblyDirectory = Path.GetDirectoryName(assemblyLocation).Replace('\\', '/');

				ApplicationPath = assemblyDirectory;
				SourcePath = Directory.GetCurrentDirectory();

				Directory.SetCurrentDirectory(ApplicationPath);

				StartupSettings = JsonUtility.LoadFromFile<Configuration.StartupSettings>(assemblyDirectory + "/assets/defaults/startup.json") ?? new Configuration.StartupSettings();

				if (string.IsNullOrEmpty(StartupSettings.SettingsPath))
				{
					StartupSettings.SettingsPath = "{SpecialFolder.MyDocuments}/NHxD/user/";
				}

				{
					PathFormatter pathFormatter = new PathFormatter(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), Directory.GetCurrentDirectory(), null, null, null, false);

					Settings = new Configuration.Settings();
					JsonUtility.PopulateFromFile(pathFormatter.GetPath(StartupSettings.SettingsPath + "/settings.json"), Settings);

					// always override settings paths.
					if (Settings.PathFormatter != null)
					{
						Settings.PathFormatter.Custom["DefaultSettingsPath"] = StartupSettings.DefaultSettingsPath;
						Settings.PathFormatter.Custom["SettingsPath"] = StartupSettings.SettingsPath;
					}
				}

				string currentProcessName = Process.GetCurrentProcess().ProcessName;
				Process[] activeProcesses = Process.GetProcessesByName(currentProcessName);

				if (!Settings.Process.AllowMultipleInstances
					&& activeProcesses.Length > 1)
				{
					IntPtr hWnd = IntPtr.Zero;

					hWnd = activeProcesses[0].MainWindowHandle;
					User32.NativeMethods.ShowWindowAsync(new HandleRef(null, hWnd), User32.NativeMethods.SW_RESTORE);
					User32.NativeMethods.SetForegroundWindow(activeProcesses[0].MainWindowHandle);

					return;
				}

				InstanceIndex = activeProcesses.Length - 1;

				if (Settings.Eula.CheckLegalAge)
				{
					DialogResult dialogResult = MessageBox.Show("If you are under the age of 18 years, or under the age of majority in the location from where you are launching this program, you do not have authorization or permission to use this program or access any of its materials.\r\n\r\nBy clicking on the \"Yes\" button, you agree and certify under penalty of perjury that you are an adult.", "End-User License Agreement", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

					if (dialogResult != DialogResult.Yes)
					{
						return;
					}

					Settings.Eula.CheckLegalAge = false;
				}

				if (Settings.Eula.PleadArtistSupport)
				{
					MessageBox.Show("If you find something you really like, please consider buying a copy to support the artist!", "NHxD", MessageBoxButtons.OK, MessageBoxIcon.Information);

					Settings.Eula.PleadArtistSupport = false;
				}

				if (Settings.Log.Filters != LogFilters.None)
				{
					PathFormatter PathFormatter = new PathFormatter(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), Directory.GetCurrentDirectory(), Settings.PathFormatter.Custom, Settings.PathFormatter, Settings.Lists.Tags.LanguageNames, Settings.PathFormatter.IsEnabled);
					string logPath = Settings.Log.KeepSeparateLogs ? PathFormatter.GetLog(DateTime.Now) : PathFormatter.GetLog();

					if (Settings.Log.Overwrite
						&& File.Exists(logPath))
					{
						File.WriteAllText(logPath, "");
					}

					Logger = new Logger(logPath, Settings.Log.Filters);

					FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);

					Logger.WriteSeparator('=');
					Logger.WriteLineFormat("{0} version {1}", fileVersionInfo.ProductName, fileVersionInfo.FileVersion);
					Logger.WriteLine(DateTime.Now.ToString(CultureInfo.InvariantCulture));
					Logger.WriteSeparator('=');
				}
				else
				{
					Logger = new Logger();
				}

				Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault(false);
				Application.Run(new MainForm());
			}
			catch (Exception ex)
			{
#if DEBUG
				if (MessageBox.Show("Debug?" + Environment.NewLine + Environment.NewLine + ex.ToString(), "Un unhandled exception occured.", MessageBoxButtons.YesNo, MessageBoxIcon.Error) == DialogResult.Yes)
				{
					throw;
				}
#else
				MessageBox.Show(ex.ToString(), "Un unhandled exception occured.", MessageBoxButtons.OK, MessageBoxIcon.Error);
#endif
			}
		}
	}

	internal partial class User32
	{
		private User32() { }

		internal partial class NativeMethods
		{
			private NativeMethods() { }

			internal const Int32 SW_RESTORE = 9;

			[DllImport("user32.dll")]
			[return: MarshalAs(UnmanagedType.Bool)]
			internal static extern Boolean ShowWindowAsync(HandleRef hWnd, Int32 nCmdShow);

			[DllImport("user32.dll")]
			[return: MarshalAs(UnmanagedType.Bool)]
			internal static extern Boolean SetForegroundWindow(IntPtr WindowHandle);
		}
	}
}
