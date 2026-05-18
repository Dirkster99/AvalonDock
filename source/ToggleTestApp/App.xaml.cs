using System;
using System.Windows;
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
			// ViewModels
			services.AddSingleton<TerminalViewModel>();
			services.AddSingleton<FolderExplorerViewModel>(sp =>
				new FolderExplorerViewModel(_ => { }));
			services.AddSingleton<MainViewModel>(sp =>
			{
				var folderVm = sp.GetRequiredService<FolderExplorerViewModel>();
				var termVm = sp.GetRequiredService<TerminalViewModel>();
				var mainVm = new MainViewModel(folderVm, termVm);
				folderVm.SetOpenFileCallback(mainVm.OpenFile);
				return mainVm;
			});

			// Main window (receives MainViewModel via constructor)
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
