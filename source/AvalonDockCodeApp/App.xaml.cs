using System;
using System.Windows;
using AvalonDock;
using AvalonDock.Core;
using AvalonDock.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using ToggleTestApp.ViewModels;

namespace ToggleTestApp;

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
		// Dock layout service — configure toggle dock options and toolboxes in one call
		services.AddDockLayoutService(dock =>
		{
			dock.ConfigureToggleDock(opts =>
			{
				opts.ButtonSize = 28;
				opts.DefaultDockWidth = 280;
				opts.DefaultDockHeight = 220;
				opts.LayoutPriority = nameof(DockLayoutPriority.BottomFullWidth);
			});

			// Register toolboxes — order determines sidebar button order
			dock.AddToolbox<FolderExplorerViewModel>(sp =>
				new FolderExplorerViewModel(_ => { }));
			dock.AddToolbox<SearchViewModel>();
			dock.AddToolbox<SourceControlViewModel>();
			dock.AddToolbox<ProblemsViewModel>();
			dock.AddToolbox<TerminalViewModel>();
		});

		// MainViewModel uses only the layout service — all anchorables accessible via GetAnchorable<T>()
		services.AddSingleton<MainViewModel>();

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