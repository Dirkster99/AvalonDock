/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using System;
using System.Linq;
using System.Windows;
using AvalonDock.Layout;
using System.Windows.Input;
using AvalonDock.Commands;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Controls;

namespace AvalonDock.Controls
{
	/// <inheritdoc />
	/// <summary>
	/// This class provides a basic wrapper around the custom content view of <see cref="LayoutElement"/>.
	/// Implements the <see cref="System.Windows.FrameworkElement" />
	/// </summary>
	/// <seealso cref="System.Windows.FrameworkElement" />
	public abstract class LayoutItem : FrameworkElement
	{
		#region fields
		private ICommand _defaultCloseCommand;
		private ICommand _defaultFloatCommand;
		private ICommand _defaultDockAsDocumentCommand;
		private ICommand _defaultCloseAllButThisCommand;
		private ICommand _defaultCloseAllCommand;
		private ICommand _defaultActivateCommand;
		private ICommand _defaultNewVerticalTabGroupCommand;
		private ICommand _defaultNewHorizontalTabGroupCommand;
		private ICommand _defaultMoveToNextTabGroupCommand;
		private ICommand _defaultMoveToPreviousTabGroupCommand;
		private ContentPresenter _view = null;
		private ReentrantFlag _isSelectedReentrantFlag = new ReentrantFlag();
		private ReentrantFlag _isActiveReentrantFlag = new ReentrantFlag();
		#endregion fields

		#region Constructors

		static LayoutItem()
		{
			ToolTipProperty.OverrideMetadata(typeof(LayoutItem), new FrameworkPropertyMetadata(null, OnToolTipChanged));
			VisibilityProperty.OverrideMetadata(typeof(LayoutItem), new FrameworkPropertyMetadata(Visibility.Visible, OnVisibilityChanged));
		}

		internal LayoutItem()
		{
		}

		#endregion

		#region Properties

		#region LayoutElement

		public LayoutContent LayoutElement { get; private set; }

		#endregion

		#region Model

		public object Model { get; private set; }

		#endregion

		#region View

		public ContentPresenter View
		{
			get
			{
				if (_view != null) return _view;
				_view = new ContentPresenter();
				_view.SetBinding(ContentPresenter.ContentProperty, new Binding(nameof(ContentPresenter.Content)) { Source = LayoutElement });
				if (LayoutElement?.Root == null) return _view;
				_view.SetBinding(ContentPresenter.ContentTemplateProperty, new Binding(nameof(DockingManager.LayoutItemTemplate)) { Source = LayoutElement.Root.Manager });
				_view.SetBinding(ContentPresenter.ContentTemplateSelectorProperty, new Binding(nameof(DockingManager.LayoutItemTemplateSelector)) { Source = LayoutElement.Root.Manager });
				LayoutElement.Root.Manager?.InternalAddLogicalChild(_view);
				return _view;
			}
		}

		#endregion

		#region Title

		/// <summary><see cref="Title"/> dependency property.</summary>
		public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(nameof(Title), typeof(string), typeof(LayoutItem),
				new FrameworkPropertyMetadata(null, OnTitleChanged));

		/// <summary>
		/// Gets or sets the <see cref="Title"/> property.  This dependency property 
		/// indicates the title of the element.
		/// </summary>
		public string Title
		{
			get => (string)GetValue(TitleProperty);
			set => SetValue(TitleProperty, value);
		}

		/// <summary>Handles changes to the <see cref="Title"/> property.</summary>
		private static void OnTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((LayoutItem)d).OnTitleChanged(e);

		/// <summary>Provides derived classes an opportunity to handle changes to the <see cref="Title"/> property.</summary>
		protected virtual void OnTitleChanged(DependencyPropertyChangedEventArgs e)
		{
			if (LayoutElement != null) LayoutElement.Title = (string)e.NewValue;
		}

		#endregion

		#region IconSource

		/// <summary><see cref="IconSource"/> dependency property.</summary>
		public static readonly DependencyProperty IconSourceProperty = DependencyProperty.Register(nameof(IconSource), typeof(ImageSource), typeof(LayoutItem),
				new FrameworkPropertyMetadata(null, OnIconSourceChanged));

		/// <summary>
		/// Gets or sets the <see cref="IconSource"/> property. This dependency property 
		/// indicates icon associated with the item.
		/// </summary>
		public ImageSource IconSource
		{
			get => (ImageSource)GetValue(IconSourceProperty);
			set => SetValue(IconSourceProperty, value);
		}

		/// <summary>Handles changes to the <see cref="IconSource"/> property.</summary>
		private static void OnIconSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((LayoutItem)d).OnIconSourceChanged(e);

		/// <summary>Provides derived classes an opportunity to handle changes to the <see cref="IconSource"/> property.</summary>
		protected virtual void OnIconSourceChanged(DependencyPropertyChangedEventArgs e)
		{
			if (LayoutElement != null) LayoutElement.IconSource = IconSource;
		}

		#endregion

		#region ContentId

