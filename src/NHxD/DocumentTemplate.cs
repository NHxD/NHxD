using System;
using System.IO;

namespace NHxD
{
	public class DocumentTemplate<T>
	{
		private string text;

		public string Text => text;

		public string Name { get; }
		public bool CacheEnabled { get; }
		public IPathFormatter PathFormatter { get; }
		public Func<string, T, string> FormatText { get; }

		public DocumentTemplate(string name, bool cacheEnabled, IPathFormatter pathFormatter, Func<string, T, string> formatText)
		{
			Name = name;
			CacheEnabled = cacheEnabled;
			PathFormatter = pathFormatter;
			FormatText = formatText;
		}

		public string GetFormattedText()
		{
			return GetFormattedText(default(T));
		}

		public string GetFormattedText(T context)
		{
			if (text == null
				|| !CacheEnabled)
			{
				text = LoadFromFile();
			}

			string result = text;

			if (FormatText != null)
			{
				result = FormatText.Invoke(result, context);
			}

			return result;
		}

		public string LoadFromFile()
		{
			string templateFilePath = PathFormatter.GetTemplate(Name);

			if (!File.Exists(templateFilePath))
			{
				return "";
			}

			return File.ReadAllText(templateFilePath);
		}
	}
}
