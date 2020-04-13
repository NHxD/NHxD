using NHxD.Formatting;
using NHxD.Formatting.TokenModifiers;
using NHxD.Formatting.TokenReplacers;
using System.Diagnostics;
using System.Reflection;

namespace NHxD.Frontend.Winforms
{
	public class AboutTextFormatter
	{
		public IPathFormatter PathFormatter { get; }

		public AboutTextFormatter(IPathFormatter pathFormatter)
		{
			PathFormatter = pathFormatter;
		}

		public string Format(string text)
		{
			return new Formatter(new ITokenReplacer[]
			{
				new PathTokenReplacer(PathFormatter),
				new BackgroundTokenReplacer(PathFormatter),
				new FileVersionInfoTokenReplacer(FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location))
			}, TokenModifiers.All).Format(text);
		}
	}
}
