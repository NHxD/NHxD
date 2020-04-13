namespace NHxD.Formatting
{
	public interface ITokenReplacer
	{
		string Replace(string[] tokens, string[] namespaces);
	}
}
