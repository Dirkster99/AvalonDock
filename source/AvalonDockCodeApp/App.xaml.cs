using System;
using System.Windows;
using AvalonDock;
using AvalonDock.Core;
using AvalonDock.DependencyInjection;
#if DEVFLOW
using LeXtudio.DevFlow.Agent.Core;
using LeXtudio.DevFlow.Agent.Wpf;
#endif
using Microsoft.Extensions.DependencyInjection;
#if DEVFLOW
using Microsoft.Maui.DevFlow.Agent.Core;
#endif
using ToggleTestApp.ViewModels;

namespace ToggleTestApp;

public partial class App : Application
{
	private IServiceProvider? _serviceProvider;
#if DEVFLOW
	private WpfAgentService? _devFlowService;
#endif

	protected override void OnStartup(StartupEventArgs e)
	{
		base.OnStartup(e);

		// Attached before the window is shown so an integration test can query the very first layout
		// state (sidebar buttons and their checked states) without racing the startup.
#if DEVFLOW
		_devFlowService = this.AddWpfDevFlowAgent(new AgentOptions
		{
			Port = GetAgentPort(),
		});
#endif

		var services = new ServiceCollection();
		ConfigureServices(services);
		_serviceProvider = services.BuildServiceProvider();

		var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
		mainWindow.Show();
	}

#if DEVFLOW
	/// <summary>
	/// The DevFlow port: DEVFLOW_AGENT_PORT when the fixture set one, otherwise the assembly's own
	/// declared port (see DevFlowIntegrationTests for the constant this app is tested on), otherwise
	/// the agent default.
	/// </summary>
	private static int GetAgentPort()
	{
		var portValue = Environment.GetEnvironmentVariable("DEVFLOW_AGENT_PORT");
		if (int.TryParse(portValue, out var parsedPort) && parsedPort > 0)
			return parsedPort;

		return DevFlowAgentPortResolver.GetPortFromAssemblyMetadata() ?? AgentOptions.DefaultPort;
	}
#endif

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