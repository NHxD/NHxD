namespace NHxD.Formatting.TokenModifiers
{
	public static class TokenModifiers
	{
		private static readonly ITokenModifier[] all =
			new ITokenModifier[]
			{
				new HtmlTokenModifier(),
				new UriTokenModifier(),
				new UrlTokenModifier(),
				new TransformTokenModifier(),
				new EncodingTokenModifier(),
				new CultureTokenModifier(),
				new XmlTokenModifier(),
			};

		public static ITokenModifier[] All => all;
	}
}
