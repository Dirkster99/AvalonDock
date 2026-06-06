using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Markup;
using System.Xml.Serialization;

namespace AvalonDock.Layout
{
	/// <summary>
	/// Represents a layout document floating window.
	/// </summary>
	[ContentProperty(nameof(RootPanel))]
	[Serializable]
	public class LayoutDocumentFloatingWindow : LayoutFloatingWindow, ILayoutElementWithVisibility
	{
		private LayoutDocumentPaneGroup _rootPanel = null;

		[NonSerialized]
		private bool _isVisible = true;

		/// <summary>
		/// Occurs when the is visible changed event is raised.
		/// </summary>
		public event EventHandler IsVisibleChanged;

		/// <summary>
		/// Gets or sets the root panel.
		/// </summary>
		public LayoutDocumentPaneGroup RootPanel
		{
			get => _rootPanel;
			set
			{
				if (_rootPanel == value) return;
				if (_rootPanel != null) _rootPanel.ChildrenTreeChanged -= _rootPanel_ChildrenTreeChanged;

				_rootPanel = value;
				if (_rootPanel != null) _rootPanel.Parent = this;
				if (_rootPanel != null) _rootPanel.ChildrenTreeChanged += _rootPanel_ChildrenTreeChanged;

				RaisePropertyChanged(nameof(RootPanel));
				RaisePropertyChanged(nameof(IsSinglePane));
				RaisePropertyChanged(nameof(SinglePane));
				RaisePropertyChanged(nameof(Children));
				RaisePropertyChanged(nameof(ChildrenCount));
				((ILayoutElementWithVisibility)this).ComputeVisibility();
			}
		}

		/// <summary>
		/// Executes the root panel children tree changed operation.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The e.</param>
		private void _rootPanel_ChildrenTreeChanged(object sender, ChildrenTreeChangedEventArgs e)
		{
			RaisePropertyChanged(nameof(IsSinglePane));
			RaisePropertyChanged(nameof(SinglePane));
		}

		/// <summary>
		/// Gets a value indicating whether this instance is single pane.
		/// </summary>
		public bool IsSinglePane => RootPanel?.Descendents().OfType<LayoutDocumentPane>().Count(p => p.IsVisible) == 1;

		/// <summary>
		/// Gets the single pane.
		/// </summary>
		public LayoutDocumentPane SinglePane
		{
			get
			{
				if (!IsSinglePane) return null;
				var singlePane = RootPanel.Descendents().OfType<LayoutDocumentPane>().Single(p => p.IsVisible);
				// singlePane.UpdateIsDirectlyHostedInFloatingWindow();
				return singlePane;
			}
		}

		/// <summary>
		/// Gets a value indicating whether this instance is visible.
		/// </summary>
		[XmlIgnore]
		public bool IsVisible
		{
			get => _isVisible;
			private set
			{
				if (_isVisible == value) return;
				RaisePropertyChanging(nameof(IsVisible));
				_isVisible = value;
				RaisePropertyChanged(nameof(IsVisible));
				IsVisibleChanged?.Invoke(this, EventArgs.Empty);
			}
		}

		/// <inheritdoc/>
		public override IEnumerable<ILayoutElement> Children
		{
			get { if (ChildrenCount == 1) yield return RootPanel; }
		}

		/// <inheritdoc/>
		public override void RemoveChild(ILayoutElement element)
		{
			Debug.Assert(element == RootPanel && element != null);
			RootPanel = null;
		}

		/// <inheritdoc/>
		public override void ReplaceChild(ILayoutElement oldElement, ILayoutElement newElement)
		{
			Debug.Assert(oldElement == RootPanel && oldElement != null);
			RootPanel = newElement as LayoutDocumentPaneGroup;
		}

		/// <inheritdoc/>
		public override int ChildrenCount => RootPanel == null ? 0 : 1;

		/// <inheritdoc/>
		void ILayoutElementWithVisibility.ComputeVisibility() => IsVisible = RootPanel != null && RootPanel.IsVisible;

		/// <inheritdoc/>
		public override bool IsValid => RootPanel != null;

#if TRACE
		/// <inheritdoc/>
		public override void ConsoleDump(int tab)
		{
			System.Diagnostics.Trace.Write(new string(' ', tab * 4));
			System.Diagnostics.Trace.WriteLine("FloatingDocumentWindow()");

			RootPanel.ConsoleDump(tab + 1);
		}
#endif

	}
}