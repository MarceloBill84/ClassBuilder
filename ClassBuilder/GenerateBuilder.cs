using Microsoft.VisualStudio.TextTemplating.VSHost;
using System.Text;

namespace ClassBuilder
{
	public class GenerateBuilder : BaseCodeGenerator
	{
		public const string Name = nameof(GenerateBuilder);
		public const string Description = "Generate Builder";
		public override string GetDefaultExtension() => ".cs";

		protected override byte[] GenerateCode(string inputFileName, string inputFileContent)
		{
			var file = inputFileContent;
			return Encoding.UTF8.GetBytes(file);
		}
	}
}
