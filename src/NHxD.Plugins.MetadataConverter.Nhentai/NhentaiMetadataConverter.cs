using Nhentai;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace NHxD.Plugin.MetadataConverter.Nhentai
{
	public sealed class PluginSettings
	{
		public bool Indent { get; set; } = true;
	}

	public sealed class NhentaiMetadataConverter : IMetadataConverter
	{
		public IPluginInfo Info => new PluginInfo("Nhentai", "Serialize metadata to Nhentai JSON.", "ash", "1.0");
		public string[] Options => new string[] { "indent:bool(true)" };
		public string FileName => "nhentai.json";

		private readonly PluginSettings settings = new PluginSettings();

		public void Initialize(Dictionary<string, string> settingsDictionary)
		{
			string value;

			if (settingsDictionary.TryGetValue("indent", out value))
			{
				bool boolValue;

				if (bool.TryParse(value, out boolValue))
				{
					settings.Indent = boolValue;
				}
			}
		}

		public void Destroy()
		{

		}

		public bool Write(Metadata metadata, out string blob)
		{
			blob = JsonConvert.SerializeObject(metadata, settings.Indent ? Formatting.Indented : Formatting.None);

			return true;
		}
	}
}