		/// <summary><see cref="ContentId"/> dependency property.</summary>
		public static readonly DependencyProperty ContentIdProperty = DependencyProperty.Register(nameof(ContentId), typeof(string), typeof(LayoutItem),
				new FrameworkPropertyMetadata(null, OnContentIdChanged));

		/// <summary>
		/// Gets or sets the <see cref="ContentId"/> property. This dependency property 
		/// indicates the content id used to retrieve content when deserializing layouts.
		/// </summary>
		public string ContentId
		{
			get => (string)GetValue(ContentIdProperty);
			set => SetValue(ContentIdProperty, value);
		}

		/// <summary>Handles changes to the <see cref="ContentId"/> property.</summary>
		private static void OnContentIdChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((LayoutItem)d).OnContentIdChanged(e);

		/// <summary>Provides derived classes an opportunity to handle changes to the <see cref="ContentId"/> property.</summary>
		protected virtual void OnContentIdChanged(DependencyPropertyChangedEventArgs e)
		{
			if (LayoutElement != null) LayoutElement.ContentId = (string)e.NewValue;
		}

		#endregion

		#region IsSelected

		/// <summary><see cref="IsSelected"/> dependency property.</summary>
		public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register(nameof(IsSelected), typeof(bool), typeof(LayoutItem),
				new FrameworkPropertyMetadata(false, OnIsSelectedChanged));

		/// <summary>
		/// Gets or sets the <see cref="IsSelected"/> property. This dependency property 
		/// indicates if the item is selected inside its container.
		/// </summary>
		public bool IsSelected
		{
			get => (bool)GetValue(IsSelectedProperty);
			set => SetValue(IsSelectedProperty, value);
		}

		/// <summary>Handles changes to the <see cref="IsSelected"/> property.</summary>
		private static void OnIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((LayoutItem)d).OnIsSelectedChanged(e);

		/// <summary>Provides derived classes an opportunity to handle changes to the <see cref="IsSelected"/> property.</summary>
		protected virtual void OnIsSelectedChanged(DependencyPropertyChangedEventArgs e)
		{
			if (!_isSelectedReentrantFlag.CanEnter) return;
			using (_isSelectedReentrantFlag.Enter())
			{
				if (LayoutElement != null) LayoutElement.IsSelected = (bool)e.NewValue;
			}
		}

		#endregion

		#region IsActive  

		/// <summary><see cref="IsActive"/> dependency property.</summary>
		public static readonly DependencyProperty IsActiveProperty = DependencyProperty.Register(nameof(IsActive), typeof(bool), typeof(LayoutItem),
				new FrameworkPropertyMetadata(false, OnIsActiveChanged));

		/// <summary>
		/// Gets or sets the <see cref="IsActive"/> property.  This dependency property 
		/// indicates if the item is active in the UI.
		/// </summary>
		public bool IsActive
		{
			get => (bool)GetValue(IsActiveProperty);
			set => SetValue(IsActiveProperty, value);
		}

		/// <summary>Handles changes to the <see cref="IsActive"/> property.</summary>
		private static void OnIsActiveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((LayoutItem)d).OnIsActiveChanged(e);

		/// <summary>Provides derived classes an opportunity to handle changes to the <see cref="IsActive"/> property.</summary>
		protected virtual void OnIsActiveChanged(DependencyPropertyChangedEventArgs e)
		{
			if (!_isActiveReentrantFlag.CanEnter) return;
			using (_isActiveReentrantFlag.Enter())
			{
				if (LayoutElement != null) LayoutElement.IsActive = (bool)e.NewValue;
			}
		}

		#endregion

		#region CanClose

		/// <summary><see cref="CanClose"/> dependency property.</summary>
		public static readonly DependencyProperty CanCloseProperty = DependencyProperty.Register(nameof(CanClose), typeof(bool), typeof(LayoutItem),
				new FrameworkPropertyMetadata(true, OnCanCloseChanged));

		/// <summary>
		/// Gets or sets the <see cref="CanClose"/> property. This dependency property 
		/// indicates if the item can be closed.
		/// </summary>
		public bool CanClose
		{
			get => (bool)GetValue(CanCloseProperty);
			set => SetValue(CanCloseProperty, value);
		}

		/// <summary>Handles changes to the <see cref="CanClose"/> property.</summary>
		private static void OnCanCloseChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((LayoutItem)d).OnCanCloseChanged(e);

		/// <summary>Provides derived classes an opportunity to handle changes to the <see cref="CanClose"/> property.</summary>
		protected virtual void OnCanCloseChanged(DependencyPropertyChangedEventArgs e)
		{
			if (LayoutElement != null) LayoutElement.CanClose = (bool)e.NewValue;
		}

		#endregion

		#region CanFloat

		/// <summary><see cref="CanFloat"/> dependency property.</summary>
		public static readonly DependencyProperty CanFloatProperty = DependencyProperty.Register(nameof(CanFloat), typeof(bool), typeof(LayoutItem),
				new FrameworkPropertyMetadata(true, OnCanFloatChanged));

