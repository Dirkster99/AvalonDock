using System;
using System.Linq;
using System.Windows.Markup;
using System.Xml.Serialization;

namespace AvalonDock.Layout
{
	/// <summary>
	/// Represents a layout anchorable pane.
	/// </summary>
	[ContentProperty(nameof(Children))]
	[Serializable]
	public class LayoutAnchorablePane : LayoutPositionableGroup<LayoutAnchorable>, ILayoutAnchorablePane, ILayoutPositionableElement, ILayoutContentSelector, ILayoutPaneSerializable, Core.Serialization.ISerializableLayoutPane
	{
		private int _selectedIndex = -1;

		[XmlIgnore]
		private bool _autoFixSelectedContent = true;

		private string _name = null;
		private string _id;

		/// <summary>
		/// Initializes a new instance of the <see cref="LayoutAnchorablePane"/> class.
		/// </summary>
		public LayoutAnchorablePane()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="LayoutAnchorablePane"/> class.
		/// </summary>
		/// <param name="anchorable">The anchorable.</param>
		public LayoutAnchorablePane(LayoutAnchorable anchorable)
		{
			Children.Add(anchorable);
		}

		/// <summary>
		/// Gets a value indicating whether this instance can hide.
		/// </summary>
		public bool CanHide => Children.All(a => a.CanHide);

		/// <summary>
		/// Gets a value indicating whether this instance can close.
		/// </summary>
		public bool CanClose => Children.All(a => a.CanClose);

		/// <summary>
		/// Gets a value indicating whether this instance is hosted in floating window.
		/// </summary>
		public bool IsHostedInFloatingWindow => this.FindParent<LayoutFloatingWindow>() != null;

		/// <summary>
		/// Gets or sets the name.
		/// </summary>
		public string Name
		{
			get => _name;
			set
			{
				if (value == _name) return;
				_name = value;
				RaisePropertyChanged(nameof(Name));
			}
		}

		/// <summary>
		/// Gets or sets the selected content index.
		/// </summary>
		public int SelectedContentIndex
		{
			get => _selectedIndex;
			set
			{
				if (value < 0 || value >= Children.Count) value = -1;
				if (value == _selectedIndex) return;
				RaisePropertyChanging(nameof(SelectedContentIndex));
				RaisePropertyChanging(nameof(SelectedContent));
				if (_selectedIndex >= 0 && _selectedIndex < Children.Count)
					Children[_selectedIndex].IsSelected = false;
				_selectedIndex = value;
				if (_selectedIndex >= 0 && _selectedIndex < Children.Count)
					Children[_selectedIndex].IsSelected = true;
				RaisePropertyChanged(nameof(SelectedContentIndex));
				RaisePropertyChanged(nameof(SelectedContent));
			}
		}

		/// <summary>
		/// Gets the selected content.
		/// </summary>
		public LayoutContent SelectedContent => _selectedIndex == -1 ? null : Children[_selectedIndex];

		/// <inheritdoc/>
		string ILayoutPaneSerializable.Id
		{
			get => _id;
			set => _id = value;
		}

		/// <inheritdoc/>
		string Core.Serialization.ISerializableLayoutPane.Id
		{
			get => _id;
			set => _id = value;
		}

		/// <inheritdoc/>
		protected override bool GetVisibility() => Children.Count > 0 && Children.Any(c => c.IsVisible);

		/// <inheritdoc/>
		protected override void ChildMoved(int oldIndex, int newIndex)
		{
			if (_selectedIndex == oldIndex)
			{
				RaisePropertyChanging(nameof(SelectedContentIndex));
				_selectedIndex = newIndex;
				RaisePropertyChanged(nameof(SelectedContentIndex));
			}

			base.ChildMoved(oldIndex, newIndex);
		}

		/// <inheritdoc/>
		protected override void OnChildrenCollectionChanged()
		{
			AutoFixSelectedContent();
			for (var i = 0; i < Children.Count; i++)
			{
				if (!Children[i].IsSelected) continue;
				SelectedContentIndex = i;
				break;
			}

			RaisePropertyChanged(nameof(CanClose));
			RaisePropertyChanged(nameof(CanHide));
			RaisePropertyChanged(nameof(IsDirectlyHostedInFloatingWindow));
			base.OnChildrenCollectionChanged();
		}

		/// <inheritdoc/>
		protected override void OnParentChanged(ILayoutContainer oldValue, ILayoutContainer newValue)
		{
			if (oldValue is ILayoutGroup oldGroup) oldGroup.ChildrenCollectionChanged -= OnParentChildrenCollectionChanged;
			RaisePropertyChanged(nameof(IsDirectlyHostedInFloatingWindow));
			if (newValue is ILayoutGroup newGroup) newGroup.ChildrenCollectionChanged += OnParentChildrenCollectionChanged;
			base.OnParentChanged(oldValue, newValue);
		}

#if TRACE
		/// <inheritdoc />
		public override void ConsoleDump(int tab)
		{
			System.Diagnostics.Trace.Write(new string(' ', tab * 4));
			System.Diagnostics.Trace.WriteLine("AnchorablePane()");

			foreach (LayoutElement child in Children)
				child.ConsoleDump(tab + 1);
		}
#endif

		/// <summary>
		/// Executes the index of operation.
		/// </summary>
		/// <param name="content">The layout content.</param>
		/// <returns>The resulting value.</returns>
		public int IndexOf(LayoutContent content)
		{
			if (!(content is LayoutAnchorable anchorableChild)) return -1;
			return Children.IndexOf(anchorableChild);
		}

		/// <summary>
		/// Gets a value indicating whether this instance is directly hosted in floating window.
		/// </summary>
		public bool IsDirectlyHostedInFloatingWindow
		{
			get
			{
				var parentFloatingWindow = this.FindParent<LayoutAnchorableFloatingWindow>();
				return parentFloatingWindow != null && parentFloatingWindow.IsSinglePane;
				// return Parent != null && Parent.ChildrenCount == 1 && Parent.Parent is LayoutFloatingWindow;
			}
		}

		/// <summary>
		/// Sets the next selected index.
		/// </summary>
		internal void SetNextSelectedIndex()
		{
			SelectedContentIndex = -1;
			for (var i = 0; i < Children.Count; ++i)
			{
				if (!Children[i].IsEnabled) continue;
				SelectedContentIndex = i;
				return;
			}
		}

		/// <summary>
		/// Updates the is directly hosted in floating window.
		/// </summary>
		internal void UpdateIsDirectlyHostedInFloatingWindow() => RaisePropertyChanged(nameof(IsDirectlyHostedInFloatingWindow));

		/// <summary>
		/// Executes the auto fix selected content operation.
		/// </summary>
		private void AutoFixSelectedContent()
		{
			if (!_autoFixSelectedContent) return;
			if (SelectedContentIndex >= ChildrenCount) SelectedContentIndex = Children.Count - 1;
			if (SelectedContentIndex == -1 && ChildrenCount > 0) SetLastActivatedIndex();
		}

		/// <summary>
		/// Sets the last activated index.
		/// </summary>
		private void SetLastActivatedIndex()
		{
			var lastActivatedDocument = Children.Where(c => c.IsEnabled).OrderByDescending(c => c.LastActivationTimeStamp.GetValueOrDefault()).FirstOrDefault();
			SelectedContentIndex = Children.IndexOf(lastActivatedDocument);
		}

		/// <summary>
		/// Executes the on parent children collection changed operation.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The e.</param>
		private void OnParentChildrenCollectionChanged(object sender, EventArgs e) => RaisePropertyChanged(nameof(IsDirectlyHostedInFloatingWindow));
	}
}