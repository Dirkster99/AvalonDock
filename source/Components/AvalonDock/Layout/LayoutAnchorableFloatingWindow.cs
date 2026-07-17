using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Markup;
using System.Xml.Serialization;

namespace AvalonDock.Layout
{
	/// <summary>
	/// Represents a layout anchorable floating window.
	/// </summary>
	[Serializable]
	[ContentProperty(nameof(RootPanel))]
	public class LayoutAnchorableFloatingWindow : LayoutFloatingWindow, ILayoutElementWithVisibility
	{
		private LayoutAnchorablePaneGroup _rootPanel;

		[NonSerialized]
		private bool _isVisible = true;

		/// <summary>
		/// Occurs when the is visible changed event is raised.
		/// </summary>
		public event EventHandler IsVisibleChanged;

		/// <summary>
		/// Gets a value indicating whether this instance is single pane.
		/// </summary>
		public bool IsSinglePane => RootPanel != null && RootPanel.Descendents().OfType<ILayoutAnchorablePane>().Count(p => p.IsVisible) == 1;

		/// <summary>
		/// Gets a value indicating whether this instance is visible.
		/// </summary>
		[XmlIgnore]
		public bool IsVisible
		{
			get => _isVisible;
			private set
			{
				if (value == _isVisible) return;
				RaisePropertyChanging(nameof(IsVisible));
				_isVisible = value;
				RaisePropertyChanged(nameof(IsVisible));
				IsVisibleChanged?.Invoke(this, EventArgs.Empty);
			}
		}

		/// <summary>
		/// Gets or sets the root panel.
		/// </summary>
		public LayoutAnchorablePaneGroup RootPanel
		{
			get => _rootPanel;
			set
			{
				if (value == _rootPanel) return;
				RaisePropertyChanging(nameof(RootPanel));
				if (_rootPanel != null) _rootPanel.ChildrenTreeChanged -= _rootPanel_ChildrenTreeChanged;
				_rootPanel = value;
				if (_rootPanel != null)
				{
					_rootPanel.Parent = this;
					_rootPanel.ChildrenTreeChanged += _rootPanel_ChildrenTreeChanged;
				}

				RaisePropertyChanged(nameof(RootPanel));
				RaisePropertyChanged(nameof(IsSinglePane));
				RaisePropertyChanged(nameof(SinglePane));
				RaisePropertyChanged(nameof(Children));
				RaisePropertyChanged(nameof(ChildrenCount));
				((ILayoutElementWithVisibility)this).ComputeVisibility();
			}
		}

		/// <summary>
		/// Gets the single pane.
		/// </summary>
		public ILayoutAnchorablePane SinglePane
		{
			get
			{
				if (!IsSinglePane) return null;
				var singlePane = RootPanel.Descendents().OfType<LayoutAnchorablePane>().Single(p => p.IsVisible);
				singlePane.UpdateIsDirectlyHostedInFloatingWindow();
				return singlePane;
			}
		}

		/// <inheritdoc/>
		void ILayoutElementWithVisibility.ComputeVisibility() => ComputeVisibility();

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
			RootPanel = newElement as LayoutAnchorablePaneGroup;
		}

		/// <inheritdoc/>
		public override int ChildrenCount => RootPanel == null ? 0 : 1;

		/// <inheritdoc/>
		public override bool IsValid => RootPanel != null;

#if TRACE
		/// <inheritdoc />
		public override void ConsoleDump(int tab)
		{
			System.Diagnostics.Trace.Write(new string(' ', tab * 4));
			System.Diagnostics.Trace.WriteLine("FloatingAnchorableWindow()");

			RootPanel.ConsoleDump(tab + 1);
		}
#endif

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
		/// Executes the compute visibility operation.
		/// </summary>
		private void ComputeVisibility() => IsVisible = RootPanel != null && RootPanel.IsVisible;
	}
}