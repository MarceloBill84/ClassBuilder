using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using System;
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

			var cmd = new OleMenuCommand((s, e) => Execute(s, e, dte), menuCommandID, false);
			commandService.AddCommand(cmd);
		}

		private static void Execute(object teste, EventArgs e, DTE2 dte)
		{

			ThreadHelper.ThrowIfNotOnUIThread();

			var item = dte.SelectedItems.Item(1).ProjectItem;


			var test = File.ReadAllText(item.FileNames[0]);

			//var textManager = (IVsTextManager)ServiceProvider.GlobalProvider.GetService(typeof(SVsTextManager));


			//textManager.

			/*string message = string.Format(CultureInfo.CurrentCulture, "Inside {0}.MenuItemCallback()", this.GetType().FullName);
			string title = "BuilderCommand";

			// Show a message box to prove we were here
			VsShellUtilities.ShowMessageBox(
				this.package,
				message,
				title,
				OLEMSGICON.OLEMSGICON_INFO,
				OLEMSGBUTTON.OLEMSGBUTTON_OK,
				OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);*/
		}
	}
}
