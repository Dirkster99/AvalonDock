using AvalonDockTest.TestHelpers;
using AvalonDockTest.Views;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting.STAExtensions;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Controls;

namespace AvalonDockTest
{
	/// <summary>
	/// This class tests the NotifyCollectionChangedAction.Reset handling. Previous implementations
	/// of AvalonDock clear the associated collection if the notification was received, instead
	/// of iterating for any remaining items in the collection.
	/// </summary>
	[STATestClass]
	public class CollectionResetTest : AutomationTestBase
	{
		#region Notification Counting for CollectionChanged
		public class NotifyActionCount
		{
			public int AddCount;
			public int RemoveCount;
			public int ReplaceCount;
			public int MoveCount;
			public int ResetCount;

			// Use simple string comparison instead of IEquatable<T>
			public override string ToString()
			{
				return $"{{ AddCount: {AddCount}, RemoveCount: {RemoveCount}, ReplaceCount: {ReplaceCount}, MoveCount: {MoveCount}, ResetCount: {ResetCount} }}";
			}
		}

		private static void UpdateNotificationCount(NotifyCollectionChangedEventArgs e, NotifyActionCount notifyCount)
		{
			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					++notifyCount.AddCount;
					break;
				case NotifyCollectionChangedAction.Remove:
					++notifyCount.RemoveCount;
					break;
				case NotifyCollectionChangedAction.Replace:
					++notifyCount.ReplaceCount;
					break;
				case NotifyCollectionChangedAction.Move:
					++notifyCount.MoveCount;
					break;
				case NotifyCollectionChangedAction.Reset:
					++notifyCount.ResetCount;
					break;
				default:
					break;
			}
		}
		#endregion

		#region Shared Main Test Window for both Anchorables and Documents
		static Lazy<CollectionResetTestWindow> TestWindow =
			new Lazy<CollectionResetTestWindow>(() => CreateTestWindow());

		private static CollectionResetTestWindow CreateTestWindow()
		{
			TestHost.SwitchToAppThread();

			var task = WindowHelpers.CreateInvisibleWindowAsync<CollectionResetTestWindow>();
			task.Wait();

			CollectionResetTestWindow window = task.Result;
			Assert.IsTrue(window.IsLoaded);

			// Hook up to ObservableCollection notifications
			window.Anchorables.CollectionChanged += (s, e) => { UpdateNotificationCount(e, AnchorNotifications); };
			window.Documents.CollectionChanged += (s, e) => { UpdateNotificationCount(e, DocumentNotifications); };

			// Create some anchorable test items
			for (int index = 0; index < ExpectedAnchorableCount; index++)
			{
				var child = new UserControl() { Tag = $"Anchorable {index}" };
				window.Anchorables.Add(child);
			}

			// Create some document test items
			for (int index = 0; index < ExpectedDocumentCount; index++)
			{
				var child = new UserControl() { Tag = $"Document {index}" };
				window.Documents.Add(child);
			}

			return window;
		}
		#endregion

		#region Adapter for testing Anchorables and Documents
		const int ExpectedAnchorableCount = 5;
		const int ExpectedDocumentCount = 7;

		static NotifyActionCount AnchorNotifications = new NotifyActionCount();
		static NotifyActionCount DocumentNotifications = new NotifyActionCount();

		public interface ITestAdapter
		{
			CustomObservableCollection<object> GetCollection();
			IEnumerable<object> GetSource();
			int ExpectedCount { get; }
			int CollectionCount { get; }
			int SourceCount { get; }
			NotifyActionCount Notifications { get; }
		}

		public class AnchorablesTestAdapter : ITestAdapter
		{
			private readonly CollectionResetTestWindow _window;

			public AnchorablesTestAdapter(CollectionResetTestWindow window)
			{
				_window = window;
			}

			public int ExpectedCount => ExpectedAnchorableCount;

			public int CollectionCount => _window.Anchorables.Count;

			public int SourceCount => _window.DockManager.AnchorablesSource.Cast<object>().Count();

			public NotifyActionCount Notifications => AnchorNotifications;

			public CustomObservableCollection<object> GetCollection()
			{
				return _window.Anchorables;
			}

			public IEnumerable<object> GetSource()
			{
				return _window.DockManager.AnchorablesSource.Cast<object>();
			}
		}

		public class DocumentsTestAdapter : ITestAdapter
		{
			private readonly CollectionResetTestWindow _window;

			public DocumentsTestAdapter(CollectionResetTestWindow window)
			{
				_window = window;
			}

			public int ExpectedCount => ExpectedDocumentCount;

			public int CollectionCount => _window.Documents.Count;

			public int SourceCount => _window.DockManager.DocumentsSource.Cast<object>().Count();

			public NotifyActionCount Notifications => DocumentNotifications;

			public CustomObservableCollection<object> GetCollection()
			{
				return _window.Documents;
			}

			public IEnumerable<object> GetSource()
			{
				return _window.DockManager.DocumentsSource.Cast<object>();
			}
		}
		#endregion

		[STATestMethod]
		public void CollectionResetAnchorablesTest()
		{
			CollectionResetTestWindow window = TestWindow.Value;
			Assert.IsTrue(window.IsLoaded);

			var adapter = new AnchorablesTestAdapter(window);
			TestResetNotification(adapter);
		}

		[STATestMethod]
		public void CollectionResetDocumentsTest()
		{
			CollectionResetTestWindow window = TestWindow.Value;
			Assert.IsTrue(window.IsLoaded);

			var adapter = new DocumentsTestAdapter(window);
			TestResetNotification(adapter);
		}

		/// <summary>
		/// Common logic for test both Anchorables and Documents
		/// </summary>
		/// <param name="adapter"></param>
		private void TestResetNotification(ITestAdapter adapter)
		{
			// Make sure we have the correct initial counts
			Assert.AreEqual(adapter.ExpectedCount, adapter.CollectionCount);
			Assert.AreEqual(adapter.ExpectedCount, adapter.SourceCount);

			// We are only expecting the newly added items
			AreEqual(adapter.Notifications, new NotifyActionCount { AddCount = adapter.ExpectedCount });

			// Raise the Reset notification. Previous versions of AvalonDock assumed the collection was cleared.
			adapter.GetCollection().Refresh();

			// Nothing should have changed
			Assert.AreEqual(adapter.ExpectedCount, adapter.CollectionCount);
			Assert.AreEqual(adapter.ExpectedCount, adapter.SourceCount);

			// Verify nothing has been closed and the Reset notification was received
			AreEqual(adapter.Notifications, new NotifyActionCount { AddCount = adapter.ExpectedCount, ResetCount = 1 });

			// Close the odd items. This will also raise the Reset notification.
			var removeChildren = adapter.GetSource().Where((x, i) => i % 2 != 0).ToList();
			adapter.GetCollection().RemoveRange(removeChildren);

			// Verify the remaining items
			int remainingCount = adapter.ExpectedCount - removeChildren.Count;
			Assert.AreEqual(remainingCount, adapter.CollectionCount);
			Assert.AreEqual(remainingCount, adapter.SourceCount);

			// NOTE: The RemoveCount is still zero because RemoveRange silently removed the items then issues a Reset.
			AreEqual(adapter.Notifications, new NotifyActionCount { AddCount = adapter.ExpectedCount, ResetCount = 2 });
		}

		// Use simple string comparison instead of IEquatable<T>
		private void AreEqual(NotifyActionCount expected, NotifyActionCount actual)
		{
			Assert.AreEqual(expected.ToString(), actual.ToString());
		}
	}
}
