using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace AvalonDockTest
{
	/// <summary>
	/// Custom ObservableCollection to raise the Reset notification for unit testing
	/// </summary>
	public class CustomObservableCollection<T> : ObservableCollection<T>
	{
		/// <summary>
		/// Enables/Disables property change notification.
		/// </summary>
		public bool IsNotifying { get; set; } = true;

		/// <summary>
		/// Raises a change notification indicating that all bindings should be refreshed.
		/// </summary>
		public void Refresh()
		{
			OnPropertyChanged(new PropertyChangedEventArgs("Count"));
			OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}

		/// <summary>
		/// Adds a range of items. Suspends notifications during add, then raises a Reset notification.
		/// </summary>
		/// <param name="items"></param>
		public void AddRange(IEnumerable<T> items)
		{
			var previousNotifying = IsNotifying;
			IsNotifying = false;
			var index = Count;
			foreach (var item in items)
			{
				InsertItem(index, item);
				++index;
			}
			IsNotifying = previousNotifying;

			Refresh();
		}

		/// <summary>
		/// Removes a range of items. Suspends notifications during add, then raises a Reset notification.
		/// </summary>
		/// <param name="items"></param>
		public void RemoveRange(IEnumerable<T> items)
		{
			var previousNotifying = IsNotifying;
			IsNotifying = false;
			foreach (var item in items)
			{
				var index = IndexOf(item);
				if (index >= 0)
				{
					RemoveItem(index);
				}
			}
			IsNotifying = previousNotifying;

			Refresh();
		}

		/// <summary>
		/// Raises the <see cref = "E:System.Collections.ObjectModel.ObservableCollection`1.CollectionChanged" /> event with the provided arguments.
		/// </summary>
		/// <param name = "e">Arguments of the event being raised.</param>
		protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
		{
			if (IsNotifying)
			{
				base.OnCollectionChanged(e);
			}
		}

		/// <summary>
		/// Raises the PropertyChanged event with the provided arguments.
		/// </summary>
		/// <param name = "e">The event data to report in the event.</param>
		protected override void OnPropertyChanged(PropertyChangedEventArgs e)
		{
			if (IsNotifying)
			{
				base.OnPropertyChanged(e);
			}
		}
	}
}
