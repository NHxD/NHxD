using System.Collections.Generic;

namespace NHxD.Plugin
{
	public interface IPlugin
	{
		IPluginInfo Info { get; }
		string[] Options { get; }

		void Initialize(Dictionary<string, string> settingsDictionary);
		void Destroy();
	}

	public interface IPluginInfo
	{
		string Name { get; }
		string Description { get; }
		string Author { get; }
		string Version { get; }
	}

	public class PluginInfo : IPluginInfo
	{
		public string Name { get; }
		public string Description { get; }
		public string Author { get; }
		public string Version { get; }

		public PluginInfo(string name, string description, string author, string version)
		{
			Name = name;
			Description = description;
			Author = author;
			Version = version;
		}
	}
}
