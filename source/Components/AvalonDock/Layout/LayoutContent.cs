using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Xml.Serialization;
using AvalonDock.Controls;

namespace AvalonDock.Layout
{
	/// <summary>
	/// Provides a base class for layout content.
	/// </summary>
	[ContentProperty(nameof(Content))]
	[Serializable]
	public abstract class LayoutContent : LayoutElement, ILayoutElementForFloatingWindow, IComparable<LayoutContent>, ILayoutPreviousContainer, Core.Serialization.ISerializableLayoutContent, Core.Serialization.ISerializablePreviousContainer
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="LayoutContent"/> class.
		/// </summary>
		internal LayoutContent()
		{
		}

		/// <summary>
		/// Occurs when the closed event is raised.
		/// </summary>
		public event EventHandler Closed;

		/// <summary>
		/// Occurs when the closing event is raised.
		/// </summary>
		public event EventHandler<CancelEventArgs> Closing;

		/// <summary>
		/// Occurs when the floating properties updated event is raised.
		/// </summary>
		public event EventHandler FloatingPropertiesUpdated;

		/// <summary>
		/// Identifies the <see cref="Title"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(nameof(Title), typeof(string), typeof(LayoutContent), new UIPropertyMetadata(null, OnTitlePropertyChanged, CoerceTitleValue));

		/// <summary>
		/// Gets or sets the title.
		/// </summary>
		public string Title
		{
			get => (string)GetValue(TitleProperty);
			set => SetValue(TitleProperty, value);
		}

		/// <summary>
		/// Executes the coerce title value operation.
		/// </summary>
		/// <param name="obj">The object instance.</param>
		/// <param name="value">The value.</param>
		/// <returns>The resulting value.</returns>
		private static object CoerceTitleValue(DependencyObject obj, object value)
		{
			var lc = (LayoutContent)obj;
			if ((string)value != lc.Title) lc.RaisePropertyChanging(TitleProperty.Name);
			return value;
		}

