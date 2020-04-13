namespace NHxD.Plugin.ArchiveWriter
{
	public interface IArchiveWriter : IPlugin
	{
		string FileExtension { get; }

		bool SupportsCreateEmpty { get; }
		bool SupportsCreateFromDirectory { get; }
		bool SupportsCreateEntryFromFile { get; }

		bool CreateEmpty(string archiveFilePath);
		bool CreateFromDirectory(string directoryPath, string archiveFilePath);
		bool CreateEntryFromFile(string entryName, string entryFilePath, string archiveFilePath);
	}
}
