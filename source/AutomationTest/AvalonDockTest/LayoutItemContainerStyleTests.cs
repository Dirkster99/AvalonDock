using System.Threading;
using System.Windows;
using System.Windows.Controls;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;
using NUnit.Framework;

namespace AvalonDockTest
{
	/// <summary>
	/// Unit tests for the styles a DockingManager applies to its LayoutItem containers via
	/// LayoutItemContainerStyle and LayoutItemContainerStyleSelector.
	/// Covers issues:
	///   #505 - CanClose setter of LayoutItem Style will not work if DataContext is set in xaml
	/// </summary>
	[TestFixture]
	[Apartment(ApartmentState.STA)]
	public class LayoutItemContainerStyleTests
	{
		/// <summary>
		/// A container style assigned after the documents already exist must still win.
		/// This is the ordering produced by declaring the DataContext in XAML: DocumentsSource
		/// resolves while the XAML is parsed, so the documents are created before the parser
		/// reaches the LayoutItemContainerStyle(Selector) property.
		/// Regression for #505 - CanClose setter of LayoutItem Style will not work if DataContext is set in xaml.
		/// </summary>
		[Test]
		public void CanCloseSetter_AppliesWhenStyleAssignedAfterDocumentExists_Issue505()
		{
			var manager = new DockingManager();
			var document = new LayoutDocument { Title = "Doc", ContentId = "doc" };

			// Document first, style second - the broken order.
			manager.Layout = CreateLayoutWith(document);
			manager.LayoutItemContainerStyle = CreateCanCloseStyle(false);

			var item = manager.GetLayoutItemFromModel(document);

			Assert.That(item.CanClose, Is.False, "The CanClose style setter must win over the value seeded from the model (Issue #505).");
			Assert.That(document.CanClose, Is.False, "The CanClose style setter must reach the layout model (Issue #505).");
		}

		/// <summary>
		/// The same expectation for a style supplied through a StyleSelector, which is what the
		/// MVVM sample and the issue reporter use.
		/// Regression for #505 - CanClose setter of LayoutItem Style will not work if DataContext is set in xaml.
		/// </summary>
		[Test]
		public void CanCloseSetter_AppliesWhenStyleSelectorAssignedAfterDocumentExists_Issue505()
		{
			var manager = new DockingManager();
			var document = new LayoutDocument { Title = "Doc", ContentId = "doc" };

			manager.Layout = CreateLayoutWith(document);
			manager.LayoutItemContainerStyleSelector = new CanCloseStyleSelector(CreateCanCloseStyle(false));

			var item = manager.GetLayoutItemFromModel(document);

			Assert.That(item.CanClose, Is.False, "The CanClose style setter must win when supplied by a StyleSelector (Issue #505).");
			Assert.That(document.CanClose, Is.False, "The CanClose style setter must reach the layout model (Issue #505).");
		}

		/// <summary>
		/// The order that already worked before the fix - style first, documents second - must keep working.
		/// </summary>
		[Test]
		public void CanCloseSetter_AppliesWhenStyleAssignedBeforeDocumentExists_Issue505()
		{
			var manager = new DockingManager();
			var document = new LayoutDocument { Title = "Doc", ContentId = "doc" };

			// Style first, document second - the order that works when the DataContext is set in code behind.
			manager.LayoutItemContainerStyle = CreateCanCloseStyle(false);
			manager.Layout = CreateLayoutWith(document);

			var item = manager.GetLayoutItemFromModel(document);

			Assert.That(item.CanClose, Is.False, "The CanClose style setter must win regardless of assignment order (Issue #505).");
			Assert.That(document.CanClose, Is.False, "The CanClose style setter must reach the layout model (Issue #505).");
		}

		/// <summary>
		/// Without a container style the item still mirrors the model, so seeding CanClose from the
		/// model is not lost by the fix.
		/// </summary>
		[Test]
		public void CanClose_MirrorsModelWhenNoContainerStyleIsSet_Issue505()
		{
			var manager = new DockingManager();
			var closeable = new LayoutDocument { Title = "Closeable", ContentId = "closeable" };
			var fixedDocument = new LayoutDocument { Title = "Fixed", ContentId = "fixed", CanClose = false };

			manager.Layout = CreateLayoutWith(closeable, fixedDocument);

			Assert.That(manager.GetLayoutItemFromModel(closeable).CanClose, Is.True, "CanClose must still be seeded from the model.");
			Assert.That(manager.GetLayoutItemFromModel(fixedDocument).CanClose, Is.False, "CanClose must still be seeded from the model.");
		}

		private static LayoutRoot CreateLayoutWith(params LayoutDocument[] documents)
		{
			var pane = new LayoutDocumentPane();
			foreach (var document in documents)
			{
				pane.Children.Add(document);
			}

			return new LayoutRoot { RootPanel = new LayoutPanel(pane) };
		}

		private static Style CreateCanCloseStyle(bool canClose)
		{
			var style = new Style(typeof(LayoutDocumentItem));
			style.Setters.Add(new Setter(LayoutItem.CanCloseProperty, canClose));
			return style;
		}

		private sealed class CanCloseStyleSelector : StyleSelector
		{
			private readonly Style _style;

			public CanCloseStyleSelector(Style style) => _style = style;

			public override Style SelectStyle(object item, DependencyObject container) => _style;
		}
	}
}
