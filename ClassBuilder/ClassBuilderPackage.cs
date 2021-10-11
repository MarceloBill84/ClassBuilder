using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TextTemplating.VSHost;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using Task = System.Threading.Tasks.Task;

namespace ClassBuilder
{
	[PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
	[Guid(PackageGuids.guidClassBuilderPackageString)]
	[ProvideCodeGenerator(typeof(GenerateBuilder), GenerateBuilder.Name, GenerateBuilder.Description, true, RegisterCodeBase = true)]
	[ProvideCodeGeneratorExtension(GenerateBuilder.Name, "cs")]
	[ProvideMenuResource("Menus.ctmenu", 1)]
	[ProvideUIContextRule(PackageGuids.guidClassBuilderPackageString,
		name: "CS files",
		expression: "Cs",
		termNames: new[] { "Cs" },
		termValues: new[] { "HierSingleSelectionName:.cs" })]
	public sealed class ClassBuilderPackage : AsyncPackage
	{
		protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
		{
			await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
			await BuilderCommand.InitializeAsync(this);
		}
	}
}