		/// <summary>
		/// Gets or sets the <see cref="CanFloat"/> property. This dependency property 
		/// indicates if user can move the layout element dragging it to another position.
		/// </summary>
		public bool CanFloat
		{
			get => (bool)GetValue(CanFloatProperty);
			set => SetValue(CanFloatProperty, value);
		}

		/// <summary>Handles changes to the <see cref="CanFloat"/> property.</summary>
		private static void OnCanFloatChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((LayoutItem)d).OnCanFloatChanged(e);

		/// <summary>Provides derived classes an opportunity to handle changes to the <see cref="CanFloat"/> property.</summary>
		protected virtual void OnCanFloatChanged(DependencyPropertyChangedEventArgs e)
		{
			if (LayoutElement != null) LayoutElement.CanFloat = (bool)e.NewValue;
		}

		#endregion

		#region CloseCommand

		/// <summary><see cref="CloseCommand"/> dependency property.</summary>
		public static readonly DependencyProperty CloseCommandProperty = DependencyProperty.Register(nameof(CloseCommand), typeof(ICommand), typeof(LayoutItem),
				new FrameworkPropertyMetadata(null, OnCloseCommandChanged, CoerceCloseCommandValue));

		/// <summary>
		/// Gets or sets the <see cref="CloseCommand"/>  property. This dependency property 
		/// indicates the command to execute when user click the document close button.
		/// </summary>
		public ICommand CloseCommand
		{
			get => (ICommand)GetValue(CloseCommandProperty);
			set => SetValue(CloseCommandProperty, value);
		}

		/// <summary>Handles changes to the <see cref="CloseCommand"/> property.</summary>
		private static void OnCloseCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((LayoutItem)d).OnCloseCommandChanged(e);

		/// <summary>Provides derived classes an opportunity to handle changes to the <see cref="CloseCommand"/>  property.</summary>
		protected virtual void OnCloseCommandChanged(DependencyPropertyChangedEventArgs e)
		{
		}

		/// <summary>Coerces the <see cref="CloseCommand"/>  value.</summary>
		private static object CoerceCloseCommandValue(DependencyObject d, object value) => value;

		private bool CanExecuteCloseCommand(object parameter) => LayoutElement != null && LayoutElement.CanClose;

		private void ExecuteCloseCommand(object parameter) => Close();

		protected abstract void Close();

		#endregion

		#region FloatCommand
		/// <summary><see cref="FloatCommand"/> dependency property.</summary>
		public static readonly DependencyProperty FloatCommandProperty = DependencyProperty.Register(nameof(FloatCommand), typeof(ICommand), typeof(LayoutItem),
				new FrameworkPropertyMetadata(null, OnFloatCommandChanged, CoerceFloatCommandValue));

		/// <summary>
		/// Gets or sets the <see cref="FloatCommand"/> property.
		/// This dependency property indicates the command to execute when user click the float button.
		/// </summary>
		/// <remarks>By default this command move the anchorable inside new floating window.</remarks>
		public ICommand FloatCommand
		{
			get => (ICommand)GetValue(FloatCommandProperty);
			set => SetValue(FloatCommandProperty, value);
		}

		/// <summary>Handles changes to the <see cref="FloatCommand"/> property.</summary>
		private static void OnFloatCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((LayoutItem)d).OnFloatCommandChanged(e);

		/// <summary>Provides derived classes an opportunity to handle changes to the <see cref="FloatCommand"/> property.</summary>
		protected virtual void OnFloatCommandChanged(DependencyPropertyChangedEventArgs e)
		{
		}

		/// <summary>Coerces the <see cref="FloatCommand"/> value.</summary>
		private static object CoerceFloatCommandValue(DependencyObject d, object value) => value;

		private bool CanExecuteFloatCommand(object anchorable) => LayoutElement != null && LayoutElement.CanFloat && LayoutElement.FindParent<LayoutFloatingWindow>() == null;

		/// <summary>Executes to float the content of this LayoutItem in a separate <see cref="LayoutFloatingWindowControl"/>.</summary>
		/// <param name="parameter"></param>
		private void ExecuteFloatCommand(object parameter) => LayoutElement.Root.Manager._ExecuteFloatCommand(LayoutElement);

		#endregion

		#region DockAsDocumentCommand

		/// <summary><see cref="DockAsDocumentCommand"/> dependency property.</summary>
		public static readonly DependencyProperty DockAsDocumentCommandProperty = DependencyProperty.Register(nameof(DockAsDocumentCommand), typeof(ICommand), typeof(LayoutItem),
				new FrameworkPropertyMetadata(null, OnDockAsDocumentCommandChanged, CoerceDockAsDocumentCommandValue));

		/// <summary>
		/// Gets or sets the <see cref="DockAsDocumentCommand"/> property. This dependency property 
		/// indicates the command to execute when user click the DockAsDocument button.
		/// </summary>
		/// <remarks>By default this command move the anchorable inside the last focused document pane.</remarks>
		public ICommand DockAsDocumentCommand
		{
			get => (ICommand)GetValue(DockAsDocumentCommandProperty);
			set => SetValue(DockAsDocumentCommandProperty, value);
		}

