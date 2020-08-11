using Caliburn.Micro;
using CaliburnDockTestApp.ViewModels;
using System.Windows;

namespace CaliburnDockTestApp
{
	public class Bootstrapper : Caliburn.Micro.BootstrapperBase
	{
		public Bootstrapper()
		{
			LogManager.GetLog = type => new CaliburnLogger(type);
			Initialize();
		}

		protected override void OnStartup(object sender, StartupEventArgs e)
		{
			base.OnStartup(sender, e);
			DisplayRootViewFor<MainWindowViewModel>();
		}
	}
}
