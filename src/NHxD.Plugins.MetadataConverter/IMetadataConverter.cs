using Nhentai;

namespace NHxD.Plugin.MetadataConverter
{
	public interface IMetadataConverter : IPlugin
	{
		string FileName { get; }

		bool Write(Metadata metadata, out string blob);
	}
}
