using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Serialization;

namespace AvalonDock.Layout
{
	/// <summary>
	/// Represents a layout anchorable.
	/// </summary>
	[Serializable]
	public class LayoutAnchorable : LayoutContent, Core.Serialization.ISerializableLayoutAnchorable
	{
		private double _autohideWidth = 0.0;
		private double _autohideMinWidth = 100.0;
		private double _autohideHeight = 0.0;
		private double _autohideMinHeight = 100.0;
		private bool _canHide = true;
		private bool _canAutoHide = true;
		private bool _canDockAsTabbedDocument = true;
		// BD: 17.08.2020 Remove that bodge and handle CanClose=false && CanHide=true in XAML
		// private bool _canCloseValueBeforeInternalSet;
		private bool _canMove = true;

		/// <summary>
		/// Initializes a new instance of the <see cref="LayoutAnchorable"/> class.
		/// </summary>
		public LayoutAnchorable()
		{
			// LayoutAnchorable will hide by default, not close.
			// BD: 14.08.2020 Inverting both _canClose and _canCloseDefault to false as anchorables are only hidden but not closed
			//     That would allow CanClose to be properly serialized if set to true for an instance of LayoutAnchorable
			_canClose = _canCloseDefault = false;
		}

		/// <summary>
		/// Occurs when the is visible changed event is raised.
		/// </summary>
		public event EventHandler IsVisibleChanged;

		/// <summary>
		/// Occurs when the hiding event is raised.
		/// </summary>
		public event EventHandler<CancelEventArgs> Hiding;

		/// <summary>
		/// Gets or sets the auto hide width.
		/// </summary>
		public double AutoHideWidth
		{
			get => _autohideWidth;
			set
			{
				if (value == _autohideWidth) return;
				RaisePropertyChanging(nameof(AutoHideWidth));
				value = Math.Max(value, _autohideMinWidth);
				_autohideWidth = value;
				RaisePropertyChanged(nameof(AutoHideWidth));
			}
		}

		/// <summary>
		/// Gets or sets the auto hide min width.
		/// </summary>
		public double AutoHideMinWidth
		{
			get => _autohideMinWidth;
			set
			{
				if (value == _autohideMinWidth) return;
				RaisePropertyChanging(nameof(AutoHideMinWidth));
				if (value < 0) throw new ArgumentOutOfRangeException("Negative value is not allowed.", nameof(value));
				_autohideMinWidth = value;
				RaisePropertyChanged(nameof(AutoHideMinWidth));
			}
		}

		/// <summary>
		/// Gets or sets the auto hide height.
		/// </summary>
		public double AutoHideHeight
		{
			get => _autohideHeight;
			set
			{
				if (value == _autohideHeight) return;
				RaisePropertyChanging(nameof(AutoHideHeight));
				value = Math.Max(value, _autohideMinHeight);
				_autohideHeight = value;
				RaisePropertyChanged(nameof(AutoHideHeight));
			}
		}

