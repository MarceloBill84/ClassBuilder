using ClassBuilder.Factory;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using System.ComponentModel.Design;
using System.IO;
using Task = System.Threading.Tasks.Task;

namespace ClassBuilder
{
	internal sealed class BuilderCommand
	{
		public static async Task InitializeAsync(AsyncPackage package)
		{
			await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

			var commandService = await package.GetServiceAsync<IMenuCommandService, IMenuCommandService>();

			DTE2 dte = await package.GetServiceAsync<DTE, DTE2>();

			var menuCommandID = new CommandID(PackageGuids.guidClassBuilderPackageCmdSet, 0x0100);

			var cmd = new OleMenuCommand((s, e) => Execute(dte), menuCommandID, false);
			commandService.AddCommand(cmd);
		}

		private static void Execute(DTE2 dte)
		{

			ThreadHelper.ThrowIfNotOnUIThread();

			var item = dte.SelectedItems.Item(1).ProjectItem;

			var originalFileName = item.FileNames[0];

			var fileContent = File.ReadAllText(originalFileName);

			var newFileName = string.Concat(item.FileNames[0].Substring(0, originalFileName.Length - 3), "Builder.cs");

			File.WriteAllText(newFileName, BuilderFactory.Create(fileContent));
		}
	}
}