		/// <summary>Handles changes to the <see cref="DockAsDocumentCommand"/> property.</summary>
		private static void OnDockAsDocumentCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((LayoutItem)d).OnDockAsDocumentCommandChanged(e);

		/// <summary>Provides derived classes an opportunity to handle changes to the <see cref="DockAsDocumentCommand"/> property.</summary>
		protected virtual void OnDockAsDocumentCommandChanged(DependencyPropertyChangedEventArgs e)
		{
		}

		/// <summary>Coerces the <see cref="DockAsDocumentCommand"/> value.</summary>
		private static object CoerceDockAsDocumentCommandValue(DependencyObject d, object value) => value;

		protected virtual bool CanExecuteDockAsDocumentCommand() => LayoutElement != null && LayoutElement.FindParent<LayoutDocumentPane>() == null;

		private bool CanExecuteDockAsDocumentCommand(object parameter) => CanExecuteDockAsDocumentCommand();

		private void ExecuteDockAsDocumentCommand(object parameter) => LayoutElement.Root.Manager._ExecuteDockAsDocumentCommand(LayoutElement);

		#endregion

		#region CloseAllButThisCommand

		/// <summary><see cref="CloseAllButThisCommand"/> dependency property.</summary>
		public static readonly DependencyProperty CloseAllButThisCommandProperty = DependencyProperty.Register(nameof(CloseAllButThisCommand), typeof(ICommand), typeof(LayoutItem),
				new FrameworkPropertyMetadata(null, OnCloseAllButThisCommandChanged, CoerceCloseAllButThisCommandValue));

		/// <summary>
		/// Gets or sets the <see cref="CloseAllButThisCommand"/> property.  This dependency property 
		/// indicates the 'Close All But This' command.
		/// </summary>
		public ICommand CloseAllButThisCommand
		{
			get => (ICommand)GetValue(CloseAllButThisCommandProperty);
			set => SetValue(CloseAllButThisCommandProperty, value);
		}

		/// <summary>Handles changes to the <see cref="CloseAllButThisCommand"/> property.</summary>
		private static void OnCloseAllButThisCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((LayoutItem)d).OnCloseAllButThisCommandChanged(e);

		/// <summary>Provides derived classes an opportunity to handle changes to the <see cref="CloseAllButThisCommand"/> property.</summary>
		protected virtual void OnCloseAllButThisCommandChanged(DependencyPropertyChangedEventArgs e)
		{
		}

		/// <summary>Coerces the <see cref="CloseAllButThisCommand"/> value.</summary>
		private static object CoerceCloseAllButThisCommandValue(DependencyObject d, object value) => value;

		private bool CanExecuteCloseAllButThisCommand(object parameter)
		{
			var root = LayoutElement?.Root;
			if (root == null) return false;
			return LayoutElement.Root.Manager.Layout.Descendents().OfType<LayoutContent>().Any(d => d != LayoutElement && (d.Parent is LayoutDocumentPane || d.Parent is LayoutDocumentFloatingWindow));
		}

		private void ExecuteCloseAllButThisCommand(object parameter) => LayoutElement.Root.Manager._ExecuteCloseAllButThisCommand(LayoutElement);

		#endregion

		#region CloseAllCommand

		/// <summary><see cref="CloseAllCommand"/> dependency property.</summary>
		public static readonly DependencyProperty CloseAllCommandProperty = DependencyProperty.Register(nameof(CloseAllCommand), typeof(ICommand), typeof(LayoutItem),
				new FrameworkPropertyMetadata(null, OnCloseAllCommandChanged, CoerceCloseAllCommandValue));

		/// <summary>Gets or sets the <see cref="CloseAllCommand"/> property. This dependency property indicates the 'Close All' command.</summary>
		public ICommand CloseAllCommand
		{
			get => (ICommand)GetValue(CloseAllCommandProperty);
			set => SetValue(CloseAllCommandProperty, value);
		}

		/// <summary>Handles changes to the <see cref="CloseAllCommand"/> property.</summary>
		private static void OnCloseAllCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((LayoutItem)d).OnCloseAllCommandChanged(e);

		/// <summary>Provides derived classes an opportunity to handle changes to the <see cref="CloseAllCommand"/> property.</summary>
		protected virtual void OnCloseAllCommandChanged(DependencyPropertyChangedEventArgs e)
		{
		}

		/// <summary>Coerces the <see cref="CloseAllCommand"/> value.</summary>
		private static object CoerceCloseAllCommandValue(DependencyObject d, object value) => value;

		private bool CanExecuteCloseAllCommand(object parameter)
		{
			var root = LayoutElement?.Root;
			if (root == null) return false;
			return LayoutElement.Root.Manager.Layout.Descendents().OfType<LayoutContent>().Any(d => d.Parent is LayoutDocumentPane || d.Parent is LayoutDocumentFloatingWindow);
		}

		private void ExecuteCloseAllCommand(object parameter) => LayoutElement.Root.Manager._ExecuteCloseAllCommand(LayoutElement);

		#endregion

		#region ActivateCommand

