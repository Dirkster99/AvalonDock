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

		// Register each as IToolbox for collection injection into DockLayoutService
		services.AddSingleton<IToolbox>(sp => sp.GetRequiredService<FolderExplorerViewModel>());
		services.AddSingleton<IToolbox>(sp => sp.GetRequiredService<SearchViewModel>());
		services.AddSingleton<IToolbox>(sp => sp.GetRequiredService<SourceControlViewModel>());
		services.AddSingleton<IToolbox>(sp => sp.GetRequiredService<ProblemsViewModel>());
		services.AddSingleton<IToolbox>(sp => sp.GetRequiredService<TerminalViewModel>());

		// Dock layout service — auto-builds the MVVM dock tree from toolboxes
		services.AddDockLayoutService();

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