using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization;

namespace Nhentai
{
	public class Metadata
	{
		[JsonProperty("id")]
		public int Id { get; set; }

		[JsonProperty("media_id"), JsonConverter(typeof(FormatLongAsStringConverter))]
		public long MediaId { get; set; }

		[JsonProperty("title")]
		public Title Title { get; set; }

		[JsonProperty("images")]
		public Images Images { get; set; }

		[JsonProperty("scanlator")]
		public string Scanlator { get; set; }

		[JsonProperty("upload_date")]
		public long UploadDate { get; set; }

		[JsonProperty("tags")]
		public List<Tag> Tags { get; set; }

		[JsonProperty("num_pages")]
		public int NumPages { get; set; }

		[JsonProperty("num_favorites")]
		public int NumFavorites { get; set; }
	}

	public class Images
	{
		[JsonProperty("pages")]
		public List<Image> Pages { get; set; }

		[JsonProperty("cover")]
		public Image Cover { get; set; }

		[JsonProperty("thumbnail")]
		public Image Thumbnail { get; set; }
	}
	
	public class Image
	{
		[JsonProperty("t")]
		public ImageType Type { get; set; }

		[JsonProperty("w")]
		public int Width { get; set; }

		[JsonProperty("h")]
		public int Height { get; set; }


		public string GetFileExtension()
		{
			switch (Type)
			{
				case ImageType.Jpeg:
					return ImageFileExtensions.Jpeg;

				case ImageType.Png:
					return ImageFileExtensions.Png;

				case ImageType.Gif:
					return ImageFileExtensions.Gif;

				default:
					return ImageFileExtensions.None;
			}
		}
	}

	[JsonConverter(typeof(StringEnumConverter))]
	public enum ImageType
	{
		[EnumMember(Value = ImageTypeNames.Jpeg)]
		Jpeg,

		[EnumMember(Value = ImageTypeNames.Png)]
		Png,

		[EnumMember(Value = ImageTypeNames.Gif)]
		Gif,
	}

	public static class ImageTypeNames
	{
		public const string Jpeg = "j";
		public const string Png = "p";
		public const string Gif = "g";
	}

	public static class ImageFileExtensions
	{
		public const string None = "";
		public const string Jpeg = ".jpg";
		public const string Png = ".png";
		public const string Gif = ".gif";
	}

	public class Title
	{
		[JsonProperty("english")]
		public string English { get; set; }

		[JsonProperty("japanese")]
		public string Japanese { get; set; }

		[JsonProperty("pretty")]
		public string Pretty { get; set; }
	}

	public class Tag
	{
		[JsonProperty("id")]
		public int Id { get; set; }

		[JsonProperty("type")]
		public TagType Type { get; set; }

		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("url")]
		public string Url { get; set; }

		[JsonProperty("count")]
		public int Count { get; set; }
	}

	public static class TagTypeNames
	{
		public const string Tag = "tag";
		public const string Character = "character";
		public const string Language = "language";
		public const string Parody = "parody";
		public const string Category = "category";
		public const string Artist = "artist";
		public const string Group = "group";
	}

	[JsonConverter(typeof(StringEnumConverter))]
	public enum TagType
	{
		[EnumMember(Value = TagTypeNames.Tag)]
		Tag,

		[EnumMember(Value = TagTypeNames.Character)]
		Character,

		[EnumMember(Value = TagTypeNames.Language)]
		Language,

		[EnumMember(Value = TagTypeNames.Parody)]
		Parody,

		[EnumMember(Value = TagTypeNames.Category)]
		Category,

		[EnumMember(Value = TagTypeNames.Artist)]
		Artist,

		[EnumMember(Value = TagTypeNames.Group)]
		Group
	}

	public class SearchResult
	{
		[JsonProperty("result")]
		public List<Metadata> Result { get; set; }

		[JsonProperty("num_pages")]
		public int NumPages { get; set; }

		[JsonProperty("per_page")]
		public int PerPage { get; set; }

		[JsonProperty("error")]
		public bool Error { get; set; }
	}

	internal sealed class FormatLongAsStringConverter : JsonConverter
	{
		public override bool CanRead => true;
		public override bool CanWrite => true;
		public override bool CanConvert(Type type) => type == typeof(long);

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			if (writer == null)
			{
				return;
			}

			long number = (long)value;

			writer.WriteValue(number.ToString(CultureInfo.InvariantCulture));
			//serializer.Serialize(writer, number.ToString(CultureInfo.InvariantCulture));
		}

		public override object ReadJson(JsonReader reader, Type type, object existingValue, JsonSerializer serializer)
		{
			if (reader == null)
			{
				return null;
			}

			JToken jt = JValue.ReadFrom(reader);

			return jt.Value<long>();
		}
	}
}