		/// <summary><see cref="ActivateCommand"/> dependency property.</summary>
		public static readonly DependencyProperty ActivateCommandProperty = DependencyProperty.Register(nameof(ActivateCommand), typeof(ICommand), typeof(LayoutItem),
				new FrameworkPropertyMetadata(null, OnActivateCommandChanged, CoerceActivateCommandValue));

		/// <summary>
		/// Gets or sets the <see cref="ActivateCommand"/> property. This dependency property 
		/// indicates the command to execute when user wants to activate a content (either a Document or an Anchorable).
		/// </summary>
		public ICommand ActivateCommand
		{
			get => (ICommand)GetValue(ActivateCommandProperty);
			set => SetValue(ActivateCommandProperty, value);
		}

		/// <summary>Handles changes to the <see cref="ActivateCommand"/> property.</summary>
		private static void OnActivateCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((LayoutItem)d).OnActivateCommandChanged(e);

		/// <summary>Provides derived classes an opportunity to handle changes to the <see cref="ActivateCommand"/> property.</summary>
		protected virtual void OnActivateCommandChanged(DependencyPropertyChangedEventArgs e)
		{
		}

		/// <summary>Coerces the <see cref="ActivateCommand"/> value.</summary>
		private static object CoerceActivateCommandValue(DependencyObject d, object value) => value;

		private bool CanExecuteActivateCommand(object parameter) => LayoutElement != null;

		private void ExecuteActivateCommand(object parameter) => LayoutElement.Root.Manager._ExecuteContentActivateCommand(LayoutElement);

		#endregion

		#region NewVerticalTabGroupCommand

		/// <summary><see cref="NewVerticalTabGroupCommand"/> dependency property.</summary>
		public static readonly DependencyProperty NewVerticalTabGroupCommandProperty = DependencyProperty.Register(nameof(NewVerticalTabGroupCommand), typeof(ICommand), typeof(LayoutItem),
				new FrameworkPropertyMetadata(null, OnNewVerticalTabGroupCommandChanged));

		/// <summary>
		/// Gets or sets the <see cref="NewVerticalTabGroupCommand"/> property. This dependency property 
		/// indicates the new vertical tab group command.
		/// </summary>
		public ICommand NewVerticalTabGroupCommand
		{
			get => (ICommand)GetValue(NewVerticalTabGroupCommandProperty);
			set => SetValue(NewVerticalTabGroupCommandProperty, value);
		}

		/// <summary>Handles changes to the <see cref="NewVerticalTabGroupCommand"/> property.</summary>
		private static void OnNewVerticalTabGroupCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((LayoutItem)d).OnNewVerticalTabGroupCommandChanged(e);

		/// <summary>Provides derived classes an opportunity to handle changes to the <see cref="NewVerticalTabGroupCommand"/> property.</summary>
		protected virtual void OnNewVerticalTabGroupCommandChanged(DependencyPropertyChangedEventArgs e)
		{
		}

		private bool CanExecuteNewVerticalTabGroupCommand(object parameter)
		{
			if (LayoutElement == null) return false;
			if (LayoutElement is LayoutDocument layoutDocument && !layoutDocument.CanMove) return false;
			var parentDocumentGroup = LayoutElement.FindParent<LayoutDocumentPaneGroup>();
			return (parentDocumentGroup == null ||
					  parentDocumentGroup.ChildrenCount == 1 ||
					  parentDocumentGroup.Root.Manager.AllowMixedOrientation ||
					  parentDocumentGroup.Orientation == Orientation.Horizontal) &&
					 LayoutElement.Parent is LayoutDocumentPane parentDocumentPane &&
					 parentDocumentPane.ChildrenCount > 1;
		}

		private void ExecuteNewVerticalTabGroupCommand(object parameter)
		{
			var layoutElement = LayoutElement;
			var parentDocumentGroup = layoutElement.FindParent<LayoutDocumentPaneGroup>();
			var parentDocumentPane = layoutElement.Parent as LayoutDocumentPane;

			if (parentDocumentGroup == null)
			{
				var grandParent = parentDocumentPane.Parent;
				parentDocumentGroup = new LayoutDocumentPaneGroup { Orientation = Orientation.Horizontal };
				grandParent.ReplaceChild(parentDocumentPane, parentDocumentGroup);
				parentDocumentGroup.Children.Add(parentDocumentPane);
			}
			parentDocumentGroup.Orientation = Orientation.Horizontal;
			var indexOfParentPane = parentDocumentGroup.IndexOfChild(parentDocumentPane);
			parentDocumentGroup.InsertChildAt(indexOfParentPane + 1, new LayoutDocumentPane(layoutElement));
			layoutElement.IsActive = true;
			layoutElement.Root.CollectGarbage();
		}
		#endregion

		#region NewHorizontalTabGroupCommand

		/// <summary><see cref="NewHorizontalTabGroupCommand"/> dependency property.</summary>
		public static readonly DependencyProperty NewHorizontalTabGroupCommandProperty = DependencyProperty.Register(nameof(NewHorizontalTabGroupCommand), typeof(ICommand), typeof(LayoutItem),
				new FrameworkPropertyMetadata(null, OnNewHorizontalTabGroupCommandChanged));