		/// <summary>
		/// Gets or sets the auto hide min height.
		/// </summary>
		public double AutoHideMinHeight
		{
			get => _autohideMinHeight;
			set
			{
				if (value == _autohideMinHeight) return;
				RaisePropertyChanging(nameof(AutoHideMinHeight));
				if (value < 0) throw new ArgumentOutOfRangeException("Negative value is not allowed.", nameof(value));

				_autohideMinHeight = value;
				RaisePropertyChanged(nameof(AutoHideMinHeight));
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether this instance can hide.
		/// </summary>
		public bool CanHide
		{
			get => _canHide;
			set
			{
				if (value == _canHide) return;
				_canHide = value;
				RaisePropertyChanged(nameof(CanHide));
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether this instance can auto hide.
		/// </summary>
		public bool CanAutoHide
		{
			get => _canAutoHide;
			set
			{
				if (value == _canAutoHide) return;
				_canAutoHide = value;
				RaisePropertyChanged(nameof(CanAutoHide));
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether this instance can dock as tabbed document.
		/// </summary>
		public bool CanDockAsTabbedDocument
		{
			get => _canDockAsTabbedDocument;
			set
			{
				if (_canDockAsTabbedDocument == value) return;
				_canDockAsTabbedDocument = value;
				RaisePropertyChanged(nameof(CanDockAsTabbedDocument));
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether this instance can move.
		/// </summary>
		public bool CanMove
		{
			get => _canMove;
			set
			{
				if (value == _canMove) return;
				_canMove = value;
				RaisePropertyChanged(nameof(CanMove));
			}
		}

		/// <summary>
		/// Gets a value indicating whether this instance is auto hidden.
		/// </summary>
		public bool IsAutoHidden => Parent is LayoutAnchorGroup;

		/// <summary>
		/// Gets a value indicating whether this instance is hidden.
		/// </summary>
		[XmlIgnore]
		public bool IsHidden => Parent is LayoutRoot;

		/// <summary>
		/// Gets or sets a value indicating whether this instance is visible.
		/// </summary>
		[XmlIgnore]
		public bool IsVisible
		{
			get => Parent != null && !(Parent is LayoutRoot);
			set { if (value) Show(); else Hide(); }
		}

		/// <inheritdoc/>
		protected override void OnParentChanged(ILayoutContainer oldValue, ILayoutContainer newValue)
		{
			UpdateParentVisibility();
			RaisePropertyChanged(nameof(IsVisible));
			NotifyIsVisibleChanged();
			RaisePropertyChanged(nameof(IsHidden));
			RaisePropertyChanged(nameof(IsAutoHidden));
			base.OnParentChanged(oldValue, newValue);
		}

		/// <inheritdoc/>
		protected override void InternalDock()
		{
			var root = Root as LayoutRoot;
			LayoutAnchorablePane anchorablePane = null;

			// look for active content parent pane
			if (root.ActiveContent != null && root.ActiveContent != this) anchorablePane = root.ActiveContent.Parent as LayoutAnchorablePane;
			// look for a pane on the right side
			if (anchorablePane == null) anchorablePane = root.Descendents().OfType<LayoutAnchorablePane>().FirstOrDefault(pane => !pane.IsHostedInFloatingWindow && pane.GetSide() == AnchorSide.Right);
			// look for an available pane
			if (anchorablePane == null) anchorablePane = root.Descendents().OfType<LayoutAnchorablePane>().FirstOrDefault();
			var added = false;
			if (root.Manager.LayoutUpdateStrategy != null)
				added = root.Manager.LayoutUpdateStrategy.BeforeInsertAnchorable(root, this, anchorablePane);

			if (!added)
			{
				if (anchorablePane == null)
				{
					var mainLayoutPanel = new LayoutPanel { Orientation = Orientation.Horizontal };
					if (root.RootPanel != null)
						mainLayoutPanel.Children.Add(root.RootPanel);

					root.RootPanel = mainLayoutPanel;
					anchorablePane = new LayoutAnchorablePane { DockWidth = new GridLength(200.0, GridUnitType.Pixel) };
					mainLayoutPanel.Children.Add(anchorablePane);
				}

				anchorablePane.Children.Add(this);
			}

			root.Manager.LayoutUpdateStrategy?.AfterInsertAnchorable(root, this);
			base.InternalDock();
		}

		/// <inheritdoc/>
		public override void Close()
		{
			if (Root?.Manager != null)
			{
				var dockingManager = Root.Manager;
				dockingManager.ExecuteCloseCommand(this);
			}
			else
			{
				CloseAnchorable();
			}
		}
#if TRACE
		/// <inheritdoc />
		public override void ConsoleDump(int tab)
		{
			System.Diagnostics.Trace.Write(new string(' ', tab * 4));
			System.Diagnostics.Trace.WriteLine("Anchorable()");
		}
#endif

		/// <summary>
		/// Executes the on hiding operation.
		/// </summary>
		/// <param name="args">The event arguments.</param>
		protected virtual void OnHiding(CancelEventArgs args) => Hiding?.Invoke(this, args);

		/// <summary>
		/// Executes the hide operation.
		/// </summary>
		public void Hide()
		{
			if (Root?.Manager is DockingManager dockingManager)
				dockingManager.ExecuteHideCommand(this);
			else
				HideAnchorable(true);
		}

		/// <summary>
		/// Executes the hide anchorable operation.
		/// </summary>
		/// <param name="cancelable">The cancelable.</param>
		/// <returns><see langword="true"/> if the operation succeeds; otherwise, <see langword="false"/>.</returns>
		public bool HideAnchorable(bool cancelable)
		{
			if (!IsVisible)
			{
				IsSelected = true;
				IsActive = true;
				return false;
			}

			if (cancelable)
			{
				var args = new CancelEventArgs();
				OnHiding(args);
				if (args.Cancel) return false;
			}

			RaisePropertyChanging(nameof(IsHidden));
			RaisePropertyChanging(nameof(IsVisible));
			if (Parent is ILayoutGroup)
			{
				var parentAsGroup = Parent as ILayoutGroup;
				PreviousContainer = parentAsGroup;
				PreviousContainerIndex = parentAsGroup.IndexOfChild(this);
			}

			Root?.Hidden?.Add(this);
			RaisePropertyChanged(nameof(IsVisible));
			RaisePropertyChanged(nameof(IsHidden));
			NotifyIsVisibleChanged();

			return true;
		}

		/// <summary>
		/// Executes the show operation.
		/// </summary>
		public void Show()
		{
			if (IsVisible) return;
			if (!IsHidden) throw new InvalidOperationException();
			RaisePropertyChanging(nameof(IsHidden));
			RaisePropertyChanging(nameof(IsVisible));
			var added = false;
			var root = Root;
			if (root?.Manager?.LayoutUpdateStrategy != null)
				added = root.Manager.LayoutUpdateStrategy.BeforeInsertAnchorable(root as LayoutRoot, this, PreviousContainer);

			if (!added && PreviousContainer is ILayoutGroup previousContainerAsLayoutGroup)
			{
				if (PreviousContainerIndex >= 0 && PreviousContainerIndex < previousContainerAsLayoutGroup.ChildrenCount)
					previousContainerAsLayoutGroup.InsertChildAt(PreviousContainerIndex, this);
				else
					previousContainerAsLayoutGroup.InsertChildAt(previousContainerAsLayoutGroup.ChildrenCount, this);

				Parent = previousContainerAsLayoutGroup;
				IsSelected = true;
				IsActive = true;
			}

			root?.Manager?.LayoutUpdateStrategy?.AfterInsertAnchorable(root as LayoutRoot, this);
			PreviousContainer = null;
			PreviousContainerIndex = -1;
			RaisePropertyChanged(nameof(IsVisible));
			RaisePropertyChanged(nameof(IsHidden));
			NotifyIsVisibleChanged();
		}

		/// <summary>
		/// Executes the add to layout operation.
		/// </summary>
		/// <param name="manager">The manager.</param>
		/// <param name="strategy">The strategy.</param>
		public void AddToLayout(DockingManager manager, AnchorableShowStrategy strategy)
		{
			if (IsVisible || IsHidden) throw new InvalidOperationException();

			var most = (strategy & AnchorableShowStrategy.Most) == AnchorableShowStrategy.Most;
			var left = (strategy & AnchorableShowStrategy.Left) == AnchorableShowStrategy.Left;
			var right = (strategy & AnchorableShowStrategy.Right) == AnchorableShowStrategy.Right;
			var top = (strategy & AnchorableShowStrategy.Top) == AnchorableShowStrategy.Top;
			var bottom = (strategy & AnchorableShowStrategy.Bottom) == AnchorableShowStrategy.Bottom;

			if (!most)
			{
				var side = AnchorSide.Left;
				if (left) side = AnchorSide.Left;
				if (right) side = AnchorSide.Right;
				if (top) side = AnchorSide.Top;
				if (bottom) side = AnchorSide.Bottom;

				var anchorablePane = manager.Layout.Descendents().OfType<LayoutAnchorablePane>().FirstOrDefault(p => p.GetSide() == side);
				if (anchorablePane != null)
					anchorablePane.Children.Add(this);
				else
					most = true;
			}

			if (!most) return;
			if (manager.Layout.RootPanel == null)
				manager.Layout.RootPanel = new LayoutPanel { Orientation = left || right ? Orientation.Horizontal : Orientation.Vertical };

			if (left || right)
			{
				if (manager.Layout.RootPanel.Orientation == Orientation.Vertical && manager.Layout.RootPanel.ChildrenCount > 1)
					manager.Layout.RootPanel = new LayoutPanel(manager.Layout.RootPanel);
				manager.Layout.RootPanel.Orientation = Orientation.Horizontal;
				if (left)
					manager.Layout.RootPanel.Children.Insert(0, new LayoutAnchorablePane(this));
				else
					manager.Layout.RootPanel.Children.Add(new LayoutAnchorablePane(this));
			}
			else
			{
				if (manager.Layout.RootPanel.Orientation == Orientation.Horizontal && manager.Layout.RootPanel.ChildrenCount > 1)
					manager.Layout.RootPanel = new LayoutPanel(manager.Layout.RootPanel);
				manager.Layout.RootPanel.Orientation = Orientation.Vertical;
				if (top)
					manager.Layout.RootPanel.Children.Insert(0, new LayoutAnchorablePane(this));
				else
					manager.Layout.RootPanel.Children.Add(new LayoutAnchorablePane(this));
			}
		}

		/// <summary>
		/// Executes the toggle auto hide operation.
		/// </summary>
		public void ToggleAutoHide()
		{
			if (IsAutoHidden)
			{
				var parentGroup = Parent as LayoutAnchorGroup;
				var parentSide = parentGroup.Parent as LayoutAnchorSide;
				var previousContainer = ((ILayoutPreviousContainer)parentGroup).PreviousContainer as LayoutAnchorablePane;

				if (previousContainer == null)
				{
					var side = ((LayoutAnchorSide)parentGroup.Parent).Side;
					previousContainer = new LayoutAnchorablePane
					{
						DockMinWidth = AutoHideMinWidth,
						DockMinHeight = AutoHideMinHeight
					};

					var root = parentGroup.Root as LayoutRoot;
					var engine = root?.Manager?.LayoutEngine;
					if (engine != null)
					{
						engine.InsertPane(root, previousContainer, side);
					}
					else
					{
						// Fallback when no manager is available (e.g. in tests)
						new DefaultLayoutEngine().InsertPane(root, previousContainer, side);
					}
				}
				else
				{
					// I'm about to remove parentGroup, redirect any content (ie hidden contents) that point to it
					// to previousContainer
					var root = parentGroup.Root as LayoutRoot;
					foreach (var cnt in root.Descendents().OfType<ILayoutPreviousContainer>().Where(c => c.PreviousContainer == parentGroup))
						cnt.PreviousContainer = previousContainer;
				}

				var selectedIndex = -1;
				var selectedItem = parentGroup.Children.FirstOrDefault(x => x.IsActive);
				if (selectedItem != null)
					selectedIndex = parentGroup.Children.IndexOf(selectedItem);

				foreach (var anchorableToToggle in parentGroup.Children.ToArray())
					previousContainer.Children.Add(anchorableToToggle);

				if (selectedIndex != -1)
					previousContainer.SelectedContentIndex = selectedIndex;

				parentSide.Children.Remove(parentGroup);

				var parent = previousContainer.Parent as LayoutGroupBase;
				while (parent != null)
				{
					if (parent is LayoutGroup<ILayoutPanelElement> layoutGroup)
						layoutGroup.ComputeVisibility();
					parent = parent.Parent as LayoutGroupBase;
				}
			}
			else if (Parent is LayoutAnchorablePane)
			{
				var root = Root;
				var parentPane = Parent as LayoutAnchorablePane;
				var newAnchorGroup = new LayoutAnchorGroup();
				((ILayoutPreviousContainer)newAnchorGroup).PreviousContainer = parentPane;

				foreach (var anchorableToImport in parentPane.Children.ToArray())
					newAnchorGroup.Children.Add(anchorableToImport);

				// detect anchor side for the pane
				var anchorSide = parentPane.GetSide();

				switch (anchorSide)
				{
					case AnchorSide.Right: root.RightSide?.Children.Add(newAnchorGroup); break;
					case AnchorSide.Left: root.LeftSide?.Children.Add(newAnchorGroup); break;
					case AnchorSide.Top: root.TopSide?.Children.Add(newAnchorGroup); break;
					case AnchorSide.Bottom: root.BottomSide?.Children.Add(newAnchorGroup); break;
				}
			}
		}

		/// <summary>
		/// Executes the toggle single auto hide operation.
		/// </summary>
		public void ToggleSingleAutoHide()
		{
			if (IsAutoHidden)
			{
				// Move from LayoutAnchorGroup back to a docked pane (same logic as ToggleAutoHide for single item)
				var parentGroup = Parent as LayoutAnchorGroup;
				if (parentGroup == null) return;
				var parentSide = parentGroup.Parent as LayoutAnchorSide;
				if (parentSide == null) return;

				var previousContainer = ((ILayoutPreviousContainer)parentGroup).PreviousContainer as LayoutAnchorablePane;

				// If previousContainer was removed from the tree (detached), treat as null
				if (previousContainer != null && previousContainer.Root == null)
					previousContainer = null;

				if (previousContainer == null)
				{
					var side = parentSide.Side;
					previousContainer = new LayoutAnchorablePane
					{
						DockMinWidth = AutoHideMinWidth,
						DockMinHeight = AutoHideMinHeight
					};

					switch (side)
					{
						case AnchorSide.Right:
							if (parentGroup.Root.RootPanel.Orientation == Orientation.Horizontal)
							{
								parentGroup.Root.RootPanel.Children.Add(previousContainer);
							}
							else
							{
								var panel = new LayoutPanel { Orientation = Orientation.Horizontal };
								var root = parentGroup.Root as LayoutRoot;
								var oldRootPanel = parentGroup.Root.RootPanel;
								root.RootPanel = panel;
								panel.Children.Add(oldRootPanel);
								panel.Children.Add(previousContainer);
							}

							break;
						case AnchorSide.Left:
							if (parentGroup.Root.RootPanel.Orientation == Orientation.Horizontal)
							{
								parentGroup.Root.RootPanel.Children.Insert(0, previousContainer);
							}
							else
							{
								var panel = new LayoutPanel { Orientation = Orientation.Horizontal };
								var root = parentGroup.Root as LayoutRoot;
								var oldRootPanel = parentGroup.Root.RootPanel;
								root.RootPanel = panel;
								panel.Children.Add(previousContainer);
								panel.Children.Add(oldRootPanel);
							}

							break;
						case AnchorSide.Top:
							if (parentGroup.Root.RootPanel.Orientation == Orientation.Vertical)
							{
								parentGroup.Root.RootPanel.Children.Insert(0, previousContainer);
							}
							else
							{
								var panel = new LayoutPanel { Orientation = Orientation.Vertical };
								var root = parentGroup.Root as LayoutRoot;
								var oldRootPanel = parentGroup.Root.RootPanel;
								root.RootPanel = panel;
								panel.Children.Add(previousContainer);
								panel.Children.Add(oldRootPanel);
							}

							break;
						case AnchorSide.Bottom:
							if (parentGroup.Root.RootPanel.Orientation == Orientation.Vertical)
							{
								parentGroup.Root.RootPanel.Children.Add(previousContainer);
							}
							else
							{
								var panel = new LayoutPanel { Orientation = Orientation.Vertical };
								var root = parentGroup.Root as LayoutRoot;
								var oldRootPanel = parentGroup.Root.RootPanel;
								root.RootPanel = panel;
								panel.Children.Add(oldRootPanel);
								panel.Children.Add(previousContainer);
							}

							break;
					}
				}

				// Move only THIS anchorable (not siblings)
				parentGroup.Children.Remove(this);
				previousContainer.Children.Add(this);

				// Clean up empty group
				if (parentGroup.Children.Count == 0)
					parentSide.Children.Remove(parentGroup);
			}
			else if (Parent is LayoutAnchorablePane parentPane)
			{
				// Move from docked pane to auto-hide anchor group (only this one)
				var root = Root;
				var anchorSide = parentPane.GetSide();
				var newAnchorGroup = new LayoutAnchorGroup();
				((ILayoutPreviousContainer)newAnchorGroup).PreviousContainer = parentPane;

				parentPane.Children.Remove(this);
				newAnchorGroup.Children.Add(this);

				switch (anchorSide)
				{
					case AnchorSide.Right: root.RightSide?.Children.Add(newAnchorGroup); break;
					case AnchorSide.Left: root.LeftSide?.Children.Add(newAnchorGroup); break;
					case AnchorSide.Top: root.TopSide?.Children.Add(newAnchorGroup); break;
					case AnchorSide.Bottom: root.BottomSide?.Children.Add(newAnchorGroup); break;
				}
			}
		}

		/// <summary>
		/// Executes the close anchorable operation.
		/// </summary>
		/// <returns><see langword="true"/> if the operation succeeds; otherwise, <see langword="false"/>.</returns>
		internal bool CloseAnchorable()
		{
			if (!TestCanClose()) return false;
			if (IsAutoHidden) ToggleAutoHide();
			CloseInternal();
			return true;
		}

		// BD: 17.08.2020 Remove that bodge and handle CanClose=false && CanHide=true in XAML
		// internal void SetCanCloseInternal(bool canClose)
		// {
		//     _canCloseValueBeforeInternalSet = _canClose;
		//     _canClose = canClose;
		// }
		// internal void ResetCanCloseInternal()
		// {
		//     _canClose = _canCloseValueBeforeInternalSet;
		// }

		/// <summary>
		/// Executes the notify is visible changed operation.
		/// </summary>
		private void NotifyIsVisibleChanged() => IsVisibleChanged?.Invoke(this, EventArgs.Empty);

		/// <summary>
		/// Updates the parent visibility.
		/// </summary>
		private void UpdateParentVisibility()
		{
			// Element is Hidden since it has no parent but a previous parent
			if (PreviousContainer != null && Parent == null)
			{
				// Go back to using previous parent
				Parent = PreviousContainer;
				////        PreviousContainer = null;
			}

			if (Parent is ILayoutElementWithVisibility parentPane)
				parentPane.ComputeVisibility();
		}
	}
}