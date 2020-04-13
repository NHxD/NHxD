using NHxD.Formatting;
using NHxD.Formatting.TokenModifiers;
using NHxD.Formatting.TokenReplacers;

namespace NHxD.Frontend.Winforms
{
	public class TagTextFormatter
	{
		public string Format(TagInfo tag, string text)
		{
			return new Formatter(new ITokenReplacer[]
			{
				new TagTokenReplacer(tag)
			}, TokenModifiers.All).Format(text);
		}
	}
}
