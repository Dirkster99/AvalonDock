/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using System;
using System.Linq;
using System.Windows;
using System.Xml.Serialization;
using System.Windows.Controls;
using System.Globalization;
using System.ComponentModel;

namespace AvalonDock.Layout
{
	[Serializable]
	public class LayoutAnchorable : LayoutContent
	{
		#region fields
		private double _autohideWidth = 0.0;
		private double _autohideMinWidth = 100.0;
		private double _autohideHeight = 0.0;
		private double _autohideMinHeight = 100.0;
		private bool _canHide = true;
		private bool _canAutoHide = true;
		private bool _canDockAsTabbedDocument = true;
		private bool _canCloseValueBeforeInternalSet;
		#endregion fields

		#region Constructors

		public LayoutAnchorable()
		{
			// LayoutAnchorable will hide by default, not close.
			_canClose = false;
		}

		#endregion Constructors

		#region Events

		public event EventHandler IsVisibleChanged;
		public event EventHandler<CancelEventArgs> Hiding;

		#endregion Events

		#region Properties

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

		/// <summary>Get a value indicating if the anchorable is anchored to a border in an autohide status.</summary>
		public bool IsAutoHidden => Parent is LayoutAnchorGroup;

		[XmlIgnore]
		public bool IsHidden => Parent is LayoutRoot;

		[XmlIgnore]
		public bool IsVisible
		{
			get => Parent != null && !(Parent is LayoutRoot);
			set { if (value) Show(); else Hide(); }
		}

		#endregion Properties

		#region Overrides

		protected override void OnParentChanged(ILayoutContainer oldValue, ILayoutContainer newValue)
		{
			UpdateParentVisibility();
			RaisePropertyChanged(nameof(IsVisible));
			NotifyIsVisibleChanged();
			RaisePropertyChanged(nameof(IsHidden));
			RaisePropertyChanged(nameof(IsAutoHidden));
			base.OnParentChanged(oldValue, newValue);
		}

