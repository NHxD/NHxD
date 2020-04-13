using System;
using System.Globalization;
using System.Linq;

namespace NHxD.Formatting.TokenModifiers
{
	public class TagTokenModifier : ITokenModifier
	{
		public const string Namespace = "Tag";

		public TagsModel TagsModel { get; }

		public TagTokenModifier(TagsModel tagsModel) { TagsModel = tagsModel; }

		public bool Mutate(string[] tokens, string[] namespaces, ref string value, ref int tokenIndex)
		{
			string result = null;

			if (namespaces[0].Equals(Namespace, StringComparison.OrdinalIgnoreCase))
			{
				int id;

				if (int.TryParse(value, out id)
					&& TagsModel.AllTags.Any(x => x.Id == id))
				{
					TagInfo tag = TagsModel.AllTags.First(x => x.Id == id);
					
					if (namespaces[1].Equals("Name", StringComparison.OrdinalIgnoreCase))
					{
						result = tag.Name;
					}
					else if (namespaces[1].Equals("Type", StringComparison.OrdinalIgnoreCase))
					{
						result = tag.Type.ToString();
					}
					else if (namespaces[1].Equals("Count", StringComparison.OrdinalIgnoreCase))
					{
						result = tag.Count.ToString(CultureInfo.InvariantCulture);
					}
					else if (namespaces[1].Equals("Url", StringComparison.OrdinalIgnoreCase))
					{
						result = tag.Url;
					}
					else if (namespaces[1].Equals("Id", StringComparison.OrdinalIgnoreCase))
					{
						result = tag.Id.ToString(CultureInfo.InvariantCulture);
					}
				}
			}

			if (result != null)
			{
				value = result;
				return true;
			}

			return false;
		}
	}
}
