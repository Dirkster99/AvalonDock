using System;
using System.Collections.Generic;
using System.Windows;
using AvalonDock;
using AvalonDock.Core;
using AvalonDock.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using ToggleTestApp.ViewModels;

namespace ToggleTestApp
{
	public partial class App : Application
	{
		private IServiceProvider? _serviceProvider;

		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);

			var services = new ServiceCollection();
			ConfigureServices(services);
			_serviceProvider = services.BuildServiceProvider();

			var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
			mainWindow.Show();
		}

		private static void ConfigureServices(IServiceCollection services)
		{
			// AvalonDock toggle options via DI
			services.AddToggleDockOptions(opts =>
			{
				opts.ButtonSize = 28;
				opts.DefaultDockWidth = 280;
				opts.DefaultDockHeight = 220;
				opts.LayoutPriority = nameof(DockLayoutPriority.BottomFullWidth);
			});

			// Register toolboxes — order determines sidebar button order
			services.AddToolbox<FolderExplorerViewModel>(sp =>
				new FolderExplorerViewModel(_ => { }));
			services.AddToolbox<SearchViewModel>();
			services.AddToolbox<SourceControlViewModel>();
			services.AddToolbox<ProblemsViewModel>();
			services.AddToolbox<TerminalViewModel>();

			// MainViewModel receives all registered toolboxes
			services.AddSingleton<MainViewModel>(sp =>
			{
				var toolboxes = sp.GetRequiredService<IEnumerable<IToolboxViewModel>>();
				var folderVm = sp.GetRequiredService<FolderExplorerViewModel>();
				var mainVm = new MainViewModel(toolboxes, folderVm);
				folderVm.SetOpenFileCallback(mainVm.OpenFile);
				return mainVm;
			});

			// Main window
			services.AddSingleton<MainWindow>();
		}

		protected override void OnExit(ExitEventArgs e)
		{
			if (_serviceProvider is IDisposable disposable)
			{
				disposable.Dispose();
			}

			base.OnExit(e);
		}
	}
}
