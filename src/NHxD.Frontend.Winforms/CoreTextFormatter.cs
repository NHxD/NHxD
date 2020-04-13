using NHxD.Formatting;
using NHxD.Formatting.TokenReplacers;

namespace NHxD.Frontend.Winforms
{
	public class CoreTextFormatter
	{
		public IPathFormatter PathFormatter { get; }
		public ITokenModifier[] TokenModifiers { get; }

		public CoreTextFormatter(IPathFormatter pathFormatter, ITokenModifier[] tokenModifiers)
		{
			PathFormatter = pathFormatter;
			TokenModifiers = tokenModifiers;
		}

		public string Format(string text, object context)
		{
			return new Formatter(new ITokenReplacer[]
				{
					new PathTokenReplacer(PathFormatter)
				},
				TokenModifiers).Format(text);
		}
	}
}