		/// <summary>
		/// Executes the on title property changed operation.
		/// </summary>
		/// <param name="obj">The object instance.</param>
		/// <param name="args">The event arguments.</param>
		private static void OnTitlePropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args) => ((LayoutContent)obj).RaisePropertyChanged(TitleProperty.Name);

		[NonSerialized]
		private object _content = null;

		/// <summary>
		/// Gets or sets the content.
		/// </summary>
		[XmlIgnore]
		public object Content
		{
			get => _content;
			set
			{
				if (value == _content) return;
				RaisePropertyChanging(nameof(Content));
				_content = value;
				RaisePropertyChanged(nameof(Content));
				if (ContentId == null) SetContentIdFromContent();
			}
		}

		/// <summary>
		/// Identifies the <see cref="ContentId"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty ContentIdProperty = DependencyProperty.Register(nameof(ContentId), typeof(string), typeof(LayoutContent), new UIPropertyMetadata(null, OnContentIdPropertyChanged));

		/// <summary>
		/// Gets or sets the content id.
		/// </summary>
		public string ContentId
		{
			get
			{
				var value = (string)GetValue(ContentIdProperty);
				if (!string.IsNullOrWhiteSpace(value)) return value;
				// #83 - if Content.Name is empty at setting content and will be set later, ContentId will stay null.
				SetContentIdFromContent();
				return (string)GetValue(ContentIdProperty);
			}
			set => SetValue(ContentIdProperty, value);
		}

		/// <summary>
		/// Executes the on content id property changed operation.
		/// </summary>
		/// <param name="obj">The object instance.</param>
		/// <param name="args">The event arguments.</param>
		private static void OnContentIdPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
		{
			if (obj is LayoutContent layoutContent) layoutContent.OnContentIdPropertyChanged((string)args.OldValue, (string)args.NewValue);
		}

		/// <summary>
		/// Executes the on content id property changed operation.
		/// </summary>
		/// <param name="oldValue">The previous value.</param>
		/// <param name="newValue">The new value.</param>
		private void OnContentIdPropertyChanged(string oldValue, string newValue)
		{
			if (oldValue != newValue) RaisePropertyChanged(nameof(ContentId));
		}

		/// <summary>
		/// Sets the content id from content.
		/// </summary>
		private void SetContentIdFromContent()
		{
			var contentAsControl = _content as FrameworkElement;
			if (!string.IsNullOrWhiteSpace(contentAsControl?.Name)) SetCurrentValue(ContentIdProperty, contentAsControl.Name);
		}

		private bool _isSelected = false;

		/// <summary>
		/// Gets or sets a value indicating whether this instance is selected.
		/// </summary>
		public bool IsSelected
		{
			get => _isSelected;
			set
			{
				if (value == _isSelected) return;
				var oldValue = _isSelected;
				RaisePropertyChanging(nameof(IsSelected));
				_isSelected = value;
				if (Parent is ILayoutContentSelector parentSelector) parentSelector.SelectedContentIndex = _isSelected ? parentSelector.IndexOf(this) : -1;
				OnIsSelectedChanged(oldValue, value);
				RaisePropertyChanged(nameof(IsSelected));
				LayoutAnchorableTabItem.CancelMouseLeave();
			}
		}

		/// <summary>
		/// Executes the on is selected changed operation.
		/// </summary>
		/// <param name="oldValue">The previous value.</param>
		/// <param name="newValue">The new value.</param>
		protected virtual void OnIsSelectedChanged(bool oldValue, bool newValue) => IsSelectedChanged?.Invoke(this, EventArgs.Empty);

		/// <summary>
		/// Occurs when the is selected changed event is raised.
		/// </summary>
		public event EventHandler IsSelectedChanged;

		[field: NonSerialized]
		private bool _isActive = false;

		/// <summary>
		/// Gets or sets a value indicating whether this instance is active.
		/// </summary>
		[XmlIgnore]
		public bool IsActive
		{
			get => _isActive;
			set
			{
				if (value == _isActive) return;
				RaisePropertyChanging(nameof(IsActive));
				var oldValue = _isActive;
				_isActive = value;
				var root = Root;
				if (root != null)
				{
					if (root.ActiveContent != this && value) Root.ActiveContent = this;
					if (_isActive && root.ActiveContent != this) root.ActiveContent = this;
				}

				if (_isActive) IsSelected = true;
				OnIsActiveChanged(oldValue, value);
				RaisePropertyChanged(nameof(IsActive));
			}
		}

		/// <summary>
		/// Executes the on is active changed operation.
		/// </summary>
		/// <param name="oldValue">The previous value.</param>
		/// <param name="newValue">The new value.</param>
		protected virtual void OnIsActiveChanged(bool oldValue, bool newValue)
		{
			if (newValue) LastActivationTimeStamp = DateTime.Now;
			IsActiveChanged?.Invoke(this, EventArgs.Empty);
		}

		/// <summary>
		/// Occurs when the is active changed event is raised.
		/// </summary>
		public event EventHandler IsActiveChanged;

		private bool _isLastFocusedDocument = false;

		/// <summary>
		/// Gets a value indicating whether this instance is the last focused document.
		/// </summary>
		public bool IsLastFocusedDocument
		{
			get => _isLastFocusedDocument;
			internal set
			{
				if (value == _isLastFocusedDocument) return;
				RaisePropertyChanging(nameof(IsLastFocusedDocument));
				_isLastFocusedDocument = value;
				RaisePropertyChanged(nameof(IsLastFocusedDocument));
			}
		}

		[field: NonSerialized]
		private ILayoutContainer _previousContainer = null;

		/// <inheritdoc/>
		[XmlIgnore]
		ILayoutContainer ILayoutPreviousContainer.PreviousContainer
		{
			get => _previousContainer;
			set
			{
				if (value == _previousContainer) return;
				_previousContainer = value;
				RaisePropertyChanged(nameof(PreviousContainer));
				if (_previousContainer is ILayoutPaneSerializable paneSerializable && paneSerializable.Id == null)
					paneSerializable.Id = Guid.NewGuid().ToString();
			}
		}

		/// <summary>
		/// Gets or sets the previous container.
		/// </summary>
		protected ILayoutContainer PreviousContainer
		{
			get => ((ILayoutPreviousContainer)this).PreviousContainer;
			set => ((ILayoutPreviousContainer)this).PreviousContainer = value;
		}

		/// <inheritdoc/>
		[XmlIgnore]
		string ILayoutPreviousContainer.PreviousContainerId { get; set; }

		/// <summary>
		/// Gets or sets the previous container id.
		/// </summary>
		protected string PreviousContainerId
		{
			get => ((ILayoutPreviousContainer)this).PreviousContainerId;
			set => ((ILayoutPreviousContainer)this).PreviousContainerId = value;
		}

		[field: NonSerialized]
		private int _previousContainerIndex = -1;

		/// <summary>
		/// Gets or sets the previous container index.
		/// </summary>
		[XmlIgnore]
		public int PreviousContainerIndex
		{
			get => _previousContainerIndex;
			set
			{
				if (value == _previousContainerIndex) return;
				_previousContainerIndex = value;
				RaisePropertyChanged(nameof(PreviousContainerIndex));
			}
		}

		private DateTime? _lastActivationTimeStamp = null;

		/// <summary>
		/// Gets or sets the last activation time stamp.
		/// </summary>
		public DateTime? LastActivationTimeStamp
		{
			get => _lastActivationTimeStamp;
			set
			{
				if (value == _lastActivationTimeStamp) return;
				_lastActivationTimeStamp = value;
				RaisePropertyChanged(nameof(LastActivationTimeStamp));
			}
		}

		private double _floatingWidth = 0.0;

		/// <summary>
		/// Gets or sets the floating width.
		/// </summary>
		public double FloatingWidth
		{
			get => _floatingWidth;
			set
			{
				if (value == _floatingWidth) return;
				RaisePropertyChanging(nameof(FloatingWidth));
				_floatingWidth = value;
				RaisePropertyChanged(nameof(FloatingWidth));
			}
		}

		private double _floatingHeight = 0.0;

		/// <summary>
		/// Gets or sets the floating height.
		/// </summary>
		public double FloatingHeight
		{
			get => _floatingHeight;
			set
			{
				if (value == _floatingHeight) return;
				RaisePropertyChanging(nameof(FloatingHeight));
				_floatingHeight = value;
				RaisePropertyChanged(nameof(FloatingHeight));
			}
		}

		private double _floatingLeft = 0.0;

		/// <summary>
		/// Gets or sets the floating left.
		/// </summary>
		public double FloatingLeft
		{
			get => _floatingLeft;
			set
			{
				if (value == _floatingLeft) return;
				RaisePropertyChanging(nameof(FloatingLeft));
				_floatingLeft = value;
				RaisePropertyChanged(nameof(FloatingLeft));
			}
		}

		private double _floatingTop = 0.0;

		/// <summary>
		/// Gets or sets the floating top.
		/// </summary>
		public double FloatingTop
		{
			get => _floatingTop;
			set
			{
				if (value == _floatingTop) return;
				RaisePropertyChanging(nameof(FloatingTop));
				_floatingTop = value;
				RaisePropertyChanged(nameof(FloatingTop));
			}
		}

		private bool _isMaximized = false;

		/// <summary>
		/// Gets or sets a value indicating whether this instance is maximized.
		/// </summary>
		public bool IsMaximized
		{
			get => _isMaximized;
			set
			{
				if (value == _isMaximized) return;
				RaisePropertyChanging(nameof(IsMaximized));
				_isMaximized = value;
				RaisePropertyChanged(nameof(IsMaximized));
			}
		}

		private object _toolTip = null;

		/// <summary>
		/// Gets or sets the tool tip.
		/// </summary>
		public object ToolTip
		{
			get => _toolTip;
			set
			{
				if (value == _toolTip) return;
				_toolTip = value;
				RaisePropertyChanged(nameof(ToolTip));
			}
		}

		/// <summary>
		/// Gets a value indicating whether this instance is floating.
		/// </summary>
		[Bindable(true)]
		[Description("Gets whether the content is currently floating or not.")]
		[Category("Other")]
		public bool IsFloating => this.FindParent<LayoutFloatingWindow>() != null;

		private ImageSource _iconSource = null;

		/// <summary>
		/// Gets or sets the icon source.
		/// </summary>
		public ImageSource IconSource
		{
			get => _iconSource;
			set
			{
				if (value == _iconSource) return;
				_iconSource = value;
				RaisePropertyChanged(nameof(IconSource));
			}
		}

		// BD: 14.08.2020 added _canCloseDefault to properly implement inverting _canClose default value in inheritors (e.g. LayoutAnchorable)
		//     Thus CanClose property will be serialized only when not equal to its default for given class
		//     With previous code it was not possible to serialize CanClose if set to true for LayoutAnchorable instance

		/// <summary>
		/// Stores the current close capability value.
		/// </summary>
		internal bool _canClose = true;

		// BD: 14.08.2020 added _canCloseDefault to properly implement inverting _canClose default value in inheritors (e.g. LayoutAnchorable)
		//     Thus CanClose property will be serialized only when not equal to its default for given class
		//     With previous code it was not possible to serialize CanClose if set to true for LayoutAnchorable instance

		/// <summary>
		/// Stores the default close capability value for serialization comparisons.
		/// </summary>
		internal bool _canCloseDefault = true;

		/// <summary>
		/// Gets or sets a value indicating whether this instance can close.
		/// </summary>
		public bool CanClose
		{
			get => _canClose;
			set
			{
				if (_canClose == value) return;
				_canClose = value;
				RaisePropertyChanged(nameof(CanClose));
			}
		}

		private bool _canFloat = true;

		/// <summary>
		/// Gets or sets a value indicating whether this instance can float.
		/// </summary>
		public bool CanFloat
		{
			get => _canFloat;
			set
			{
				if (value == _canFloat) return;
				_canFloat = value;
				RaisePropertyChanged(nameof(CanFloat));
			}
		}

		private bool _canShowOnHover = true;

		/// <summary>
		/// Gets or sets a value indicating whether this instance can show on hover.
		/// </summary>
		public bool CanShowOnHover
		{
			get => _canShowOnHover;
			set
			{
				if (value == _canShowOnHover) return;
				_canShowOnHover = value;
				RaisePropertyChanged(nameof(CanShowOnHover));
			}
		}

		private bool _isEnabled = true;

		/// <summary>
		/// Gets or sets a value indicating whether this instance is enabled.
		/// </summary>
		public bool IsEnabled
		{
			get => _isEnabled;
			set
			{
				if (value == _isEnabled) return;
				_isEnabled = value;
				RaisePropertyChanged(nameof(IsEnabled));
			}
		}

		/// <summary>
		/// Gets the tab item.
		/// </summary>
		public LayoutDocumentTabItem TabItem { get; internal set; }

		/// <summary>
		/// Executes the close operation.
		/// </summary>
		public abstract void Close();

		/// <summary>
		/// Executes the compare to operation.
		/// </summary>
		/// <param name="other">The other.</param>
		/// <returns>The resulting value.</returns>
		public int CompareTo(LayoutContent other)
		{
			if (Content is IComparable contentAsComparable)
				return contentAsComparable.CompareTo(other.Content);
			return string.Compare(Title, other.Title);
		}

		/// <summary>
		/// Executes the float operation.
		/// </summary>
		public void Float()
		{
			if (PreviousContainer != null && PreviousContainer.FindParent<LayoutFloatingWindow>() != null)
			{
				var currentContainer = Parent as ILayoutPane;
				var currentContainerIndex = (currentContainer as ILayoutGroup).IndexOfChild(this);
				var previousContainerAsLayoutGroup = PreviousContainer as ILayoutGroup;

				if (PreviousContainerIndex < previousContainerAsLayoutGroup.ChildrenCount)
					previousContainerAsLayoutGroup.InsertChildAt(PreviousContainerIndex, this);
				else
					previousContainerAsLayoutGroup.InsertChildAt(previousContainerAsLayoutGroup.ChildrenCount, this);

				PreviousContainer = currentContainer;
				PreviousContainerIndex = currentContainerIndex;
				IsSelected = true;
				IsActive = true;
				Root.CollectGarbage();
			}
			else
			{
				Root.Manager.StartDraggingFloatingWindowForContent(this, false);
				IsSelected = true;
				IsActive = true;
			}

			// BD: 14.08.2020 raise IsFloating property changed
			RaisePropertyChanged(nameof(IsFloating));
		}

		/// <summary>
		/// Executes the dock as document operation.
		/// </summary>
		public void DockAsDocument()
		{
			if (!(Root is LayoutRoot root)) throw new InvalidOperationException();
			var wasFloating = IsFloating;

			if (PreviousContainer is LayoutDocumentPane previousDocumentPane &&
				previousDocumentPane.FindParent<LayoutDocumentFloatingWindow>() == null)
			{
				Dock();
				return;
			}

			LayoutDocumentPane newParentPane = null;
			if (root.LastFocusedDocument != null &&
				root.LastFocusedDocument != this &&
				root.LastFocusedDocument.FindParent<LayoutDocumentFloatingWindow>() == null)
				newParentPane = root.LastFocusedDocument.Parent as LayoutDocumentPane;

			if (newParentPane == null)
			{
				newParentPane = root.Descendents()
					.OfType<LayoutDocumentPane>()
					.FirstOrDefault(pane => pane.FindParent<LayoutDocumentFloatingWindow>() == null);
			}

			if (newParentPane == null)
			{
				newParentPane = root.Descendents().OfType<LayoutDocumentPane>().FirstOrDefault();
			}

			if (newParentPane != null)
			{
				newParentPane.Children.Add(this);
				root.CollectGarbage();
			}

			IsSelected = true;
			IsActive = true;

			// BD: 14.08.2020 raise IsFloating property changed
			RaisePropertyChanged(nameof(IsFloating));

			if (wasFloating && !IsFloating)
				root.Manager?.RaiseContentDocked(this);
		}

		/// <summary>
		/// Executes the dock operation.
		/// </summary>
		public void Dock()
		{
			var wasFloating = IsFloating;

			if (PreviousContainer != null)
			{
				var currentContainer = Parent;
				var currentContainerIndex = currentContainer is ILayoutGroup ? (currentContainer as ILayoutGroup).IndexOfChild(this) : -1;
				var previousContainerAsLayoutGroup = PreviousContainer as ILayoutGroup;

				if (PreviousContainerIndex < previousContainerAsLayoutGroup.ChildrenCount)
					previousContainerAsLayoutGroup.InsertChildAt(PreviousContainerIndex, this);
				else
					previousContainerAsLayoutGroup.InsertChildAt(previousContainerAsLayoutGroup.ChildrenCount, this);

				if (currentContainerIndex > -1)
				{
					PreviousContainer = currentContainer;
					PreviousContainerIndex = currentContainerIndex;
				}
				else
				{
					PreviousContainer = null;
					PreviousContainerIndex = 0;
				}

				IsSelected = true;
				IsActive = true;
			}
			else
			{
				InternalDock();
			}

			Root.CollectGarbage();

			// BD: 14.08.2020 raise IsFloating property changed
			RaisePropertyChanged(nameof(IsFloating));

			if (wasFloating && !IsFloating)
				Root.Manager?.RaiseContentDocked(this);
		}

		/// <inheritdoc/>
		protected override void OnParentChanging(ILayoutContainer oldValue, ILayoutContainer newValue)
		{
			if (oldValue != null) IsSelected = false;

			base.OnParentChanging(oldValue, newValue);
		}

		/// <inheritdoc/>
		protected override void OnParentChanged(ILayoutContainer oldValue, ILayoutContainer newValue)
		{
			if (IsSelected && Parent is ILayoutContentSelector)
			{
				var parentSelector = Parent as ILayoutContentSelector;
				parentSelector.SelectedContentIndex = parentSelector.IndexOf(this);
			}

			base.OnParentChanged(oldValue, newValue);
		}

		/// <summary>
		/// Executes the test can close operation.
		/// </summary>
		/// <returns><see langword="true"/> if the operation succeeds; otherwise, <see langword="false"/>.</returns>
		internal bool TestCanClose()
		{
			var args = new CancelEventArgs();
			OnClosing(args);
			return !args.Cancel;
		}

		/// <summary>
		/// Executes the close internal operation.
		/// </summary>
		internal void CloseInternal()
		{
			var root = Root;
			var parentAsContainer = Parent;
			if (parentAsContainer == null)
			{
				return;
			}

			if (PreviousContainer == null)
			{
				var parentAsGroup = Parent as ILayoutGroup;
				PreviousContainer = parentAsContainer;
				if (parentAsGroup != null)
				{
					PreviousContainerIndex = parentAsGroup.IndexOfChild(this);
				}

				if (parentAsGroup is ILayoutPaneSerializable layoutPaneSerializable)
				{
					PreviousContainerId = layoutPaneSerializable.Id;
					// This parentAsGroup will be removed in the GarbageCollection below
					if (parentAsGroup.Children.Count() == 1 && parentAsGroup.Parent != null && Root.Manager != null)
					{
						Parent = Root.Manager.Layout;
						PreviousContainer = parentAsGroup.Parent;
						PreviousContainerIndex = -1;

						if (parentAsGroup.Parent is ILayoutPaneSerializable paneSerializable)
							PreviousContainerId = paneSerializable.Id;
						else
							PreviousContainerId = null;
					}
				}
			}

			parentAsContainer.RemoveChild(this);
			root?.CollectGarbage();
			OnClosed();
		}

		/// <summary>
		/// Executes the on closed operation.
		/// </summary>
		protected virtual void OnClosed() => Closed?.Invoke(this, EventArgs.Empty);

		/// <summary>
		/// Executes the on closing operation.
		/// </summary>
		/// <param name="args">The event arguments.</param>
		protected virtual void OnClosing(CancelEventArgs args) => Closing?.Invoke(this, args);

		/// <summary>
		/// Executes the internal dock operation.
		/// </summary>
		protected virtual void InternalDock()
		{
		}

		/// <inheritdoc/>
		void ILayoutElementForFloatingWindow.RaiseFloatingPropertiesUpdated() => FloatingPropertiesUpdated?.Invoke(this, EventArgs.Empty);

		/// <inheritdoc/>
		object Core.Serialization.ISerializableLayoutContent.IconSource
		{
			get => IconSource;
			set => IconSource = value as System.Windows.Media.ImageSource;
		}

		/// <inheritdoc/>
		Core.Serialization.ISerializableLayoutContainer Core.Serialization.ISerializablePreviousContainer.PreviousContainer
		{
			get => _previousContainer as Core.Serialization.ISerializableLayoutContainer;
			set => ((ILayoutPreviousContainer)this).PreviousContainer = value as ILayoutContainer;
		}

		/// <inheritdoc/>
		string Core.Serialization.ISerializablePreviousContainer.PreviousContainerId
		{
			get => ((ILayoutPreviousContainer)this).PreviousContainerId;
			set => ((ILayoutPreviousContainer)this).PreviousContainerId = value;
		}
	}
}