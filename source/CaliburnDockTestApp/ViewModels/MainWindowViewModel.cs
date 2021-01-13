using Caliburn.Micro;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CaliburnDockTestApp.ViewModels
{
	public class MainWindowViewModel : Conductor<Screen>.Collection.OneActive
	{
		private RelayCommand _showACommand;
		private RelayCommand _showBCommand;
		private RelayCommand _showCCommand;
		private RelayCommand _showDCommand;

		public MainWindowViewModel()
		{
			SetShowCommand<TestViewModel>(ref _showACommand, "Test A");
			SetShowCommand<TestViewModel>(ref _showBCommand, "Test B");
			SetShowCommand<TestViewModel>(ref _showCCommand, "Test C");
			SetShowCommand<TestViewModel>(ref _showDCommand, "Test D");
		}

		public RelayCommand ShowACommand { get { return _showACommand; } }
		public RelayCommand ShowBCommand { get { return _showBCommand; } }
		public RelayCommand ShowCCommand { get { return _showCCommand; } }

		private void SetShowCommand<T>(ref RelayCommand command, string displayName)
			where T : ViewModelBase
		{
			command = new RelayCommand(_ => ActivateOrCreate<T>(displayName));
		}

		private int _createCount;

		public Task ActivateOrCreate<T>(string displayName)
			where T : ViewModelBase
		{
			var item = Items.OfType<T>().FirstOrDefault(x => x.DisplayName == displayName);
			if (item == null)
			{
				item = (T)Activator.CreateInstance(typeof(T));
				item.Parent = this;
				item.ConductWith(this);
				item.DisplayName = displayName;
				item.IsDirty = ++_createCount % 2 > 0;
			}
			return ActivateItemAsync(item, CancellationToken.None);
		}

		private string GetListItemName(object obj)
		{
			if (obj == null)
				return "null";
			else if (obj is IHaveDisplayName)
				return ((IHaveDisplayName)obj).DisplayName;
			else
				return obj.GetType().Name;
		}
		private string GetListTypes(System.Collections.IList list)
		{
			if (list == null)
				return "null";
			else
				return "[" + string.Join(",", list.Cast<object>().Select(x => GetListItemName(x))) + "]";
		}

		private void Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			Trace.TraceInformation($"{GetType().Name}.Items_CollectionChanged: Action={e.Action}, OldItems={GetListTypes(e.OldItems)}, OldStartingIndex={e.OldStartingIndex}, NewItems={GetListTypes(e.NewItems)}, NewStartingIndex={e.NewStartingIndex}");
		}

#if CALIBURN_ASYNC
		// For Caliburn 4.0 or higher (asynchronous)
		protected override Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
		{
			Trace.TraceInformation($"{GetType().Name}.OnDeactivateAsync: close={close}");
			return base.OnDeactivateAsync(close, cancellationToken);
		}

		protected override Task OnInitializeAsync(CancellationToken cancellationToken)
		{
			Trace.TraceInformation($"{GetType().Name}.OnInitializeAsync");

			Items.CollectionChanged += Items_CollectionChanged;
			Task.Run(() => StartInitialDocuments());

			return base.OnInitializeAsync(cancellationToken);
		}
#else
		// For Caliburn 3.2 and lower (synchronous)
		public Task ActivateItemAsync(Screen item, CancellationToken cancellationToken)
		{
			ActivateItem(item);
			return Task.FromResult(0);
		}

		protected override void OnDeactivate(bool close)
		{
			Trace.TraceInformation($"{GetType().Name}.OnDeactivate: close={close}");
			base.OnDeactivate(close);
		}

		protected override void OnInitialize()
		{
			Trace.TraceInformation($"{GetType().Name}.OnInitialize");

			Task.Run(() => StartInitialDocuments());

			base.OnInitialize();
		}
#endif

		protected async Task StartInitialDocuments()
		{
			await Task.Delay(100);
			await ActivateOrCreate<TestViewModel>("Test A");
			await Task.Delay(100);
			await ActivateOrCreate<TestViewModel>("Test B");
			await Task.Delay(100);
			await ActivateOrCreate<TestViewModel>("Test C");
			await Task.Delay(100);
			await ActivateOrCreate<TestViewModel>("Test D");
		}
	}
}
