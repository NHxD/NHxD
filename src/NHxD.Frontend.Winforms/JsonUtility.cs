using Newtonsoft.Json;
using System;
using System.Globalization;
using System.IO;

namespace NHxD.Frontend.Winforms
{
	using Formatting = Newtonsoft.Json.Formatting;

	public static class JsonUtility
	{
		public static bool SaveToFile<T>(T configuration, string path) where T : new()
		{
			return SaveToFile(configuration, path, Formatting.Indented);
		}

		public static bool SaveToFile<T>(T obj, string path, Formatting formatting) where T : new()
		{
			try
			{
				string text = JsonConvert.SerializeObject(obj, formatting);

				Directory.CreateDirectory(Path.GetDirectoryName(path));
				File.WriteAllText(path, text);
			}
			catch (Exception ex)
			{
				if (Program.Logger == null)
				{
					System.Windows.Forms.MessageBox.Show(string.Format(CultureInfo.CurrentUICulture, "An error occurred while saving JSON file: {0}", path));
				}
				Program.Logger?.ErrorLineFormat("An error occurred while saving JSON file: {0}", path);
				Program.Logger?.ErrorLineFormat(ex.ToString());
				//throw;
				return false;
			}

			return true;
		}

		public static T LoadFromFile<T>(string path) where T : class, new()
		{
			try
			{
				if (File.Exists(path))
				{
					string text = File.ReadAllText(path);

					T obj = JsonConvert.DeserializeObject<T>(text);
					
					if (obj != null)
					{
						return obj;
					}
				}
			}
			catch (Exception ex)
			{
				if (Program.Logger == null)
				{
					System.Windows.Forms.MessageBox.Show(string.Format(CultureInfo.CurrentUICulture, "An error occurred while loading JSON file: {0}", path));
				}
				Program.Logger?.ErrorLineFormat("An error occurred while loading JSON file: {0}", path);
				Program.Logger?.ErrorLineFormat(ex.ToString());
				//throw;
			}

			return null;
		}

		public static bool PopulateFromFile<T>(string path, T target) where T : class, new()
		{
			try
			{
				if (!File.Exists(path))
				{
					return false;
				}

				string text = File.ReadAllText(path);

				JsonConvert.PopulateObject(text, target);
			}
			catch (Exception ex)
			{
				if (Program.Logger == null)
				{
					System.Windows.Forms.MessageBox.Show(string.Format(CultureInfo.CurrentUICulture, "An error occurred while populating from JSON file: {0}", path));
				}
				Program.Logger?.ErrorLineFormat("An error occurred while populating from JSON file: {0}", path);
				Program.Logger?.ErrorLineFormat(ex.ToString());
				//throw;
				return false;
			}

			return true;
		}
	}
}