		protected override void InternalDock()
		{
			var root = Root as LayoutRoot;
			LayoutAnchorablePane anchorablePane = null;

			if (root.ActiveContent != null && root.ActiveContent != this)
			{
				//look for active content parent pane
				anchorablePane = root.ActiveContent.Parent as LayoutAnchorablePane;
			}

			if (anchorablePane == null)
			{
				//look for a pane on the right side
				anchorablePane = root.Descendents().OfType<LayoutAnchorablePane>().FirstOrDefault(pane => !pane.IsHostedInFloatingWindow && Extensions.GetSide(pane) == AnchorSide.Right);
			}

			if (anchorablePane == null)
			{
				//look for an available pane
				anchorablePane = root.Descendents().OfType<LayoutAnchorablePane>().FirstOrDefault();
			}

			var added = false;
			if (root.Manager.LayoutUpdateStrategy != null)
			{
				added = root.Manager.LayoutUpdateStrategy.BeforeInsertAnchorable(root, this, anchorablePane);
			}

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

		public override void ReadXml(System.Xml.XmlReader reader)
		{
			if (reader.MoveToAttribute("CanHide"))
				CanHide = bool.Parse(reader.Value);
			if (reader.MoveToAttribute("CanAutoHide"))
				CanAutoHide = bool.Parse(reader.Value);
			if (reader.MoveToAttribute("AutoHideWidth"))
				AutoHideWidth = double.Parse(reader.Value, CultureInfo.InvariantCulture);
			if (reader.MoveToAttribute("AutoHideHeight"))
				AutoHideHeight = double.Parse(reader.Value, CultureInfo.InvariantCulture);
			if (reader.MoveToAttribute("AutoHideMinWidth"))
				AutoHideMinWidth = double.Parse(reader.Value, CultureInfo.InvariantCulture);
			if (reader.MoveToAttribute("AutoHideMinHeight"))
				AutoHideMinHeight = double.Parse(reader.Value, CultureInfo.InvariantCulture);
			if (reader.MoveToAttribute("CanDockAsTabbedDocument"))
				CanDockAsTabbedDocument = bool.Parse(reader.Value);

			base.ReadXml(reader);
		}

		public override void WriteXml(System.Xml.XmlWriter writer)
		{
			if (!CanHide)
				writer.WriteAttributeString("CanHide", CanHide.ToString());
			if (!CanAutoHide)
				writer.WriteAttributeString("CanAutoHide", CanAutoHide.ToString(CultureInfo.InvariantCulture));
			if (AutoHideWidth > 0)
				writer.WriteAttributeString("AutoHideWidth", AutoHideWidth.ToString(CultureInfo.InvariantCulture));
			if (AutoHideHeight > 0)
				writer.WriteAttributeString("AutoHideHeight", AutoHideHeight.ToString(CultureInfo.InvariantCulture));
			if (AutoHideMinWidth != 25.0)
				writer.WriteAttributeString("AutoHideMinWidth", AutoHideMinWidth.ToString(CultureInfo.InvariantCulture));
			if (AutoHideMinHeight != 25.0)
				writer.WriteAttributeString("AutoHideMinHeight", AutoHideMinHeight.ToString(CultureInfo.InvariantCulture));
			if (!CanDockAsTabbedDocument)
				writer.WriteAttributeString("CanDockAsTabbedDocument", CanDockAsTabbedDocument.ToString(CultureInfo.InvariantCulture));

			base.WriteXml(writer);
		}

		public override void Close()
		{
			CloseAnchorable();
		}

#if TRACE
		public override void ConsoleDump(int tab)
		{
			System.Diagnostics.Trace.Write(new string(' ', tab * 4));
			System.Diagnostics.Trace.WriteLine("Anchorable()");
		}
#endif

		#endregion Overrides

		#region Public Methods

		/// <summary>Hide this contents.</summary>
		/// <remarks>Add this content to <see cref="ILayoutRoot.Hidden"/> collection of parent root.</remarks>
		public void Hide(bool cancelable = true)
		{
			if (!IsVisible)
			{
				IsSelected = true;
				IsActive = true;
				return;
			}

			if (cancelable)
			{
				var args = new CancelEventArgs();
				OnHiding(args);
				if (args.Cancel) return;
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
		}

		/// <summary>Show the content.</summary>
		/// <remarks>Try to show the content where it was previously hidden.</remarks>
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

			if (!added && PreviousContainer != null)
			{
				var previousContainerAsLayoutGroup = PreviousContainer as ILayoutGroup;
				if (PreviousContainerIndex < previousContainerAsLayoutGroup.ChildrenCount)
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

		/// <summary>Add the anchorable to a <see cref="DockingManager"/> layout.</summary>
		/// <param name="manager"></param>
		/// <param name="strategy"></param>
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

		public void ToggleAutoHide()
		{
			#region Anchorable is already auto hidden
			if (IsAutoHidden)
			{
				var parentGroup = Parent as LayoutAnchorGroup;
				var parentSide = parentGroup.Parent as LayoutAnchorSide;
				var previousContainer = ((ILayoutPreviousContainer)parentGroup).PreviousContainer as LayoutAnchorablePane;

				if (previousContainer == null)
				{
					var side = ((LayoutAnchorSide)parentGroup.Parent).Side;
					switch (side)
					{
						case AnchorSide.Right:
							if (parentGroup.Root.RootPanel.Orientation == Orientation.Horizontal)
							{
								previousContainer = new LayoutAnchorablePane { DockMinWidth = AutoHideMinWidth };
								parentGroup.Root.RootPanel.Children.Add(previousContainer);
							}
							else
							{
								previousContainer = new LayoutAnchorablePane();
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
								previousContainer = new LayoutAnchorablePane { DockMinWidth = AutoHideMinWidth };
								parentGroup.Root.RootPanel.Children.Insert(0, previousContainer);
							}
							else
							{
								previousContainer = new LayoutAnchorablePane();
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
								previousContainer = new LayoutAnchorablePane {DockMinHeight = AutoHideMinHeight};
								parentGroup.Root.RootPanel.Children.Insert(0, previousContainer);
							}
							else
							{
								previousContainer = new LayoutAnchorablePane();
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
								previousContainer = new LayoutAnchorablePane();
								previousContainer.DockMinHeight = AutoHideMinHeight;
								parentGroup.Root.RootPanel.Children.Add(previousContainer);
							}
							else
							{
								previousContainer = new LayoutAnchorablePane();
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
				else
				{
					//I'm about to remove parentGroup, redirect any content (ie hidden contents) that point to it
					//to previousContainer
					var root = parentGroup.Root as LayoutRoot;
					foreach (var cnt in root.Descendents().OfType<ILayoutPreviousContainer>().Where(c => c.PreviousContainer == parentGroup)) 
						cnt.PreviousContainer = previousContainer;
				}

				foreach (var anchorableToToggle in parentGroup.Children.ToArray()) 
					previousContainer.Children.Add(anchorableToToggle);

				parentSide.Children.Remove(parentGroup);

				var parent = previousContainer.Parent as LayoutGroupBase;
				while (parent != null)
				{
					if (parent is LayoutGroup<ILayoutPanelElement> layoutGroup) 
						layoutGroup.ComputeVisibility();
					parent = parent.Parent as LayoutGroupBase;
				}
			}
			#endregion Anchorable is already auto hidden

			#region Anchorable is docked
			else if (Parent is LayoutAnchorablePane)
			{
				var root = Root;
				var parentPane = Parent as LayoutAnchorablePane;
				var newAnchorGroup = new LayoutAnchorGroup();
				((ILayoutPreviousContainer)newAnchorGroup).PreviousContainer = parentPane;

				foreach (var anchorableToImport in parentPane.Children.ToArray())
					newAnchorGroup.Children.Add(anchorableToImport);

				//detect anchor side for the pane
				var anchorSide = parentPane.GetSide();

				switch (anchorSide)
				{
					case AnchorSide.Right: root.RightSide?.Children.Add(newAnchorGroup); break;
					case AnchorSide.Left: root.LeftSide?.Children.Add(newAnchorGroup); break;
					case AnchorSide.Top: root.TopSide?.Children.Add(newAnchorGroup); break;
					case AnchorSide.Bottom: root.BottomSide?.Children.Add(newAnchorGroup); break;
				}
			}
			#endregion Anchorable is docked
		}

		#endregion Public Methods

		#region Internal Methods

		protected virtual void OnHiding(CancelEventArgs args) => Hiding?.Invoke(this, args);

		internal void CloseAnchorable()
		{
			if (!TestCanClose()) return;
			if (IsAutoHidden) ToggleAutoHide();
			CloseInternal();
		}

		internal void SetCanCloseInternal(bool canClose)
		{
			_canCloseValueBeforeInternalSet = _canClose;
			_canClose = canClose;
		}

		internal void ResetCanCloseInternal()
		{
			_canClose = _canCloseValueBeforeInternalSet;
		}

		#endregion Internal Methods

		#region Private Methods

		private void NotifyIsVisibleChanged() => IsVisibleChanged?.Invoke(this, EventArgs.Empty);

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

		#endregion Private Methods
	}
}