		/// <summary>
		/// Gets or sets the <see cref="NewHorizontalTabGroupCommand"/> property. This dependency property 
		/// indicates the new horizontal tab group command.
		/// </summary>
		public ICommand NewHorizontalTabGroupCommand
		{
			get => (ICommand)GetValue(NewHorizontalTabGroupCommandProperty);
			set => SetValue(NewHorizontalTabGroupCommandProperty, value);
		}

		/// <summary>Handles changes to the <see cref="NewHorizontalTabGroupCommand"/> property.</summary>
		private static void OnNewHorizontalTabGroupCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((LayoutItem)d).OnNewHorizontalTabGroupCommandChanged(e);

		/// <summary>Provides derived classes an opportunity to handle changes to the <see cref="NewHorizontalTabGroupCommand"/> property.</summary>
		protected virtual void OnNewHorizontalTabGroupCommandChanged(DependencyPropertyChangedEventArgs e)
		{
		}

		private bool CanExecuteNewHorizontalTabGroupCommand(object parameter)
		{
			if (LayoutElement == null) return false;
			if (LayoutElement is LayoutDocument layoutDocument && !layoutDocument.CanMove) return false;
			var parentDocumentGroup = LayoutElement.FindParent<LayoutDocumentPaneGroup>();
			return (parentDocumentGroup == null ||
					  parentDocumentGroup.ChildrenCount == 1 ||
					  parentDocumentGroup.Root.Manager.AllowMixedOrientation ||
					  parentDocumentGroup.Orientation == Orientation.Vertical) &&
					 LayoutElement.Parent is LayoutDocumentPane parentDocumentPane &&
					 parentDocumentPane.ChildrenCount > 1;
		}

		private void ExecuteNewHorizontalTabGroupCommand(object parameter)
		{
			var layoutElement = LayoutElement;
			var parentDocumentGroup = layoutElement.FindParent<LayoutDocumentPaneGroup>();
			var parentDocumentPane = layoutElement.Parent as LayoutDocumentPane;

			if (parentDocumentGroup == null)
			{
				var grandParent = parentDocumentPane.Parent;
				parentDocumentGroup = new LayoutDocumentPaneGroup { Orientation = Orientation.Vertical };
				grandParent.ReplaceChild(parentDocumentPane, parentDocumentGroup);
				parentDocumentGroup.Children.Add(parentDocumentPane);
			}
			parentDocumentGroup.Orientation = Orientation.Vertical;
			var indexOfParentPane = parentDocumentGroup.IndexOfChild(parentDocumentPane);
			parentDocumentGroup.InsertChildAt(indexOfParentPane + 1, new LayoutDocumentPane(layoutElement));
			layoutElement.IsActive = true;
			layoutElement.Root.CollectGarbage();
		}
		#endregion

		#region MoveToNextTabGroupCommand

		/// <summary><see cref="MoveToNextTabGroupCommand"/> dependency property.</summary>
		public static readonly DependencyProperty MoveToNextTabGroupCommandProperty = DependencyProperty.Register(nameof(MoveToNextTabGroupCommand), typeof(ICommand), typeof(LayoutItem),
				new FrameworkPropertyMetadata(null, OnMoveToNextTabGroupCommandChanged));

		/// <summary>
		/// Gets or sets the <see cref="MoveToNextTabGroupCommand"/> property. This dependency property 
		/// indicates move to next tab group command.
		/// </summary>
		public ICommand MoveToNextTabGroupCommand
		{
			get => (ICommand)GetValue(MoveToNextTabGroupCommandProperty);
			set => SetValue(MoveToNextTabGroupCommandProperty, value);
		}

		/// <summary>Handles changes to the <see cref="MoveToNextTabGroupCommand"/> property.</summary>
		private static void OnMoveToNextTabGroupCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((LayoutItem)d).OnMoveToNextTabGroupCommandChanged(e);

		/// <summary>Provides derived classes an opportunity to handle changes to the <see cref="MoveToNextTabGroupCommand"/> property.</summary>
		protected virtual void OnMoveToNextTabGroupCommandChanged(DependencyPropertyChangedEventArgs e)
		{
		}

		private bool CanExecuteMoveToNextTabGroupCommand(object parameter)
		{
			if (LayoutElement == null) return false;
			var parentDocumentGroup = LayoutElement.FindParent<LayoutDocumentPaneGroup>();
			return parentDocumentGroup != null &&
					 LayoutElement.Parent is LayoutDocumentPane parentDocumentPane &&
					 parentDocumentGroup.ChildrenCount > 1 &&
					 parentDocumentGroup.IndexOfChild(parentDocumentPane) < parentDocumentGroup.ChildrenCount - 1 &&
					 parentDocumentGroup.Children[parentDocumentGroup.IndexOfChild(parentDocumentPane) + 1] is LayoutDocumentPane;
		}

