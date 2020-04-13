using Nhentai;
using System.Collections.Generic;

namespace NHxD.Plugin.MetadataProcessor
{
	public interface IMetadataProcessor : IPlugin
	{
		bool Run(Metadata metadata, GalleryResourcePaths paths);
	}

	public sealed class GalleryResourcePaths
	{
		public List<string> Metadatas { get; }
		public List<string> Covers { get; }
		public List<string> Pages { get; }
		public List<string> Archives { get; }

		public GalleryResourcePaths(List<string> metadatas, List<string> covers, List<string> pages, List<string> archives)
		{
			Metadatas = metadatas;
			Covers = covers;
			Pages = pages;
			Archives = archives;
		}
	}
}
