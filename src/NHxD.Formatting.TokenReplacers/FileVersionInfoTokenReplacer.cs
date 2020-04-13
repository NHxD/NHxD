using System;
using System.Diagnostics;
using System.Globalization;

namespace NHxD.Formatting.TokenReplacers
{
	public class FileVersionInfoTokenReplacer : ITokenReplacer
	{
		public const string Namespace = "FileVersionInfo";

		public FileVersionInfo FileVersionInfo { get; }

		public FileVersionInfoTokenReplacer(FileVersionInfo fileVersionInfo)
		{
			FileVersionInfo = fileVersionInfo;
		}

		public string Replace(string[] tokens, string[] namespaces)
		{
			string result = null;

			if (namespaces[0].Equals(Namespace, StringComparison.OrdinalIgnoreCase))
			{
				if (namespaces.Length >= 2)
				{
					if (namespaces[1].Equals("Comments", StringComparison.OrdinalIgnoreCase))
					{
						result = FileVersionInfo.Comments;
					}
					else if (namespaces[1].Equals("CompanyName", StringComparison.OrdinalIgnoreCase))
					{
						result = FileVersionInfo.CompanyName;
					}
					else if (namespaces[1].Equals("FileBuildPart", StringComparison.OrdinalIgnoreCase))
					{
						result = FileVersionInfo.FileBuildPart.ToString(CultureInfo.InvariantCulture);
					}
					else if (namespaces[1].Equals("FileDescription", StringComparison.OrdinalIgnoreCase))
					{
						result = FileVersionInfo.FileDescription;
					}
					else if (namespaces[1].Equals("FileMajorPart", StringComparison.OrdinalIgnoreCase))
					{
						result = FileVersionInfo.FileMajorPart.ToString(CultureInfo.InvariantCulture);
					}
					else if (namespaces[1].Equals("FileMinorPart", StringComparison.OrdinalIgnoreCase))
					{
						result = FileVersionInfo.FileMinorPart.ToString(CultureInfo.InvariantCulture);
					}
					else if (namespaces[1].Equals("FileName", StringComparison.OrdinalIgnoreCase))
					{
						result = FileVersionInfo.FileName;
					}
					else if (namespaces[1].Equals("FilePrivatePart", StringComparison.OrdinalIgnoreCase))
					{
						result = FileVersionInfo.FilePrivatePart.ToString(CultureInfo.InvariantCulture);
					}
					else if (namespaces[1].Equals("FileVersion", StringComparison.OrdinalIgnoreCase))
					{
						result = FileVersionInfo.FileVersion;
					}
					else if (namespaces[1].Equals("InternalName", StringComparison.OrdinalIgnoreCase))
					{
						result = FileVersionInfo.InternalName;
					}
					else if (namespaces[1].Equals("IsDebug", StringComparison.OrdinalIgnoreCase))
					{
						result = FileVersionInfo.IsDebug.ToString(CultureInfo.InvariantCulture);
					}
					else if (namespaces[1].Equals("IsPatched", StringComparison.OrdinalIgnoreCase))
					{
						result = FileVersionInfo.IsPatched.ToString(CultureInfo.InvariantCulture);
					}
					else if (namespaces[1].Equals("IsPreRelease", StringComparison.OrdinalIgnoreCase))
					{
						result = FileVersionInfo.IsPreRelease.ToString(CultureInfo.InvariantCulture);
					}
					else if (namespaces[1].Equals("IsPrivateBuild", StringComparison.OrdinalIgnoreCase))
					{
						result = FileVersionInfo.IsPrivateBuild.ToString(CultureInfo.InvariantCulture);
					}
					else if (namespaces[1].Equals("IsSpecialBuild", StringComparison.OrdinalIgnoreCase))
					{
						result = FileVersionInfo.IsSpecialBuild.ToString(CultureInfo.InvariantCulture);
					}
					else if (namespaces[1].Equals("Language", StringComparison.OrdinalIgnoreCase))
					{
						result = FileVersionInfo.Language;
					}
					else if (namespaces[1].Equals("LegalCopyright", StringComparison.OrdinalIgnoreCase))
					{
						result = FileVersionInfo.LegalCopyright;
					}
					else if (namespaces[1].Equals("LegalTrademarks", StringComparison.OrdinalIgnoreCase))
					{
						result = FileVersionInfo.LegalTrademarks;
					}
					else if (namespaces[1].Equals("OriginalFilename", StringComparison.OrdinalIgnoreCase))
					{
						result = FileVersionInfo.OriginalFilename;
					}
					else if (namespaces[1].Equals("PrivateBuild", StringComparison.OrdinalIgnoreCase))
					{
						result = FileVersionInfo.PrivateBuild;
					}
					else if (namespaces[1].Equals("ProductBuildPart", StringComparison.OrdinalIgnoreCase))
					{
						result = FileVersionInfo.ProductBuildPart.ToString(CultureInfo.InvariantCulture);
					}
					else if (namespaces[1].Equals("ProductMajorPart", StringComparison.OrdinalIgnoreCase))
					{
						result = FileVersionInfo.ProductMajorPart.ToString(CultureInfo.InvariantCulture);
					}
					else if (namespaces[1].Equals("ProductMinorPart", StringComparison.OrdinalIgnoreCase))
					{
						result = FileVersionInfo.ProductMinorPart.ToString(CultureInfo.InvariantCulture);
					}
					else if (namespaces[1].Equals("ProductName", StringComparison.OrdinalIgnoreCase))
					{
						result = FileVersionInfo.ProductName;
					}
					else if (namespaces[1].Equals("ProductPrivatePart", StringComparison.OrdinalIgnoreCase))
					{
						result = FileVersionInfo.ProductPrivatePart.ToString(CultureInfo.InvariantCulture);
					}
					else if (namespaces[1].Equals("ProductVersion", StringComparison.OrdinalIgnoreCase))
					{
						result = FileVersionInfo.ProductVersion;
					}
					else if (namespaces[1].Equals("SpecialBuild", StringComparison.OrdinalIgnoreCase))
					{
						result = FileVersionInfo.SpecialBuild;
					}
					else if (namespaces[1].Equals("Website", StringComparison.OrdinalIgnoreCase))
					{
						result = "https://github.com/NHxD/NHxD";
					}
				}
			}

			return result;
		}
	}
}
