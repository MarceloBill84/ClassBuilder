using System.Text;
using System.Text.RegularExpressions;

namespace ClassBuilder.Factory
{
	public static class BuilderFactory
	{
		public static string Create(string fileContent)
		{
			var content = new StringBuilder();

			content.Append(GetUsings(fileContent));

			fileContent = fileContent.Substring(content.Length);

			content.Append(GetNameSpace(fileContent));

			content.Append("{\n");

			var className = GetClassName(fileContent);

			content.AppendLine(string.Concat("\tpublic class ", className));

			content.AppendLine("\t{");

			//GetProperties

			content.AppendLine("\t}");

			content.AppendLine("}");

			return content.ToString();
		}

		private static string GetNameSpace(string fileContent)
		{
			return fileContent.Substring(fileContent.IndexOf("namespace"), fileContent.IndexOf("{"));
		}

		private static string GetUsings(string fileContent)
		{
			return fileContent.Substring(0, fileContent.IndexOf("namespace"));
		}

		private static string GetClassName(string fileContent)
		{
			var teste = Regex.Match(fileContent, @"\s+(class)\s+(?<Name>[^\s]+)");

			return string.Concat(teste.Groups["Name"].Value, "Builder");
		}
	}
}