		private void ExecuteMoveToNextTabGroupCommand(object parameter)
		{
			var layoutElement = LayoutElement;
			var parentDocumentGroup = layoutElement.FindParent<LayoutDocumentPaneGroup>();
			var parentDocumentPane = layoutElement.Parent as LayoutDocumentPane;
			var indexOfParentPane = parentDocumentGroup.IndexOfChild(parentDocumentPane);
			var nextDocumentPane = parentDocumentGroup.Children[indexOfParentPane + 1] as LayoutDocumentPane;
			nextDocumentPane.InsertChildAt(0, layoutElement);
			layoutElement.IsActive = true;
			layoutElement.Root.CollectGarbage();
		}

		#endregion

		#region MoveToPreviousTabGroupCommand

		/// <summary><see cref="MoveToPreviousTabGroupCommand"/> dependency property.</summary>
		public static readonly DependencyProperty MoveToPreviousTabGroupCommandProperty = DependencyProperty.Register(nameof(MoveToPreviousTabGroupCommand), typeof(ICommand), typeof(LayoutItem),
				new FrameworkPropertyMetadata(null, OnMoveToPreviousTabGroupCommandChanged));

		/// <summary>
		/// Gets or sets the <see cref="MoveToPreviousTabGroupCommand"/> property. This dependency property 
		/// indicates move to previous tab group command.
		/// </summary>
		public ICommand MoveToPreviousTabGroupCommand
		{
			get => (ICommand)GetValue(MoveToPreviousTabGroupCommandProperty);
			set => SetValue(MoveToPreviousTabGroupCommandProperty, value);
		}

		/// <summary>Handles changes to the <see cref="MoveToPreviousTabGroupCommand"/> property.</summary>
		private static void OnMoveToPreviousTabGroupCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((LayoutItem)d).OnMoveToPreviousTabGroupCommandChanged(e);

		/// <summary>Provides derived classes an opportunity to handle changes to the <see cref="MoveToPreviousTabGroupCommand"/> property.</summary>
		protected virtual void OnMoveToPreviousTabGroupCommandChanged(DependencyPropertyChangedEventArgs e)
		{
		}

		private bool CanExecuteMoveToPreviousTabGroupCommand(object parameter)
		{
			if (LayoutElement == null) return false;
			var parentDocumentGroup = LayoutElement.FindParent<LayoutDocumentPaneGroup>();
			return parentDocumentGroup != null &&
					 LayoutElement.Parent is LayoutDocumentPane parentDocumentPane &&
					 parentDocumentGroup.ChildrenCount > 1 &&
					 parentDocumentGroup.IndexOfChild(parentDocumentPane) > 0 &&
					 parentDocumentGroup.Children[parentDocumentGroup.IndexOfChild(parentDocumentPane) - 1] is LayoutDocumentPane;
		}

		private void ExecuteMoveToPreviousTabGroupCommand(object parameter)
		{
			var layoutElement = LayoutElement;
			var parentDocumentGroup = layoutElement.FindParent<LayoutDocumentPaneGroup>();
			var parentDocumentPane = layoutElement.Parent as LayoutDocumentPane;
			var indexOfParentPane = parentDocumentGroup.IndexOfChild(parentDocumentPane);
			var nextDocumentPane = parentDocumentGroup.Children[indexOfParentPane - 1] as LayoutDocumentPane;
			nextDocumentPane.InsertChildAt(0, layoutElement);
			layoutElement.IsActive = true;
			layoutElement.Root.CollectGarbage();
		}
		#endregion

		#endregion

		#region Internal Methods

		protected virtual void InitDefaultCommands()
		{
			_defaultCloseCommand = new RelayCommand(ExecuteCloseCommand, CanExecuteCloseCommand);
			_defaultFloatCommand = new RelayCommand(ExecuteFloatCommand, CanExecuteFloatCommand);
			_defaultDockAsDocumentCommand = new RelayCommand(ExecuteDockAsDocumentCommand, CanExecuteDockAsDocumentCommand);
			_defaultCloseAllButThisCommand = new RelayCommand(ExecuteCloseAllButThisCommand, CanExecuteCloseAllButThisCommand);
			_defaultCloseAllCommand = new RelayCommand(ExecuteCloseAllCommand, CanExecuteCloseAllCommand);
			_defaultActivateCommand = new RelayCommand(ExecuteActivateCommand, CanExecuteActivateCommand);
			_defaultNewVerticalTabGroupCommand = new RelayCommand(ExecuteNewVerticalTabGroupCommand, CanExecuteNewVerticalTabGroupCommand);
			_defaultNewHorizontalTabGroupCommand = new RelayCommand(ExecuteNewHorizontalTabGroupCommand, CanExecuteNewHorizontalTabGroupCommand);
			_defaultMoveToNextTabGroupCommand = new RelayCommand(ExecuteMoveToNextTabGroupCommand, CanExecuteMoveToNextTabGroupCommand);
			_defaultMoveToPreviousTabGroupCommand = new RelayCommand(ExecuteMoveToPreviousTabGroupCommand, CanExecuteMoveToPreviousTabGroupCommand);
		}

