using ClassBuilder.Exceptions;
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
		private static Package _package;

		private BuilderCommand() { }

		public static async Task InitializeAsync(AsyncPackage package)
		{
			await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

			_package = package;

			var commandService = await package.GetServiceAsync<IMenuCommandService, IMenuCommandService>();

			DTE2 dte = await package.GetServiceAsync<DTE, DTE2>();

			var menuCommandID = new CommandID(PackageGuids.guidClassBuilderPackageCmdSet, 0x0100);

			var cmd = new OleMenuCommand((s, e) => Execute(dte), menuCommandID, false);
			commandService.AddCommand(cmd);
		}

		private static void Execute(DTE2 dte)
		{

			ThreadHelper.ThrowIfNotOnUIThread();

			try
			{
				for (int i = 0; i < dte.SelectedItems.Count; i++)
				{
					var item = dte.SelectedItems.Item(i + 1).ProjectItem;
					var originalFileName = item.FileNames[0];
					var fileContent = File.ReadAllText(originalFileName);
					var newFileName = string.Concat(item.FileNames[0].Substring(0, originalFileName.Length - 3), "Builder.cs");
					File.WriteAllText(newFileName, BuilderFactory.Create(fileContent));
				}
			}
			catch (ValidationException ex)
			{
				VsShellUtilities.ShowMessageBox(serviceProvider: _package,
					message: ex.Message,
					title: "Generate Builder",
					icon: Microsoft.VisualStudio.Shell.Interop.OLEMSGICON.OLEMSGICON_WARNING,
					msgButton: Microsoft.VisualStudio.Shell.Interop.OLEMSGBUTTON.OLEMSGBUTTON_OK,
					defaultButton: Microsoft.VisualStudio.Shell.Interop.OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST
					);
			}
		}
	}
}
