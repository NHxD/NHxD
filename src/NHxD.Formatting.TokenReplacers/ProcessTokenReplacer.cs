using System;

namespace NHxD.Formatting.TokenReplacers
{
	public class ProcessTokenReplacer : ITokenReplacer
	{
		public const string Namespace = "Process";

		public int InstanceIndex { get; }

		public ProcessTokenReplacer(int instanceIndex)
		{
			InstanceIndex = instanceIndex;
		}

		public string Replace(string[] tokens, string[] namespaces)
		{
			string result = null;

			if (namespaces[0].Equals(Namespace, StringComparison.OrdinalIgnoreCase))
			{
				if (namespaces.Length >= 2)
				{
					if (namespaces[1].Equals("InstanceIndex", StringComparison.OrdinalIgnoreCase))
					{
						result = InstanceIndex.ToString();
					}
				}
			}

			return result;
		}
	}
}