		protected virtual void ClearDefaultBindings()
		{
			if (CloseCommand == _defaultCloseCommand)
				BindingOperations.ClearBinding(this, CloseCommandProperty);
			if (FloatCommand == _defaultFloatCommand)
				BindingOperations.ClearBinding(this, FloatCommandProperty);
			if (DockAsDocumentCommand == _defaultDockAsDocumentCommand)
				BindingOperations.ClearBinding(this, DockAsDocumentCommandProperty);
			if (CloseAllButThisCommand == _defaultCloseAllButThisCommand)
				BindingOperations.ClearBinding(this, CloseAllButThisCommandProperty);
			if (CloseAllCommand == _defaultCloseAllCommand)
				BindingOperations.ClearBinding(this, CloseAllCommandProperty);
			if (ActivateCommand == _defaultActivateCommand)
				BindingOperations.ClearBinding(this, ActivateCommandProperty);
			if (NewVerticalTabGroupCommand == _defaultNewVerticalTabGroupCommand)
				BindingOperations.ClearBinding(this, NewVerticalTabGroupCommandProperty);
			if (NewHorizontalTabGroupCommand == _defaultNewHorizontalTabGroupCommand)
				BindingOperations.ClearBinding(this, NewHorizontalTabGroupCommandProperty);
			if (MoveToNextTabGroupCommand == _defaultMoveToNextTabGroupCommand)
				BindingOperations.ClearBinding(this, MoveToNextTabGroupCommandProperty);
			if (MoveToPreviousTabGroupCommand == _defaultMoveToPreviousTabGroupCommand)
				BindingOperations.ClearBinding(this, MoveToPreviousTabGroupCommandProperty);
		}

		protected virtual void SetDefaultBindings()
		{
			if (CloseCommand == null)
				CloseCommand = _defaultCloseCommand;
			if (FloatCommand == null)
				FloatCommand = _defaultFloatCommand;
			if (DockAsDocumentCommand == null)
				DockAsDocumentCommand = _defaultDockAsDocumentCommand;
			if (CloseAllButThisCommand == null)
				CloseAllButThisCommand = _defaultCloseAllButThisCommand;
			if (CloseAllCommand == null)
				CloseAllCommand = _defaultCloseAllCommand;
			if (ActivateCommand == null)
				ActivateCommand = _defaultActivateCommand;
			if (NewVerticalTabGroupCommand == null)
				NewVerticalTabGroupCommand = _defaultNewVerticalTabGroupCommand;
			if (NewHorizontalTabGroupCommand == null)
				NewHorizontalTabGroupCommand = _defaultNewHorizontalTabGroupCommand;
			if (MoveToNextTabGroupCommand == null)
				MoveToNextTabGroupCommand = _defaultMoveToNextTabGroupCommand;
			if (MoveToPreviousTabGroupCommand == null)
				MoveToPreviousTabGroupCommand = _defaultMoveToPreviousTabGroupCommand;


			IsSelected = LayoutElement.IsSelected;
			IsActive = LayoutElement.IsActive;
			CanClose = LayoutElement.CanClose;
		}

		protected virtual void OnVisibilityChanged()
		{
			if (LayoutElement != null && Visibility == Visibility.Collapsed)
				LayoutElement.Close();
		}

		internal virtual void Attach(LayoutContent model)
		{
			LayoutElement = model;
			Model = model.Content;
			InitDefaultCommands();
			LayoutElement.IsSelectedChanged += LayoutElement_IsSelectedChanged;
			LayoutElement.IsActiveChanged += LayoutElement_IsActiveChanged;
			DataContext = this;
		}

		internal virtual void Detach()
		{
			LayoutElement.IsSelectedChanged -= LayoutElement_IsSelectedChanged;
			LayoutElement.IsActiveChanged -= LayoutElement_IsActiveChanged;
			LayoutElement = null;
			Model = null;
		}

		internal void _ClearDefaultBindings() => ClearDefaultBindings();

		internal void _SetDefaultBindings() => SetDefaultBindings();

		internal bool IsViewExists() => _view != null;

		#endregion

		#region Private Methods

		private void LayoutElement_IsActiveChanged(object sender, EventArgs e)
		{
			if (!_isActiveReentrantFlag.CanEnter) return;
			using (_isActiveReentrantFlag.Enter())
			{
				var bnd = BindingOperations.GetBinding(this, IsActiveProperty);
				IsActive = LayoutElement.IsActive;
				var bnd2 = BindingOperations.GetBinding(this, IsActiveProperty);
			}
		}

		private void LayoutElement_IsSelectedChanged(object sender, EventArgs e)
		{
			if (!_isSelectedReentrantFlag.CanEnter) return;
			using (_isSelectedReentrantFlag.Enter())
			{
				IsSelected = LayoutElement.IsSelected;
			}
		}

		private static void OnToolTipChanged(DependencyObject s, DependencyPropertyChangedEventArgs e) => ((LayoutItem)s).OnToolTipChanged();

		private void OnToolTipChanged()
		{
			if (LayoutElement != null) LayoutElement.ToolTip = ToolTip;
		}

		private static void OnVisibilityChanged(DependencyObject s, DependencyPropertyChangedEventArgs e) => ((LayoutItem)s).OnVisibilityChanged();

		#endregion
	}
}
