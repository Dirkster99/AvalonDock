using Caliburn.Micro;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace CaliburnDockTestApp.ViewModels
{
	public abstract class ViewModelBase : Screen
	{
		public ViewModelBase()
		{
			DisplayName = GetType().Name;
		}

		private bool _isDirty;

		public bool IsDirty
		{
			get { return _isDirty; }
			set { Set(ref _isDirty, value); }
		}

		private string _customText;

		public string CustomText
		{
			get { return _customText; }
			set
			{
				if (Set(ref _customText, value))
					IsDirty = true;
			}
		}

		private RelayCommand _closeCommand;

		public RelayCommand CloseCommand
		{
			get
			{
				return _closeCommand
					?? (_closeCommand = new RelayCommand(_ => TryCloseAsync(null)));
			}
		}

		protected bool CanClosePrompt()
		{
			bool close = true;
			if (IsDirty)
			{
				var result = MessageBox.Show($"Are you sure you want to close '{DisplayName}'?", DisplayName, MessageBoxButton.YesNo, MessageBoxImage.Question);
				close = result == MessageBoxResult.Yes;
			}
			return close;
		}

#if CALIBURN_ASYNC
		// For Caliburn 4.0 or higher (asynchronous)
		public override Task<bool> CanCloseAsync(CancellationToken cancellationToken = default)
		{
			bool close = CanClosePrompt();
			Trace.TraceInformation($"{GetType().Name}.CanCloseAsync: close={close}");
			return Task.FromResult(close);
		}

		protected override Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
		{
			Trace.TraceInformation($"{GetType().Name}.OnDeactivateAsync: close={close}");
			return base.OnDeactivateAsync(close, cancellationToken);
		}
#else
		// For Caliburn 3.2 and lower (synchronous)
		public Task TryCloseAsync(bool? dialogResult)
		{
			TryClose(dialogResult);
			return Task.FromResult(dialogResult);
		}

		public override void CanClose(Action<bool> callback)
		{
			bool close = CanClosePrompt();
			Trace.TraceInformation($"{GetType().Name}.CanClose: close={close}");
			callback(close);
		}

		protected override void OnDeactivate(bool close)
		{
			Trace.TraceInformation($"{GetType().Name}.OnDeactivate: close={close}");
			base.OnDeactivate(close);
		}
#endif
	}
}
