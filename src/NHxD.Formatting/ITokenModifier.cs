namespace NHxD.Formatting
{
	public interface ITokenModifier
	{
		bool Mutate(string[] tokens, string[] namespaces, ref string value, ref int tokenIndex);
	}
}
