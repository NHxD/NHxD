using System.Text.RegularExpressions;

namespace NHxD.Formatting
{
	public class Formatter : IFormatter
	{
		public static readonly Regex FormatItemRegex = new Regex(@"({([\w!@#$%^&*()_+\-=\[\]|;',.:"" <>?/\\]+)})", RegexOptions.Compiled);
		//public static readonly Regex FormatCustomItemRegex = new Regex(@"({@([\w.,:/\\]+)})", RegexOptions.Compiled);
		//public static readonly Regex PreprocessorDirectiveRegex = new Regex(@"(<\?(.+)\?>)", RegexOptions.Compiled);
		//public static readonly Regex PreprocessorExpressionRegex = new Regex(@"([\w@.,]+)", RegexOptions.Compiled);
		
		public ITokenReplacer[] TokenReplacers { get; }
		public ITokenModifier[] TokenModifiers { get; }

		public Formatter(ITokenReplacer[] tokenReplacers)
		{
			TokenReplacers = tokenReplacers;
			TokenModifiers = new ITokenModifier[] { };
		}

		public Formatter(ITokenReplacer[] tokenReplacers, ITokenModifier[] tokenModifiers)
		{
			TokenReplacers = tokenReplacers;
			TokenModifiers = tokenModifiers;
		}

		public string Format(string format)
		{
			format = FormatItemRegex.Replace(format, new MatchEvaluator(
				(Match match) =>
				{
					string[] tokens = match.Groups[2].Value.Split(new char[] { ':' });
					string[] namespaces = tokens[0].Split(new char[] { '.' });
					string result = null;

					foreach (ITokenReplacer tokenReplacer in TokenReplacers)
					{
						result = tokenReplacer.Replace(tokens, namespaces);

						if (result != null)
						{
							break;
						}
					}

					if (result == null)
					{
						result = match.Value;
					}

					for (var i = 1; i < tokens.Length; ++i)
					{
						string[] modifierNamespaces = tokens[i].Split(new char[] { '.' });

						foreach (ITokenModifier tokenModifier in TokenModifiers)
						{
							if (tokenModifier.Mutate(tokens, modifierNamespaces, ref result, ref i))
							{
								break;
							}
						}
					}

					return result;
				}
			));

			return format;
		}
	}
}
