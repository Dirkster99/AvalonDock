/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using AvalonDock.Controls;
using AvalonDock.Layout;
using AvalonDock.Themes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Threading;

namespace AvalonDock
{
	/// <inheritdoc cref="Control"/>
	/// <inheritdoc cref="IOverlayWindowHost"/>
	/// <summary>
	/// The <see cref="DockingManager"/> is the custom control at the root of the visual tree.
	/// This control is the core control of AvalonDock.
	/// It contains core dependency properties, events, and methods to customize and
	/// manage many aspects of the docking framework.
	/// </summary>
	/// <seealso cref="Control"/>
	/// <seealso cref="IOverlayWindowHost"/>
	[ContentProperty(nameof(Layout))]
	[TemplatePart(Name = "PART_AutoHideArea")]
	public class DockingManager : Control, IOverlayWindowHost//, ILogicalChildrenContainer
	{
		#region fields
		// ShortCut to current AvalonDock theme if OnThemeChanged() is invoked with DictionaryTheme instance
		// in e.OldValue and e.NewValue of the passed event
		private ResourceDictionary currentThemeResourceDictionary;

		private AutoHideWindowManager _autoHideWindowManager;
		private FrameworkElement _autohideArea;
		private readonly List<LayoutFloatingWindowControl> _fwList = new List<LayoutFloatingWindowControl>();
		private readonly List<LayoutFloatingWindowControl> _fwHiddenList = new List<LayoutFloatingWindowControl>();
		private OverlayWindow _overlayWindow = null;
		private List<IDropArea> _areas = null;
		private bool _insideInternalSetActiveContent = false;

		// Collection of LayoutDocumentItems & LayoutAnchorableItems attached to their corresponding
		// LayoutDocument & LayoutAnchorable
		private readonly List<LayoutItem> _layoutItems = new List<LayoutItem>();

		private bool _suspendLayoutItemCreation = false;
		private DispatcherOperation _collectLayoutItemsOperations = null;
		private NavigatorWindow _navigatorWindow = null;

		internal bool SuspendDocumentsSourceBinding = false;
		internal bool SuspendAnchorablesSourceBinding = false;

		#endregion fields

		#region Constructors

		/// <summary>
		/// Static class constructor to support WPF property control registration.
		/// </summary>
		static DockingManager()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(DockingManager), new FrameworkPropertyMetadata(typeof(DockingManager)));
			FocusableProperty.OverrideMetadata(typeof(DockingManager), new FrameworkPropertyMetadata(false));
			HwndSource.DefaultAcquireHwndFocusInMenuMode = false;
		}

		/// <summary>
		/// Class constructor.
		/// </summary>
		public DockingManager()
		{
			IsVirtualizingDocument = true;
			IsVirtualizingAnchorable = true;

#if !VS2008
			Layout = new LayoutRoot { RootPanel = new LayoutPanel(new LayoutDocumentPaneGroup(new LayoutDocumentPane())) };
#else
		  this.SetCurrentValue( DockingManager.LayoutProperty, new LayoutRoot() { RootPanel = new LayoutPanel(new LayoutDocumentPaneGroup(new LayoutDocumentPane())) } );
#endif
			Loaded += DockingManager_Loaded;
			Unloaded += DockingManager_Unloaded;
		}

		#endregion Constructors

		#region Events

		/// <summary>Event fired when <see cref="DockingManager.Layout"/> property changes.</summary>
		/// <seealso cref="Layout"/>
		public event EventHandler LayoutChanged;

		/// <summary>Event fired when <see cref="DockingManager.Layout"/> property is about to be changed.</summary>
		/// <seealso cref="Layout"/>
		public event EventHandler LayoutChanging;

		/// <summary>Event fired when a document is about to be closed.</summary>
		/// <remarks>Subscribers have the opportunity to cancel the operation.</remarks>
		public event EventHandler<DocumentClosingEventArgs> DocumentClosing;

		/// <summary>Event fired after a document is closed.</summary>
		public event EventHandler<DocumentClosedEventArgs> DocumentClosed;

		/// <summary>Event is raised when <see cref="ActiveContent"/> changes.</summary>
		/// <seealso cref="ActiveContent"/>
		public event EventHandler ActiveContentChanged;

		#endregion Events

		#region Public Properties

		#region Layout

		/// <summary><see cref="Layout"/> dependency property.</summary>
		public static readonly DependencyProperty LayoutProperty = DependencyProperty.Register(nameof(Layout), typeof(LayoutRoot), typeof(DockingManager),
				new FrameworkPropertyMetadata(null, OnLayoutChanged, CoerceLayoutValue));

		/// <summary>Gets/sets the layout root of the layout tree managed in this framework.</summary>
		[Bindable(true), Description("Gets/sets the layout root of the layout tree managed in this framework."), Category("Layout")]
		public LayoutRoot Layout
		{
			get => (LayoutRoot)GetValue(LayoutProperty);
			set => SetValue(LayoutProperty, value);
		}

		/// <summary>Coerces the <see cref="Layout"/> value.</summary>
		private static object CoerceLayoutValue(DependencyObject d, object value)
		{
			if (value == null) return new LayoutRoot { RootPanel = new LayoutPanel(new LayoutDocumentPaneGroup(new LayoutDocumentPane())) };
			((DockingManager)d).OnLayoutChanging(value as LayoutRoot);
			return value;
		}

		/// <summary>Handles changes to the <see cref="Layout"/> property.</summary>
		private static void OnLayoutChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((DockingManager)d).OnLayoutChanged(e.OldValue as LayoutRoot, e.NewValue as LayoutRoot);

		/// <summary>Provides derived classes an opportunity to handle changes to the <see cref="Layout"/> property.</summary>
		protected virtual void OnLayoutChanged(LayoutRoot oldLayout, LayoutRoot newLayout)
		{
			if (oldLayout != null)
			{
				oldLayout.PropertyChanged -= OnLayoutRootPropertyChanged;
				oldLayout.Updated -= OnLayoutRootUpdated;
			}

			foreach (var fwc in _fwList.ToArray())
			{
				fwc.KeepContentVisibleOnClose = true;
				fwc.InternalClose();
			}

			_fwList.Clear();

			foreach (var fwc in _fwHiddenList.ToArray())
			{
				fwc.InternalClose();
			}

			_fwHiddenList.Clear();
			DetachDocumentsSource(oldLayout, DocumentsSource);
			DetachAnchorablesSource(oldLayout, AnchorablesSource);

			if (oldLayout != null && oldLayout.Manager == this)
				oldLayout.Manager = null;

			ClearLogicalChildrenList();
			DetachLayoutItems();

			Layout.Manager = this;

			AttachLayoutItems();
			AttachDocumentsSource(newLayout, DocumentsSource);
			AttachAnchorablesSource(newLayout, AnchorablesSource);

			if (IsLoaded)
			{
				LayoutRootPanel = CreateUIElementForModel(Layout.RootPanel) as LayoutPanelControl;
				LeftSidePanel = CreateUIElementForModel(Layout.LeftSide) as LayoutAnchorSideControl;
				TopSidePanel = CreateUIElementForModel(Layout.TopSide) as LayoutAnchorSideControl;
				RightSidePanel = CreateUIElementForModel(Layout.RightSide) as LayoutAnchorSideControl;
				BottomSidePanel = CreateUIElementForModel(Layout.BottomSide) as LayoutAnchorSideControl;

				foreach (var fw in Layout.FloatingWindows.ToArray())
					if (fw.IsValid)
						_fwList.Add(CreateUIElementForModel(fw) as LayoutFloatingWindowControl);

				foreach (var fw in _fwList.ToArray())
				{
					if (fw.Model is LayoutAnchorableFloatingWindow window && window.RootPanel.IsMaximized)
					{
						fw.WindowState = WindowState.Normal;
						fw.Show();
						fw.WindowState = WindowState.Maximized;
					}
					else
					{
						if (fw.Content != null || (fw.Model as LayoutAnchorableFloatingWindow)?.IsVisible == true)
							fw.Show();
						else
							fw.Hide();
					}

					//fw.Owner = Window.GetWindow(this);
					//fw.SetParentToMainWindowOf(this);
				}

				// In order to prevent resource leaks, unsubscribe from SizeChanged event for case when user call loading of Layout Settigns.
				SizeChanged -= OnSizeChanged;
				SizeChanged += OnSizeChanged;
			}

			if (newLayout != null)
			{
				newLayout.PropertyChanged += OnLayoutRootPropertyChanged;
				newLayout.Updated += OnLayoutRootUpdated;
			}

			LayoutChanged?.Invoke(this, EventArgs.Empty);
			//    Layout?.CollectGarbage();
			CommandManager.InvalidateRequerySuggested();
		}

		#endregion Layout

		#region LayoutUpdateStrategy

		/// <summary><see cref="LayoutUpdateStrategy"/> dependency property.</summary>
		public static readonly DependencyProperty LayoutUpdateStrategyProperty = DependencyProperty.Register(nameof(LayoutUpdateStrategy), typeof(ILayoutUpdateStrategy), typeof(DockingManager),
				new FrameworkPropertyMetadata((ILayoutUpdateStrategy)null));

		/// <summary>
		/// Gets/sets the layout strategy class that can be to called by the framework when it needs to position a <see cref="LayoutAnchorable"/> inside an existing layout.
		/// </summary>
		/// <remarks>Sometimes it's impossible to automatically insert an anchorable in the layout without specifing the target parent pane.
		/// Set this property to an object that will be asked to insert the anchorable to the desidered position.</remarks>
		[Bindable(true), Description("Gets/sets the layout strategy class that can be to called by the framework when it needs to position a LayoutAnchorable inside an existing layout."), Category("Layout")]
		public ILayoutUpdateStrategy LayoutUpdateStrategy
		{
			get => (ILayoutUpdateStrategy)GetValue(LayoutUpdateStrategyProperty);
			set => SetValue(LayoutUpdateStrategyProperty, value);
		}

		#endregion LayoutUpdateStrategy

		#region DocumentPaneTemplate

		/// <summary><see cref="DocumentPaneTemplate"/> dependency property.</summary>
		public static readonly DependencyProperty DocumentPaneTemplateProperty = DependencyProperty.Register(nameof(DocumentPaneTemplate), typeof(ControlTemplate), typeof(DockingManager),
				new FrameworkPropertyMetadata(null, OnDocumentPaneTemplateChanged));

		/// <summary>Gets/sets the <see cref="ControlTemplate"/> that can be used to render the <see cref="LayoutDocumentPaneControl"/>.</summary>
		[Bindable(true), Description("Gets/sets the ControlTemplate´that can be used to render the LayoutDocumentPaneControl."), Category("Document")]
		public ControlTemplate DocumentPaneTemplate
		{
			get => (ControlTemplate)GetValue(DocumentPaneTemplateProperty);
			set => SetValue(DocumentPaneTemplateProperty, value);
		}

		/// <summary>Handles changes to the <see cref="DocumentPaneTemplate"/> property.</summary>
		private static void OnDocumentPaneTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((DockingManager)d).OnDocumentPaneTemplateChanged(e);
		}

		/// <summary>Provides derived classes an opportunity to handle changes to the <see cref="DocumentPaneTemplate"/> property.</summary>
		protected virtual void OnDocumentPaneTemplateChanged(DependencyPropertyChangedEventArgs e)
		{
		}

		#endregion DocumentPaneTemplate

		#region AnchorablePaneTemplate

		/// <summary>
		/// <see cref="AnchorablePaneTemplate"/> dependency property
		/// </summary>
		public static readonly DependencyProperty AnchorablePaneTemplateProperty = DependencyProperty.Register(nameof(AnchorablePaneTemplate), typeof(ControlTemplate), typeof(DockingManager),
				new FrameworkPropertyMetadata(null, OnAnchorablePaneTemplateChanged));

		/// <summary>Gets/sets the <see cref="ControlTemplate"/> used to render <see cref="LayoutAnchorablePaneControl"/>.</summary>
		[Bindable(true), Description("Gets/sets the ControlTemplate used to render LayoutAnchorablePaneControl"), Category("Anchorable")]
		public ControlTemplate AnchorablePaneTemplate
		{
			get => (ControlTemplate)GetValue(AnchorablePaneTemplateProperty);
			set => SetValue(AnchorablePaneTemplateProperty, value);
		}

		/// <summary>Handles changes to the <see cref="AnchorablePaneTemplate"/> property.</summary>
		private static void OnAnchorablePaneTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((DockingManager)d).OnAnchorablePaneTemplateChanged(e);

		/// <summary>Provides derived classes an opportunity to handle changes to the <see cref="AnchorablePaneTemplate"/> property.</summary>
		protected virtual void OnAnchorablePaneTemplateChanged(DependencyPropertyChangedEventArgs e)
		{
		}

		#endregion AnchorablePaneTemplate

		#region AnchorSideTemplate

		/// <summary>The <see cref="AnchorSideTemplate"/> dependency property.</summary>
		public static readonly DependencyProperty AnchorSideTemplateProperty = DependencyProperty.Register(nameof(AnchorSideTemplate), typeof(ControlTemplate), typeof(DockingManager),
				new FrameworkPropertyMetadata((ControlTemplate)null));

		/// <summary>Gets/sets the <see cref="ControlTemplate"/> used to render <see cref="LayoutAnchorSideControl"/>.</summary>
		[Bindable(true), Description("Gets/sets the ControlTemplate used to render LayoutAnchorSideControl."), Category("Anchor")]
		public ControlTemplate AnchorSideTemplate
		{
			get => (ControlTemplate)GetValue(AnchorSideTemplateProperty);
			set => SetValue(AnchorSideTemplateProperty, value);
		}

		#endregion AnchorSideTemplate

		#region AnchorGroupTemplate

		/// <summary><see cref="AnchorGroupTemplate"/> dependency property.</summary>
		public static readonly DependencyProperty AnchorGroupTemplateProperty = DependencyProperty.Register(nameof(AnchorGroupTemplate), typeof(ControlTemplate), typeof(DockingManager),
				new FrameworkPropertyMetadata((ControlTemplate)null));

		/// <summary>Gets/sets the <see cref="ControlTemplate"/> used to render the <see cref="LayoutAnchorGroupControl"/>.</summary>
		[Bindable(true), Description("Gets/sets the ControlTemplate used to render LayoutAnchorGroupControl."), Category("Anchor")]
		public ControlTemplate AnchorGroupTemplate
		{
			get => (ControlTemplate)GetValue(AnchorGroupTemplateProperty);
			set => SetValue(AnchorGroupTemplateProperty, value);
		}

		#endregion AnchorGroupTemplate

		#region AnchorTemplate

		/// <summary><see cref="AnchorTemplate"/> dependency property.</summary>
		public static readonly DependencyProperty AnchorTemplateProperty = DependencyProperty.Register(nameof(AnchorTemplate), typeof(ControlTemplate), typeof(DockingManager),
				new FrameworkPropertyMetadata((ControlTemplate)null));

		/// <summary>Gets/sets the <see cref="ControlTemplate"/> used to render a <see cref="LayoutAnchorControl"/>.</summary>
		[Bindable(true), Description("Gets/sets the ControlTemplate used to render a LayoutAnchorControl."), Category("Anchor")]
		public ControlTemplate AnchorTemplate
		{
			get => (ControlTemplate)GetValue(AnchorTemplateProperty);
			set => SetValue(AnchorTemplateProperty, value);
		}

		#endregion AnchorTemplate

		#region DocumentPaneControlStyle

		/// <summary><see cref="DocumentPaneControlStyle"/> dependency property.</summary>
		public static readonly DependencyProperty DocumentPaneControlStyleProperty = DependencyProperty.Register(nameof(DocumentPaneControlStyle), typeof(Style), typeof(DockingManager),
				new FrameworkPropertyMetadata(null, OnDocumentPaneControlStyleChanged));

		/// <summary>Gets/sets the style of a <see cref="LayoutDocumentPaneControl"/>.</summary>
		[Bindable(true), Description("Gets/sets the style of a LayoutDocumentPaneControl."), Category("Document")]
		public Style DocumentPaneControlStyle
		{
			get => (Style)GetValue(DocumentPaneControlStyleProperty);
			set => SetValue(DocumentPaneControlStyleProperty, value);
		}

		/// <summary>Handles changes to the DocumentPaneControlStyle property.</summary>
		private static void OnDocumentPaneControlStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((DockingManager)d).OnDocumentPaneControlStyleChanged(e);

		/// <summary>Provides derived classes an opportunity to handle changes to the <see cref="DocumentPaneControlStyle"/> property.</summary>
		protected virtual void OnDocumentPaneControlStyleChanged(DependencyPropertyChangedEventArgs e)
		{
		}

		#endregion DocumentPaneControlStyle

		#region AnchorablePaneControlStyle

		/// <summary><see cref="AnchorablePaneControlStyle"/> dependency property.</summary>
		public static readonly DependencyProperty AnchorablePaneControlStyleProperty = DependencyProperty.Register(nameof(AnchorablePaneControlStyle), typeof(Style), typeof(DockingManager),
				new FrameworkPropertyMetadata(null, OnAnchorablePaneControlStyleChanged));

		/// <summary>Gets/sets the <see cref="Style"/> to apply to <see cref="LayoutAnchorablePaneControl"/>.</summary>
		[Bindable(true), Description("Gets/sets the Style to apply to LayoutAnchorablePaneControl."), Category("Anchorable")]
		public Style AnchorablePaneControlStyle
		{
			get => (Style)GetValue(AnchorablePaneControlStyleProperty);
			set => SetValue(AnchorablePaneControlStyleProperty, value);
		}

		/// <summary>Handles changes to the <see cref="AnchorablePaneControlStyle"/> property.</summary>
		private static void OnAnchorablePaneControlStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((DockingManager)d).OnAnchorablePaneControlStyleChanged(e);

		/// <summary>Provides derived classes an opportunity to handle changes to the <see cref="AnchorablePaneControlStyle"/> property.</summary>
		protected virtual void OnAnchorablePaneControlStyleChanged(DependencyPropertyChangedEventArgs e)
		{
		}

		#endregion AnchorablePaneControlStyle

		#region DocumentHeaderTemplate

		/// <summary><see cref="DocumentHeaderTemplate"/> dependency property.</summary>
		public static readonly DependencyProperty DocumentHeaderTemplateProperty = DependencyProperty.Register(nameof(DocumentHeaderTemplate), typeof(DataTemplate), typeof(DockingManager),
				new FrameworkPropertyMetadata((DataTemplate)null, OnDocumentHeaderTemplateChanged, CoerceDocumentHeaderTemplateValue));

		/// <summary>Gets/sets the <see cref="DataTemplate"/> to use for document headers.</summary>
		[Bindable(true), Description("Gets/sets the DataTemplate to use for document headers."), Category("Document")]
		public DataTemplate DocumentHeaderTemplate
		{
			get => (DataTemplate)GetValue(DocumentHeaderTemplateProperty);
			set => SetValue(DocumentHeaderTemplateProperty, value);
		}

		/// <summary>Handles changes to the <see cref="DocumentHeaderTemplate"/> property.</summary>
		private static void OnDocumentHeaderTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((DockingManager)d).OnDocumentHeaderTemplateChanged(e);

		/// <summary>Provides derived classes an opportunity to handle changes to the <see cref="DocumentHeaderTemplate"/> property.</summary>
		protected virtual void OnDocumentHeaderTemplateChanged(DependencyPropertyChangedEventArgs e)
		{
		}

		/// <summary>Coerces the <see cref="DocumentHeaderTemplate"/> value.</summary>
		private static object CoerceDocumentHeaderTemplateValue(DependencyObject d, object value)
		{
			if (value != null && d.GetValue(DocumentHeaderTemplateSelectorProperty) != null)
				return null;
			return value;
		}

		#endregion DocumentHeaderTemplate

		#region DocumentHeaderTemplateSelector

		/// <summary><see cref="DocumentHeaderTemplateSelector"/> dependency property.</summary>
		public static readonly DependencyProperty DocumentHeaderTemplateSelectorProperty = DependencyProperty.Register(nameof(DocumentHeaderTemplateSelector), typeof(DataTemplateSelector), typeof(DockingManager),
				new FrameworkPropertyMetadata(null, OnDocumentHeaderTemplateSelectorChanged, CoerceDocumentHeaderTemplateSelectorValue));

		/// <summary>Gets /sets the <see cref="DataTemplateSelector"/> that can be used for selecting a <see cref="DataTemplate"/> for a document header.</summary>
		[Bindable(true), Description("Gets /sets the DataTemplateSelector that can be used for selecting a DataTemplates for a document header."), Category("Document")]
		public DataTemplateSelector DocumentHeaderTemplateSelector
		{
			get => (DataTemplateSelector)GetValue(DocumentHeaderTemplateSelectorProperty);
			set => SetValue(DocumentHeaderTemplateSelectorProperty, value);
		}

		/// <summary>Handles changes to the <see cref="DocumentHeaderTemplateSelector"/> property.</summary>
		private static void OnDocumentHeaderTemplateSelectorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((DockingManager)d).OnDocumentHeaderTemplateSelectorChanged(e);

		/// <summary>Provides derived classes an opportunity to handle changes to the <see cref="DocumentHeaderTemplateSelector"/> property.</summary>
		protected virtual void OnDocumentHeaderTemplateSelectorChanged(DependencyPropertyChangedEventArgs e)
		{
			if (e.NewValue != null && DocumentHeaderTemplate != null)
				DocumentHeaderTemplate = null;
			if (DocumentPaneMenuItemHeaderTemplateSelector == null)
				DocumentPaneMenuItemHeaderTemplateSelector = DocumentHeaderTemplateSelector;
		}

		/// <summary>Coerces the <see cref="DocumentHeaderTemplateSelector"/> value.</summary>
		private static object CoerceDocumentHeaderTemplateSelectorValue(DependencyObject d, object value) => value;

		#endregion DocumentHeaderTemplateSelector

		#region DocumentTitleTemplate

		/// <summary><see cref="DocumentTitleTemplate"/> dependency property.</summary>
		public static readonly DependencyProperty DocumentTitleTemplateProperty = DependencyProperty.Register(nameof(DocumentTitleTemplate), typeof(DataTemplate), typeof(DockingManager),
				new FrameworkPropertyMetadata(null, OnDocumentTitleTemplateChanged, CoerceDocumentTitleTemplateValue));

		/// <summary>Gets/sets the <see cref="DataTemplate"/> to use for displaying the title of a document.</summary>
		[Bindable(true), Description("Gets/sets the DataTemplate to use for displaying the title of a document."), Category("Document")]
		public DataTemplate DocumentTitleTemplate
		{
			get => (DataTemplate)GetValue(DocumentTitleTemplateProperty);
			set => SetValue(DocumentTitleTemplateProperty, value);
		}

		/// <summary>Handles changes to the <see cref="DocumentTitleTemplate"/> property.</summary>
		private static void OnDocumentTitleTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((DockingManager)d).OnDocumentTitleTemplateChanged(e);

		/// <summary>Provides derived classes an opportunity to handle changes to the <see cref="DocumentTitleTemplate"/> property.</summary>
		protected virtual void OnDocumentTitleTemplateChanged(DependencyPropertyChangedEventArgs e)
		{
		}

		/// <summary>Coerces the <see cref="DocumentTitleTemplate"/> value.</summary>
		private static object CoerceDocumentTitleTemplateValue(DependencyObject d, object value)
		{
			if (value != null && d.GetValue(DocumentTitleTemplateSelectorProperty) != null)
				return null;
			return value;
		}

		#endregion DocumentTitleTemplate

		#region DocumentTitleTemplateSelector

		/// <summary><see cref="DocumentTitleTemplateSelector"/> dependency property.</summary>
		public static readonly DependencyProperty DocumentTitleTemplateSelectorProperty = DependencyProperty.Register(nameof(DocumentTitleTemplateSelector), typeof(DataTemplateSelector), typeof(DockingManager),
				new FrameworkPropertyMetadata(null, OnDocumentTitleTemplateSelectorChanged, CoerceDocumentTitleTemplateSelectorValue));

		/// <summary>Gets/sets the <see cref="DataTemplateSelector"/> to use for displaying the <see cref="DataTemplate"/> of a document's title.</summary>
		[Bindable(true), Description("Gets/sets the DataTemplateSelector to use for displaying the DataTemplate of a document's title."), Category("Document")]
		public DataTemplateSelector DocumentTitleTemplateSelector
		{
			get => (DataTemplateSelector)GetValue(DocumentTitleTemplateSelectorProperty);
			set => SetValue(DocumentTitleTemplateSelectorProperty, value);
		}

		/// <summary>Handles changes to the <see cref="DocumentTitleTemplateSelector"/> property.</summary>
		private static void OnDocumentTitleTemplateSelectorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((DockingManager)d).OnDocumentTitleTemplateSelectorChanged(e);

		/// <summary>Provides derived classes an opportunity to handle changes to the <see cref="DocumentTitleTemplateSelector"/> property.</summary>
		protected virtual void OnDocumentTitleTemplateSelectorChanged(DependencyPropertyChangedEventArgs e)
		{
			if (e.NewValue != null)
				DocumentTitleTemplate = null;
		}

		/// <summary>Coerces the <see cref="DocumentTitleTemplateSelector"/> value.</summary>
		private static object CoerceDocumentTitleTemplateSelectorValue(DependencyObject d, object value) => value;

		#endregion DocumentTitleTemplateSelector

		#region AnchorableTitleTemplate

		/// <summary><see cref="AnchorableTitleTemplate"/> dependency property.</summary>
		public static readonly DependencyProperty AnchorableTitleTemplateProperty = DependencyProperty.Register(nameof(AnchorableTitleTemplate), typeof(DataTemplate), typeof(DockingManager),
				new FrameworkPropertyMetadata((DataTemplate)null, OnAnchorableTitleTemplateChanged, CoerceAnchorableTitleTemplateValue));

		/// <summary>Gets/sets the <see cref="DataTemplate"/> to use for the title of an anchorable.</summary>
		[Bindable(true), Description("Gets/sets the DataTemplate to use for the title of an anchorable."), Category("Anchorable")]
		public DataTemplate AnchorableTitleTemplate
		{
			get => (DataTemplate)GetValue(AnchorableTitleTemplateProperty);
			set => SetValue(AnchorableTitleTemplateProperty, value);
		}

		/// <summary>Handles changes to the <see cref="AnchorableTitleTemplate"/> property.</summary>
		private static void OnAnchorableTitleTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((DockingManager)d).OnAnchorableTitleTemplateChanged(e);

		/// <summary>Provides derived classes an opportunity to handle changes to the <see cref="AnchorableTitleTemplate"/> property.</summary>
		protected virtual void OnAnchorableTitleTemplateChanged(DependencyPropertyChangedEventArgs e)
		{
		}

		/// <summary>Coerces the <see cref="AnchorableTitleTemplate"/> value.</summary>
		private static object CoerceAnchorableTitleTemplateValue(DependencyObject d, object value)
		{
			if (value != null && d.GetValue(AnchorableTitleTemplateSelectorProperty) != null)
				return null;
			return value;
		}

		#endregion AnchorableTitleTemplate

		#region AnchorableTitleTemplateSelector

		/// <summary><see cref="AnchorableTitleTemplateSelector"/> dependency property.</summary>
		public static readonly DependencyProperty AnchorableTitleTemplateSelectorProperty = DependencyProperty.Register(nameof(AnchorableTitleTemplateSelector), typeof(DataTemplateSelector), typeof(DockingManager),
				new FrameworkPropertyMetadata(null, OnAnchorableTitleTemplateSelectorChanged));

		/// <summary>Gets/sets the <see cref="DataTemplateSelector"/> to use when selecting a <see cref="DataTemplate"/> for the title of an anchorable.</summary>
		[Bindable(true), Description("Gets/sets the DataTemplateSelector to use when selecting a DataTemplate for the title of an anchorable."), Category("Anchorable")]
		public DataTemplateSelector AnchorableTitleTemplateSelector
		{
			get => (DataTemplateSelector)GetValue(AnchorableTitleTemplateSelectorProperty);
			set => SetValue(AnchorableTitleTemplateSelectorProperty, value);
		}

		/// <summary>Handles changes to the <see cref="AnchorableTitleTemplateSelector"/> property.</summary>
		private static void OnAnchorableTitleTemplateSelectorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((DockingManager)d).OnAnchorableTitleTemplateSelectorChanged(e);

		/// <summary>Provides derived classes an opportunity to handle changes to the <see cref="AnchorableTitleTemplateSelector"/> property.</summary>
		protected virtual void OnAnchorableTitleTemplateSelectorChanged(DependencyPropertyChangedEventArgs e)
		{
			if (e.NewValue != null && AnchorableTitleTemplate != null)
				AnchorableTitleTemplate = null;
		}

		#endregion AnchorableTitleTemplateSelector

		#region AnchorableHeaderTemplate

		/// <summary><see cref="AnchorableHeaderTemplate"/> dependency property.</summary>
		public static readonly DependencyProperty AnchorableHeaderTemplateProperty = DependencyProperty.Register(nameof(AnchorableHeaderTemplate), typeof(DataTemplate), typeof(DockingManager),
				new FrameworkPropertyMetadata(null, OnAnchorableHeaderTemplateChanged, CoerceAnchorableHeaderTemplateValue));

		/// <summary>Gets/sets the <see cref="DataTemplate"/> to use for a header of an anchorable.</summary>
		[Bindable(true), Description("Gets/sets the DataTemplate to use for a header of an anchorable"), Category("Anchorable")]
		public DataTemplate AnchorableHeaderTemplate
		{
			get => (DataTemplate)GetValue(AnchorableHeaderTemplateProperty);
			set => SetValue(AnchorableHeaderTemplateProperty, value);
		}

		/// <summary>Handles changes to the <see cref="AnchorableHeaderTemplate"/> property.</summary>
		private static void OnAnchorableHeaderTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((DockingManager)d).OnAnchorableHeaderTemplateChanged(e);

		/// <summary>Provides derived classes an opportunity to handle changes to the <see cref="AnchorableHeaderTemplate"/> property.</summary>
		protected virtual void OnAnchorableHeaderTemplateChanged(DependencyPropertyChangedEventArgs e)
		{
		}

		/// <summary>Coerces the <see cref="AnchorableHeaderTemplate"/> value.</summary>
		private static object CoerceAnchorableHeaderTemplateValue(DependencyObject d, object value)
		{
			if (value != null && d.GetValue(AnchorableHeaderTemplateSelectorProperty) != null)
				return null;
			return value;
		}

		#endregion AnchorableHeaderTemplate

		#region AnchorableHeaderTemplateSelector

		/// <summary><see cref="AnchorableHeaderTemplateSelector"/> dependency property.</summary>
		public static readonly DependencyProperty AnchorableHeaderTemplateSelectorProperty = DependencyProperty.Register(nameof(AnchorableHeaderTemplateSelector), typeof(DataTemplateSelector), typeof(DockingManager),
				new FrameworkPropertyMetadata((DataTemplateSelector)null, OnAnchorableHeaderTemplateSelectorChanged));

		/// <summary>Gets/sets the <see cref="DataTemplateSelector"/> to use for selecting the <see cref="DataTemplate"/> for the header of an anchorable.</summary>
		[Bindable(true), Description("Gets/sets the DataTemplateSelector to use for selecting the DataTemplate for the header of an anchorable."), Category("Anchorable")]
		public DataTemplateSelector AnchorableHeaderTemplateSelector
		{
			get => (DataTemplateSelector)GetValue(AnchorableHeaderTemplateSelectorProperty);
			set => SetValue(AnchorableHeaderTemplateSelectorProperty, value);
		}

		/// <summary>Handles changes to the <see cref="AnchorableHeaderTemplateSelector"/> property.</summary>
		private static void OnAnchorableHeaderTemplateSelectorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((DockingManager)d).OnAnchorableHeaderTemplateSelectorChanged(e);

		/// <summary>Provides derived classes an opportunity to handle changes to the <see cref="AnchorableHeaderTemplateSelector"/> property.</summary>
		protected virtual void OnAnchorableHeaderTemplateSelectorChanged(DependencyPropertyChangedEventArgs e)
		{
			if (e.NewValue != null)
				AnchorableHeaderTemplate = null;
		}

		#endregion AnchorableHeaderTemplateSelector

		#region LayoutRootPanel

		/// <summary><see cref="LayoutRootPanel"/> dependency property.</summary>
		public static readonly DependencyProperty LayoutRootPanelProperty = DependencyProperty.Register(nameof(LayoutRootPanel), typeof(LayoutPanelControl), typeof(DockingManager),
				new FrameworkPropertyMetadata(null, OnLayoutRootPanelChanged));

		/// <summary>Gets/sets the layout <see cref="LayoutPanelControl"/> which is attached to the Layout.Root property.</summary>
		[Bindable(true), Description("Gets/sets the layout LayoutPanelControl which is attached to the Layout.Root property."), Category("Layout")]
		public LayoutPanelControl LayoutRootPanel
		{
			get => (LayoutPanelControl)GetValue(LayoutRootPanelProperty);
			set => SetValue(LayoutRootPanelProperty, value);
		}

		/// <summary>Handles changes to the <see cref="LayoutRootPanel"/> property.</summary>
		private static void OnLayoutRootPanelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((DockingManager)d).OnLayoutRootPanelChanged(e);

		/// <summary>Provides derived classes an opportunity to handle changes to the <see cref="LayoutRootPanel"/> property.</summary>
		protected virtual void OnLayoutRootPanelChanged(DependencyPropertyChangedEventArgs e)
		{
			if (e.OldValue != null)
				InternalRemoveLogicalChild(e.OldValue);
			if (e.NewValue != null)
				InternalAddLogicalChild(e.NewValue);
		}

		#endregion LayoutRootPanel

		#region RightSidePanel

		/// <summary><see cref="RightSidePanel"/> dependency property.</summary>
		public static readonly DependencyProperty RightSidePanelProperty = DependencyProperty.Register(nameof(RightSidePanel), typeof(LayoutAnchorSideControl), typeof(DockingManager),
				new FrameworkPropertyMetadata(null, OnRightSidePanelChanged));

		/// <summary>Gets/sets the <see cref="LayoutAnchorSideControl"/> that is displayed as right side panel control.</summary>
		[Bindable(true), Description("Gets/sets the LayoutAnchorSideControl that is displayed as right side panel control."), Category("Side Panel")]
		public LayoutAnchorSideControl RightSidePanel
		{
			get => (LayoutAnchorSideControl)GetValue(RightSidePanelProperty);
			set => SetValue(RightSidePanelProperty, value);
		}

		/// <summary>Handles changes to the <see cref="RightSidePanel"/> property.</summary>
		private static void OnRightSidePanelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((DockingManager)d).OnRightSidePanelChanged(e);

		/// <summary>Provides derived classes an opportunity to handle changes to the <see cref="RightSidePanel"/> property.</summary>
		protected virtual void OnRightSidePanelChanged(DependencyPropertyChangedEventArgs e)
		{
			if (e.OldValue != null)
				InternalRemoveLogicalChild(e.OldValue);
			if (e.NewValue != null)
				InternalAddLogicalChild(e.NewValue);
		}

		#endregion RightSidePanel

		#region LeftSidePanel

		/// <summary><see cref="LeftSidePanel"/> dependency property.</summary>
		public static readonly DependencyProperty LeftSidePanelProperty = DependencyProperty.Register(nameof(LeftSidePanel), typeof(LayoutAnchorSideControl), typeof(DockingManager),
				new FrameworkPropertyMetadata(null, OnLeftSidePanelChanged));

		/// <summary>Gets/sets the <see cref="LayoutAnchorSideControl"/> that is displayed as left side panel control.</summary>
		[Bindable(true), Description("Gets/sets the LayoutAnchorSideControl that is displayed as left side panel control."), Category("Side Panel")]
		public LayoutAnchorSideControl LeftSidePanel
		{
			get => (LayoutAnchorSideControl)GetValue(LeftSidePanelProperty);
			set => SetValue(LeftSidePanelProperty, value);
		}

		/// <summary>Handles changes to the <see cref="LeftSidePanel"/> property.</summary>
		private static void OnLeftSidePanelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((DockingManager)d).OnLeftSidePanelChanged(e);

		/// <summary>Provides derived classes an opportunity to handle changes to the <see cref="LeftSidePanel"/> property.</summary>
		protected virtual void OnLeftSidePanelChanged(DependencyPropertyChangedEventArgs e)
		{
			if (e.OldValue != null)
				InternalRemoveLogicalChild(e.OldValue);
			if (e.NewValue != null)
				InternalAddLogicalChild(e.NewValue);
		}

		#endregion LeftSidePanel

		#region TopSidePanel

		/// <summary><see cref="TopSidePanel"/> dependency property.</summary>
		public static readonly DependencyProperty TopSidePanelProperty = DependencyProperty.Register(nameof(TopSidePanel), typeof(LayoutAnchorSideControl), typeof(DockingManager),
				new FrameworkPropertyMetadata(null, OnTopSidePanelChanged));

		/// <summary>Gets/sets the <see cref="LayoutAnchorSideControl"/> that is displayed as top side panel control.</summary>
		[Bindable(true), Description("Gets/sets the LayoutAnchorSideControl that is displayed as top side panel control."), Category("Side Panel")]
		public LayoutAnchorSideControl TopSidePanel
		{
			get => (LayoutAnchorSideControl)GetValue(TopSidePanelProperty);
			set => SetValue(TopSidePanelProperty, value);
		}

		/// <summary>Handles changes to the <see cref="TopSidePanel"/> property.</summary>
		private static void OnTopSidePanelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((DockingManager)d).OnTopSidePanelChanged(e);

		/// <summary>Provides derived classes an opportunity to handle changes to the <see cref="TopSidePanel"/> property.</summary>
		protected virtual void OnTopSidePanelChanged(DependencyPropertyChangedEventArgs e)
		{
			if (e.OldValue != null)
				InternalRemoveLogicalChild(e.OldValue);
			if (e.NewValue != null)
				InternalAddLogicalChild(e.NewValue);
		}

		#endregion TopSidePanel

		#region BottomSidePanel

		/// <summary><see cref="BottomSidePanel"/> dependency property. </summary>
		public static readonly DependencyProperty BottomSidePanelProperty = DependencyProperty.Register(nameof(BottomSidePanel), typeof(LayoutAnchorSideControl), typeof(DockingManager),
				new FrameworkPropertyMetadata(null, OnBottomSidePanelChanged));

		/// <summary>Gets/sets the <see cref="LayoutAnchorSideControl"/> that is displayed as bottom side panel control.</summary>
		[Bindable(true), Description("Gets/sets the LayoutAnchorSideControl that is displayed as bottom side panel control."), Category("Side Panel")]
		public LayoutAnchorSideControl BottomSidePanel
		{
			get => (LayoutAnchorSideControl)GetValue(BottomSidePanelProperty);
			set => SetValue(BottomSidePanelProperty, value);
		}

		/// <summary>Handles changes to the <see cref="BottomSidePanel"/> property.</summary>
		private static void OnBottomSidePanelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((DockingManager)d).OnBottomSidePanelChanged(e);

		/// <summary>Provides derived classes an opportunity to handle changes to the <see cref="BottomSidePanel"/> property.</summary>
		protected virtual void OnBottomSidePanelChanged(DependencyPropertyChangedEventArgs e)
		{
			if (e.OldValue != null)
				InternalRemoveLogicalChild(e.OldValue);
			if (e.NewValue != null)
				InternalAddLogicalChild(e.NewValue);
		}

		#endregion BottomSidePanel

		#region AutoHideWindow

		/// <summary><see cref="AutoHideWindow"/> Read-Only dependency property.</summary>
		private static readonly DependencyPropertyKey AutoHideWindowPropertyKey = DependencyProperty.RegisterReadOnly(nameof(AutoHideWindow), typeof(LayoutAutoHideWindowControl), typeof(DockingManager),
				new FrameworkPropertyMetadata(null, OnAutoHideWindowChanged));

		public static readonly DependencyProperty AutoHideWindowProperty = AutoHideWindowPropertyKey.DependencyProperty;

		/// <summary>Gets the <see cref="LayoutAutoHideWindowControl"/> that is currently shown as autohide window.</summary>
		[Bindable(true), Description("Gets the LayoutAutoHideWindowControl that is currently shown as autohide window."), Category("AutoHideWindow")]
		public LayoutAutoHideWindowControl AutoHideWindow => (LayoutAutoHideWindowControl)GetValue(AutoHideWindowProperty);

		/// <summary>
		/// Provides a secure method for setting the <see cref="AutoHideWindow"/> property.
		/// This dependency property indicates the currently shown autohide window.
		/// </summary>
		/// <param name="value">The new value for the property.</param>
		protected void SetAutoHideWindow(LayoutAutoHideWindowControl value) => SetValue(AutoHideWindowPropertyKey, value);

		/// <summary>Handles changes to the <see cref="AutoHideWindow"/> property.</summary>
		private static void OnAutoHideWindowChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((DockingManager)d).OnAutoHideWindowChanged(e);

		/// <summary>Provides derived classes an opportunity to handle changes to the <see cref="AutoHideWindow"/> property.</summary>
		protected virtual void OnAutoHideWindowChanged(DependencyPropertyChangedEventArgs e)
		{
			if (e.OldValue != null)
				InternalRemoveLogicalChild(e.OldValue);
			if (e.NewValue != null)
				InternalAddLogicalChild(e.NewValue);
		}

		#endregion AutoHideWindow

		#region AutoHideDelay

		/// <summary>
		/// Implements the backing store of the <see cref="AutoHideDelay"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty AutoHideDelayProperty =
			DependencyProperty.Register("AutoHideDelay", typeof(int), typeof(DockingManager),
										new UIPropertyMetadata(500));

		/// <summary>
		/// Gets/sets the wait time in milliseconds that is applicable when the system AutoHides
		/// a <see cref="LayoutAnchorableControl"/> (reduces it to a side anchor) after the user:
		///
		/// 1) clicks on a <see cref="LayoutAnchorControl "/> that is anchored in one of the <see cref="Layout"/>
		/// property sides (top, right, left, or bottom) and
		///
		/// 2) clicks somewhere else into a focusable element (different document).
		///
		/// Expected behavior: The system waits for the configured time and reduces the <see cref="LayoutAnchorableControl"/> (into its side anchor).
		/// Recommended configuration value range should be between 0 and 1500 milliseconds.
		/// </summary>
		[Bindable(true), Description("Gets/sets the wait time in milliseconds that is applicable when the system AutoHides a LayoutAnchorableControl (reduces it to a side anchor)."), Category("AutoHideWindow")]
		public int AutoHideDelay
		{
			get => (int)GetValue(AutoHideDelayProperty);
			set => SetValue(AutoHideDelayProperty, value);
		}

		#endregion AutoHideDelay

		/// <summary>Enumerates all <see cref="LayoutFloatingWindowControl"/>s managed by this framework.</summary>
		[Bindable(false), Description("Enumerates all LayoutFloatingWindowControls managed by this framework."), Category("FloatingWindow")]
		public IEnumerable<LayoutFloatingWindowControl> FloatingWindows => _fwList;

		#region LayoutItemTemplate

		/// <summary><see cref="LayoutItemTemplate"/> dependency property.</summary>
		public static readonly DependencyProperty LayoutItemTemplateProperty = DependencyProperty.Register(nameof(LayoutItemTemplate), typeof(DataTemplate), typeof(DockingManager),
				new FrameworkPropertyMetadata((DataTemplate)null, OnLayoutItemTemplateChanged));

		/// <summary>Gets/sets the <see cref="DataTemplate"/> used to render anchorable and document content.</summary>
		[Bindable(true), Description("Gets/sets the DataTemplate used to render anchorable and document content."), Category("Layout")]
		public DataTemplate LayoutItemTemplate
		{
			get => (DataTemplate)GetValue(LayoutItemTemplateProperty);
			set => SetValue(LayoutItemTemplateProperty, value);
		}

		/// <summary>Handles changes to the <see cref="LayoutItemTemplate"/> property.</summary>
		private static void OnLayoutItemTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((DockingManager)d).OnLayoutItemTemplateChanged(e);

		/// <summary>Provides derived classes an opportunity to handle changes to the <see cref="LayoutItemTemplate"/> property.</summary>
		protected virtual void OnLayoutItemTemplateChanged(DependencyPropertyChangedEventArgs e)
		{
		}

		#endregion LayoutItemTemplate

		#region LayoutItemTemplateSelector

		/// <summary><see cref="LayoutItemTemplateSelector"/> dependency property.</summary>
		public static readonly DependencyProperty LayoutItemTemplateSelectorProperty = DependencyProperty.Register(nameof(LayoutItemTemplateSelector), typeof(DataTemplateSelector), typeof(DockingManager),
				new FrameworkPropertyMetadata(null, OnLayoutItemTemplateSelectorChanged));

		/// <summary>Gets/sets the <see cref="DataTemplateSelector"/> to select a <see cref="DataTemplate"/> of an anchorable.</summary>
		[Bindable(true), Description("Gets/sets the DataTemplateSelector to select a DataTemplate of an anchorable."), Category("Layout")]
		public DataTemplateSelector LayoutItemTemplateSelector
		{
			get => (DataTemplateSelector)GetValue(LayoutItemTemplateSelectorProperty);
			set => SetValue(LayoutItemTemplateSelectorProperty, value);
		}

		/// <summary>Handles changes to the <see cref="LayoutItemTemplateSelector"/> property.</summary>
		private static void OnLayoutItemTemplateSelectorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((DockingManager)d).OnLayoutItemTemplateSelectorChanged(e);

		/// <summary>Provides derived classes an opportunity to handle changes to the <see cref="LayoutItemTemplateSelector"/> property.</summary>
		protected virtual void OnLayoutItemTemplateSelectorChanged(DependencyPropertyChangedEventArgs e)
		{
		}

		#endregion LayoutItemTemplateSelector

		#region DocumentsSource

		/// <summary><see cref="DocumentsSource"/> dependency property.</summary>
		public static readonly DependencyProperty DocumentsSourceProperty = DependencyProperty.Register(nameof(DocumentsSource), typeof(IEnumerable), typeof(DockingManager),
				new FrameworkPropertyMetadata(null, OnDocumentsSourceChanged));

		/// <summary>Gets/sets the source collection of <see cref="LayoutDocument"/> objects.</summary>
		[Bindable(true), Description("Gets/sets the source collection of LayoutDocument objects."), Category("Document")]
		public IEnumerable DocumentsSource
		{
			get => (IEnumerable)GetValue(DocumentsSourceProperty);
			set => SetValue(DocumentsSourceProperty, value);
		}

		/// <summary>Handles changes to the <see cref="DocumentsSource"/> property.</summary>
		private static void OnDocumentsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((DockingManager)d).OnDocumentsSourceChanged(e);

		/// <summary>Provides derived classes an opportunity to handle changes to the <see cref="DocumentsSource"/> property.</summary>
		protected virtual void OnDocumentsSourceChanged(DependencyPropertyChangedEventArgs e)
		{
			DetachDocumentsSource(Layout, e.OldValue as IEnumerable);
			AttachDocumentsSource(Layout, e.NewValue as IEnumerable);
		}

		#endregion DocumentsSource

		#region DocumentContextMenu

		/// <summary><see cref="DocumentContextMenu"/> dependency property.</summary>
		public static readonly DependencyProperty DocumentContextMenuProperty = DependencyProperty.Register(nameof(DocumentContextMenu), typeof(ContextMenu), typeof(DockingManager),
				new FrameworkPropertyMetadata((ContextMenu)null, OnContextMenuPropertyChanged));

		/// <summary>Gets/sets the <see cref="ContextMenu"/> to show for a document.</summary>
		[Bindable(true), Description("Gets/sets the ContextMenu to show for a document."), Category("Document")]
		public ContextMenu DocumentContextMenu
		{
			get => (ContextMenu)GetValue(DocumentContextMenuProperty);
			set => SetValue(DocumentContextMenuProperty, value);
		}

		#endregion DocumentContextMenu

		#region AnchorablesSource

		/// <summary><see cref="AnchorablesSource"/> dependency property.</summary>
		public static readonly DependencyProperty AnchorablesSourceProperty = DependencyProperty.Register(nameof(AnchorablesSource), typeof(IEnumerable), typeof(DockingManager),
				new FrameworkPropertyMetadata(null, OnAnchorablesSourceChanged));

		/// <summary>Gets/sets the source collection for all <see cref="LayoutAnchorable"/> objects managed in this framework.</summary>
		[Bindable(true), Description("Gets/sets the source collection for all LayoutAnchorable objects managed in this framework."), Category("Anchorable")]
		public IEnumerable AnchorablesSource
		{
			get => (IEnumerable)GetValue(AnchorablesSourceProperty);
			set => SetValue(AnchorablesSourceProperty, value);
		}

		/// <summary>Handles changes to the <see cref="AnchorablesSource"/> property.</summary>
		private static void OnAnchorablesSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((DockingManager)d).OnAnchorablesSourceChanged(e);

		/// <summary>Provides derived classes an opportunity to handle changes to the <see cref="AnchorablesSource"/> property.</summary>
		protected virtual void OnAnchorablesSourceChanged(DependencyPropertyChangedEventArgs e)
		{
			DetachAnchorablesSource(Layout, e.OldValue as IEnumerable);
			AttachAnchorablesSource(Layout, e.NewValue as IEnumerable);
		}

		#endregion AnchorablesSource

		#region ActiveContent

		/// <summary><see cref="ActiveContent"/> dependency property.</summary>
		public static readonly DependencyProperty ActiveContentProperty = DependencyProperty.Register(nameof(ActiveContent), typeof(object), typeof(DockingManager),
				new FrameworkPropertyMetadata(null, OnActiveContentChanged));

		/// <summary>Gets/sets the content that is currently active (document,anchoreable, or null).</summary>
		[Bindable(true), Description("Gets/sets the content that is currently active (document,anchoreable, or null)."), Category("Other")]
		public object ActiveContent
		{
			get => (object)GetValue(ActiveContentProperty);
			set => SetValue(ActiveContentProperty, value);
		}

		/// <summary>Handles changes to the <see cref="ActiveContent"/> property.</summary>
		private static void OnActiveContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((DockingManager)d).InternalSetActiveContent(e.NewValue);
			((DockingManager)d).OnActiveContentChanged(e);
		}

		/// <summary>Provides derived classes an opportunity to handle changes to the <see cref="ActiveContent"/> property.</summary>
		protected virtual void OnActiveContentChanged(DependencyPropertyChangedEventArgs e) => ActiveContentChanged?.Invoke(this, EventArgs.Empty);

		#endregion ActiveContent

		#region AnchorableContextMenu

		/// <summary><see cref="AnchorableContextMenu"/> dependency property.</summary>
		public static readonly DependencyProperty AnchorableContextMenuProperty = DependencyProperty.Register(nameof(AnchorableContextMenu), typeof(ContextMenu), typeof(DockingManager),
				new FrameworkPropertyMetadata((ContextMenu)null, OnContextMenuPropertyChanged));

		/// <summary>Gets/sets the <see cref="ContextMenu"/> to show on an anchorable.</summary>
		[Bindable(true), Description("Gets/sets the ContextMenu to show on an anchorable."), Category("Anchorable")]
		public ContextMenu AnchorableContextMenu
		{
			get => (ContextMenu)GetValue(AnchorableContextMenuProperty);
			set => SetValue(AnchorableContextMenuProperty, value);
		}

		#endregion AnchorableContextMenu

		#region Theme

		/// <summary><see cref="Theme"/> dependency property.</summary>
		public static readonly DependencyProperty ThemeProperty = DependencyProperty.Register(nameof(Theme), typeof(Theme), typeof(DockingManager),
				new FrameworkPropertyMetadata(null, OnThemeChanged));

		/// <summary>Gets/sets the <see cref="Theme"/> to be used for the controls in this framework.</summary>
		[Bindable(true), Description("Gets/sets the Theme to be used for the controls in this framework."), Category("Other")]
		public Theme Theme
		{
			get => (Theme)GetValue(ThemeProperty);
			set => SetValue(ThemeProperty, value);
		}

		/// <summary>Handles changes to the <see cref="Theme"/> property.</summary>
		private static void OnThemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((DockingManager)d).OnThemeChanged(e);

		/// <summary>Provides derived classes an opportunity to handle changes to the <see cref="Theme"/> property.</summary>
		protected virtual void OnThemeChanged(DependencyPropertyChangedEventArgs e)
		{
			var oldTheme = e.OldValue as Theme;
			var resources = Resources;
			if (oldTheme != null)        // remove old theme from resource dictionary if present
			{
				if (oldTheme is DictionaryTheme)  // We are using AvalonDock's own DictionaryTheme class
				{
					if (currentThemeResourceDictionary != null)
					{
						resources.MergedDictionaries.Remove(currentThemeResourceDictionary);
						currentThemeResourceDictionary = null;
					}
				}
				else                              // We are using standard ResourceDictionaries
				{                                // Lockup the old theme and remove it from resource dictionary
					var resourceDictionaryToRemove =
						resources.MergedDictionaries.FirstOrDefault(r => r.Source == oldTheme.GetResourceUri());
					if (resourceDictionaryToRemove != null)
						resources.MergedDictionaries.Remove(
							resourceDictionaryToRemove);
				}
			}

			if (e.NewValue as Theme != null) // Add new theme into resource dictionary if present
			{
				if (e.NewValue as Theme is DictionaryTheme theme)
				{
					currentThemeResourceDictionary = theme.ThemeResourceDictionary;
					resources.MergedDictionaries.Add(currentThemeResourceDictionary);
				}
				else                              // We are using standard ResourceDictionaries -> Add new theme resource
					resources.MergedDictionaries.Add(new ResourceDictionary { Source = (e.NewValue as Theme).GetResourceUri() });
			}

			foreach (var fwc in _fwList)               // Update theme resources in floating window controls
				fwc.UpdateThemeResources(oldTheme);

			_navigatorWindow?.UpdateThemeResources();  // Update theme resources in related AvalonDock controls
			_overlayWindow?.UpdateThemeResources();
		}

		#endregion Theme

		#region GridSplitterWidth

		/// <summary><see cref="GridSplitterWidth"/> dependency property.</summary>
		public static readonly DependencyProperty GridSplitterWidthProperty = DependencyProperty.Register(nameof(GridSplitterWidth), typeof(double), typeof(DockingManager),
				new FrameworkPropertyMetadata(6.0));

		/// <summary>Gets/sets the width of a grid splitter.</summary>
		[Bindable(true), Description("Gets/sets the width of a grid splitter"), Category("Other")]
		public double GridSplitterWidth
		{
			get => (double)GetValue(GridSplitterWidthProperty);
			set => SetValue(GridSplitterWidthProperty, value);
		}

		#endregion GridSplitterWidth

		#region GridSplitterHeight

		/// <summary><see cref="GridSplitterHeight"/> dependency property.</summary>
		public static readonly DependencyProperty GridSplitterHeightProperty = DependencyProperty.Register(nameof(GridSplitterHeight), typeof(double), typeof(DockingManager),
				new FrameworkPropertyMetadata(6.0));

		/// <summary>Gets/sets the height of a grid splitter.</summary>
		[Bindable(true), Description("Gets/sets the height of a grid splitter"), Category("Other")]
		public double GridSplitterHeight
		{
			get => (double)GetValue(GridSplitterHeightProperty);
			set => SetValue(GridSplitterHeightProperty, value);
		}

		#endregion GridSplitterHeight

		#region GridSplitterVerticalStyle

		/// <summary>
		/// GridSplitterVerticalStyle Dependency Property
		/// </summary>
		public static readonly DependencyProperty GridSplitterVerticalStyleProperty = DependencyProperty.Register("GridSplitterVerticalStyle", typeof(Style), typeof(DockingManager),
				new FrameworkPropertyMetadata((Style)null));

		/// <summary>
		/// Gets or sets the GridSplitterVerticalStyle property.  This dependency property 
		/// indicates the style to apply to the LayoutGridResizerControl when displayed vertically.
		/// </summary>
		public Style GridSplitterVerticalStyle
		{
			get
			{
				return (Style)GetValue(GridSplitterVerticalStyleProperty);
			}
			set
			{
				SetValue(GridSplitterVerticalStyleProperty, value);
			}
		}

		#endregion

		#region GridSplitterHorizontalStyle

		/// <summary>
		/// GridSplitterHorizontalStyle Dependency Property
		/// </summary>
		public static readonly DependencyProperty GridSplitterHorizontalStyleProperty = DependencyProperty.Register("GridSplitterHorizontalStyle", typeof(Style), typeof(DockingManager),
				new FrameworkPropertyMetadata((Style)null));

		/// <summary>
		/// Gets or sets the GridSplitterHorizontalStyle property.  This dependency property 
		/// indicates the style to apply to the LayoutGridResizerControl when displayed horizontally.
		/// </summary>
		public Style GridSplitterHorizontalStyle
		{
			get
			{
				return (Style)GetValue(GridSplitterHorizontalStyleProperty);
			}
			set
			{
				SetValue(GridSplitterHorizontalStyleProperty, value);
			}
		}

		#endregion


		#region DocumentPaneMenuItemHeaderTemplate

		/// <summary><see cref="DocumentPaneMenuItemHeaderTemplate"/> dependency property.</summary>
		public static readonly DependencyProperty DocumentPaneMenuItemHeaderTemplateProperty = DependencyProperty.Register(nameof(DocumentPaneMenuItemHeaderTemplate), typeof(DataTemplate), typeof(DockingManager),
				new FrameworkPropertyMetadata(null, OnDocumentPaneMenuItemHeaderTemplateChanged, CoerceDocumentPaneMenuItemHeaderTemplateValue));

		/// <summary>Gets/sets the <see cref="DataTemplate"/> for the header to display menu dropdown items on a document pane.</summary>
		[Bindable(true), Description("Gets/sets the DataTemplate for the header to display menu dropdown items on a document pane."), Category("Other")]
		public DataTemplate DocumentPaneMenuItemHeaderTemplate
		{
			get => (DataTemplate)GetValue(DocumentPaneMenuItemHeaderTemplateProperty);
			set => SetValue(DocumentPaneMenuItemHeaderTemplateProperty, value);
		}

		/// <summary>Handles changes to the <see cref="DocumentPaneMenuItemHeaderTemplate"/> property.</summary>
		private static void OnDocumentPaneMenuItemHeaderTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((DockingManager)d).OnDocumentPaneMenuItemHeaderTemplateChanged(e);

		/// <summary>Provides derived classes an opportunity to handle changes to the <see cref="DocumentPaneMenuItemHeaderTemplate"/> property.</summary>
		protected virtual void OnDocumentPaneMenuItemHeaderTemplateChanged(DependencyPropertyChangedEventArgs e)
		{
		}

		/// <summary>Coerces the <see cref="DocumentPaneMenuItemHeaderTemplate"/> value.</summary>
		private static object CoerceDocumentPaneMenuItemHeaderTemplateValue(DependencyObject d, object value)
		{
			if (value != null && d.GetValue(DocumentPaneMenuItemHeaderTemplateSelectorProperty) != null)
				return null;
			return value ?? d.GetValue(DocumentHeaderTemplateProperty);
		}

		#endregion DocumentPaneMenuItemHeaderTemplate

		#region DocumentPaneMenuItemHeaderTemplateSelector

		/// <summary><see cref="DocumentPaneMenuItemHeaderTemplateSelector"/> dependency property.</summary>
		public static readonly DependencyProperty DocumentPaneMenuItemHeaderTemplateSelectorProperty = DependencyProperty.Register(nameof(DocumentPaneMenuItemHeaderTemplateSelector), typeof(DataTemplateSelector), typeof(DockingManager),
				new FrameworkPropertyMetadata(null, OnDocumentPaneMenuItemHeaderTemplateSelectorChanged, CoerceDocumentPaneMenuItemHeaderTemplateSelectorValue));

		/// <summary>Gets/sets the <see cref="DataTemplateSelector"/> to select a <see cref="DataTemplate"/> for the menu items shown when user selects the <see cref="LayoutDocumentPaneControl"/> context menu switch.</summary>
		[Bindable(true), Description("Gets/sets the DataTemplateSelector to select a DataTemplate for the menu items shown when user selects the LayoutDocumentPaneControl context menu switch."), Category("Document")]
		public DataTemplateSelector DocumentPaneMenuItemHeaderTemplateSelector
		{
			get => (DataTemplateSelector)GetValue(DocumentPaneMenuItemHeaderTemplateSelectorProperty);
			set => SetValue(DocumentPaneMenuItemHeaderTemplateSelectorProperty, value);
		}

		/// <summary>Handles changes to the <see cref="DocumentPaneMenuItemHeaderTemplateSelector"/> property.</summary>
		private static void OnDocumentPaneMenuItemHeaderTemplateSelectorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((DockingManager)d).OnDocumentPaneMenuItemHeaderTemplateSelectorChanged(e);

		/// <summary>Provides derived classes an opportunity to handle changes to the <see cref="DocumentPaneMenuItemHeaderTemplateSelector"/> property.</summary>
		protected virtual void OnDocumentPaneMenuItemHeaderTemplateSelectorChanged(DependencyPropertyChangedEventArgs e)
		{
			if (e.NewValue != null && DocumentPaneMenuItemHeaderTemplate != null)
				DocumentPaneMenuItemHeaderTemplate = null;
		}

		/// <summary>Coerces the <see cref="DocumentPaneMenuItemHeaderTemplateSelector"/> value.</summary>
		private static object CoerceDocumentPaneMenuItemHeaderTemplateSelectorValue(DependencyObject d, object value) => value;

		#endregion DocumentPaneMenuItemHeaderTemplateSelector

		#region IconContentTemplate

		/// <summary><see cref="IconContentTemplate"/> dependency property.</summary>
		public static readonly DependencyProperty IconContentTemplateProperty = DependencyProperty.Register(nameof(IconContentTemplate), typeof(DataTemplate), typeof(DockingManager),
				new FrameworkPropertyMetadata((DataTemplate)null));

		/// <summary>Gets/sets the <see cref="DataTemplate"/> to use on the icon extracted from the layout model.</summary>
		[Bindable(true), Description("Gets/sets the DataTemplate to use on the icon extracted from the layout model."), Category("Other")]
		public DataTemplate IconContentTemplate
		{
			get => (DataTemplate)GetValue(IconContentTemplateProperty);
			set => SetValue(IconContentTemplateProperty, value);
		}

		#endregion IconContentTemplate

		#region IconContentTemplateSelector

		/// <summary><see cref="IconContentTemplateSelector"/> dependency property.</summary>
		public static readonly DependencyProperty IconContentTemplateSelectorProperty = DependencyProperty.Register(nameof(IconContentTemplateSelector), typeof(DataTemplateSelector), typeof(DockingManager),
				new FrameworkPropertyMetadata((DataTemplateSelector)null));

		/// <summary>Gets/sets the <see cref="DataTemplateSelector"/> to select a <see cref="DataTemplate"/> for a content icon.</summary>
		[Bindable(true), Description("Gets/sets the DataTemplateSelector to select a DataTemplate for a content icon."), Category("Other")]
		public DataTemplateSelector IconContentTemplateSelector
		{
			get => (DataTemplateSelector)GetValue(IconContentTemplateSelectorProperty);
			set => SetValue(IconContentTemplateSelectorProperty, value);
		}

		#endregion IconContentTemplateSelector

		#region LayoutItemContainerStyle

		/// <summary><see cref="LayoutItemContainerStyle"/> dependency property.</summary>
		public static readonly DependencyProperty LayoutItemContainerStyleProperty = DependencyProperty.Register(nameof(LayoutItemContainerStyle), typeof(Style), typeof(DockingManager),
				new FrameworkPropertyMetadata(null, OnLayoutItemContainerStyleChanged));

		/// <summary>Gets/sets the <see cref="Style"/> to apply to a <see cref="LayoutDocumentItem"/> object.</summary>
		[Bindable(true), Description("Gets/sets the Style to apply to a LayoutDocumentItem object."), Category("Layout")]
		public Style LayoutItemContainerStyle
		{
			get => (Style)GetValue(LayoutItemContainerStyleProperty);
			set => SetValue(LayoutItemContainerStyleProperty, value);
		}

		/// <summary>Handles changes to the <see cref="LayoutItemContainerStyle"/> property.</summary>
		private static void OnLayoutItemContainerStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((DockingManager)d).OnLayoutItemContainerStyleChanged(e);

		/// <summary>Provides derived classes an opportunity to handle changes to the <see cref="LayoutItemContainerStyle"/> property.</summary>
		protected virtual void OnLayoutItemContainerStyleChanged(DependencyPropertyChangedEventArgs e) => AttachLayoutItems();

		#endregion LayoutItemContainerStyle

		#region LayoutItemContainerStyleSelector

		/// <summary><see cref="LayoutItemContainerStyleSelector"/> dependency property.</summary>
		public static readonly DependencyProperty LayoutItemContainerStyleSelectorProperty = DependencyProperty.Register(nameof(LayoutItemContainerStyleSelector), typeof(StyleSelector), typeof(DockingManager),
				new FrameworkPropertyMetadata(null, OnLayoutItemContainerStyleSelectorChanged));

		/// <summary>Gets/sets the <see cref="StyleSelector"/> to select the <see cref="Style"/> for a <see cref="LayoutDocumentItem"/>.</summary>
		[Bindable(true), Description("Gets/sets the StyleSelector to select the Style for a LayoutDocumentItem."), Category("Layout")]
		public StyleSelector LayoutItemContainerStyleSelector
		{
			get => (StyleSelector)GetValue(LayoutItemContainerStyleSelectorProperty);
			set => SetValue(LayoutItemContainerStyleSelectorProperty, value);
		}

		/// <summary>Handles changes to the <see cref="LayoutItemContainerStyleSelector"/> property.</summary>
		private static void OnLayoutItemContainerStyleSelectorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((DockingManager)d).OnLayoutItemContainerStyleSelectorChanged(e);

		/// <summary>Provides derived classes an opportunity to handle changes to the <see cref="LayoutItemContainerStyleSelector"/> property.</summary>
		protected virtual void OnLayoutItemContainerStyleSelectorChanged(DependencyPropertyChangedEventArgs e) => AttachLayoutItems();

		#endregion LayoutItemContainerStyleSelector

		#region ShowSystemMenu

		/// <summary><see cref="ShowSystemMenu"/> dependency property.</summary>
		public static readonly DependencyProperty ShowSystemMenuProperty = DependencyProperty.Register(nameof(ShowSystemMenu), typeof(bool), typeof(DockingManager),
				new FrameworkPropertyMetadata(true));

		/// <summary>Gets/sets whether floating windows should show the system menu when a custom context menu is not defined.</summary>
		[Bindable(true), Description("Gets/sets whether floating windows should show the system menu when a custom context menu is not defined."), Category("FloatingWindow")]
		public bool ShowSystemMenu
		{
			get => (bool)GetValue(ShowSystemMenuProperty);
			set => SetValue(ShowSystemMenuProperty, value);
		}

		#endregion ShowSystemMenu

		#region AllowMixedOrientation

		/// <summary><see cref="AllowMixedOrientation"/> dependency property.</summary>
		public static readonly DependencyProperty AllowMixedOrientationProperty = DependencyProperty.Register(nameof(AllowMixedOrientation), typeof(bool), typeof(DockingManager),
				new FrameworkPropertyMetadata(false));

		/// <summary>Gets/sets whether the DockingManager should allow mixed orientation for document panes.</summary>
		[Bindable(true), Description("Gets/sets whether the DockingManager should allow mixed orientation for document panes."), Category("Other")]
		public bool AllowMixedOrientation
		{
			get => (bool)GetValue(AllowMixedOrientationProperty);
			set => SetValue(AllowMixedOrientationProperty, value);
		}

		#endregion AllowMixedOrientation

		#region IsVirtualizingLayoutDocument IsVirtualizingLayoutAnchorable

		/// <summary>Gets/sets (a simple non-dependency property) to determine whether the
		/// <see cref="LayoutDocumentPaneControl"/> is virtualizing its tabbed item child controls or not.</summary>
		[Bindable(false), Description("Gets/sets whether the LayoutDocumentPaneControl is virtualizing its tabbed item child controls or not."), Category("Document")]
		public bool IsVirtualizingDocument { get; set; }

		/// <summary>Gets/sets (a simple non-dependency property) to determine whether the
		/// <see cref="LayoutAnchorablePaneControl"/> is virtualizing its tabbed item child controls or not.</summary>
		[Bindable(false), Description("Gets/sets whether the LayoutAnchorablePaneControl is virtualizing its tabbed item child controls or not."), Category("Anchorable")]
		public bool IsVirtualizingAnchorable { get; set; }

		#endregion IsVirtualizingLayoutDocument IsVirtualizingLayoutAnchorable

		#region AutoWindowSizeWhenOpened

		/// <summary>
		/// Gets/sets whether the floating window size of a <see cref="LayoutFloatingWindowControl"/> is auto sized when it is dragged out.
		/// If true the MinHeight and MinWidth of the content will be used together with the margins to determine the initial size of the floating window.
		/// </summary>
		[Bindable(true), Description("Gets/sets whether the floating window is auto sized when it is dragged out."), Category("FloatingWindow")]
		public bool AutoWindowSizeWhenOpened
		{
			get { return (bool)GetValue(AutoWindowSizeWhenOpenedProperty); }
			set { SetValue(AutoWindowSizeWhenOpenedProperty, value); }
		}

		public static readonly DependencyProperty AutoWindowSizeWhenOpenedProperty =
			DependencyProperty.Register("AutoWindowSizeWhenOpened", typeof(bool), typeof(DockingManager), new PropertyMetadata(false));

		#endregion AutoWindowSizeWhenOpened

		#endregion Public Properties

		#region LogicalChildren

		private readonly List<WeakReference> _logicalChildren = new List<WeakReference>();

		/// <inheritdoc />
		protected override IEnumerator LogicalChildren => _logicalChildren.Select(ch => ch.GetValueOrDefault<object>()).GetEnumerator();

		public IEnumerator LogicalChildrenPublic => LogicalChildren;

		internal void InternalAddLogicalChild(object element)
		{
#if DEBUG
			if (_logicalChildren.Select(ch => ch.GetValueOrDefault<object>()).Contains(element))
				throw new InvalidOperationException();
#endif
			if (_logicalChildren.Select(ch => ch.GetValueOrDefault<object>()).Contains(element))
				return;

			_logicalChildren.Add(new WeakReference(element));
			AddLogicalChild(element);
		}

		internal void InternalRemoveLogicalChild(object element)
		{
			var wrToRemove = _logicalChildren.FirstOrDefault(ch => ch.GetValueOrDefault<object>() == element);
			if (wrToRemove != null)
				_logicalChildren.Remove(wrToRemove);
			RemoveLogicalChild(element);
		}

		private void ClearLogicalChildrenList()
		{
			foreach (var child in _logicalChildren.Select(ch => ch.GetValueOrDefault<object>()).ToArray())
				RemoveLogicalChild(child);
			_logicalChildren.Clear();
		}

		#endregion LogicalChildren

		#region Private Properties

		private bool IsNavigatorWindowActive => _navigatorWindow != null;

		private bool CanShowNavigatorWindow => _layoutItems.Any();

		#endregion Private Properties

		#region IOverlayWindowHost Interface

		/// <inheritdoc/>
		bool IOverlayWindowHost.HitTestScreen(Point dragPoint)
		{
			return HitTest(this.TransformToDeviceDPI(dragPoint));
		}

		bool HitTest(Point dragPoint)
		{
			try
			{
				var detectionRect = new Rect(this.PointToScreenDPIWithoutFlowDirection(new Point()), this.TransformActualSizeToAncestor());
				return detectionRect.Contains(dragPoint);
			}
			catch
			{
				// Silently swallow exception that may occur if DockingManager is not visible (because its hidden by a tab)
			}

			return false;
		}

		/// <inheritdoc/>
		DockingManager IOverlayWindowHost.Manager => this;

		/// <inheritdoc/>
		IOverlayWindow IOverlayWindowHost.ShowOverlayWindow(LayoutFloatingWindowControl draggingWindow)
		{
			CreateOverlayWindow();
			_overlayWindow.EnableDropTargets();
			_overlayWindow.Show();
			return _overlayWindow;
		}

		/// <inheritdoc/>
		void IOverlayWindowHost.HideOverlayWindow()
		{
			_areas = null;
			_overlayWindow.Owner = null;
			_overlayWindow.HideDropTargets();
			_overlayWindow.Close();
			_overlayWindow = null;
		}

		/// <inheritdoc/>
		IEnumerable<IDropArea> IOverlayWindowHost.GetDropAreas(LayoutFloatingWindowControl draggingWindow)
		{
			if (_areas != null) return _areas;
			_areas = new List<IDropArea>();
			var isDraggingDocuments = draggingWindow.Model is LayoutDocumentFloatingWindow;
			if (!isDraggingDocuments)
			{
				_areas.Add(new DropArea<DockingManager>(this, DropAreaType.DockingManager));
				foreach (var areaHost in this.FindVisualChildren<LayoutAnchorablePaneControl>())
					if (areaHost.Model.Descendents().Any()) _areas.Add(new DropArea<LayoutAnchorablePaneControl>(areaHost, DropAreaType.AnchorablePane));
			}

			// Determine if floatingWindow is configured to dock as document or not
			var dockAsDocument = true;
			if (!isDraggingDocuments)
			{
				if (draggingWindow.Model is LayoutAnchorableFloatingWindow toolWindow)
				{
					foreach (var item in GetAnchorableInFloatingWindow(draggingWindow))
					{
						if (item.CanDockAsTabbedDocument != false) continue;
						dockAsDocument = false;
						break;
					}
				}
			}

			// Dock only documents and tools in DocumentPane if configuration does allow that
			if (dockAsDocument)
			{
				foreach (var areaHost in this.FindVisualChildren<LayoutDocumentPaneControl>())
					_areas.Add(new DropArea<LayoutDocumentPaneControl>(areaHost, DropAreaType.DocumentPane));

				foreach (var areaHost in this.FindVisualChildren<LayoutDocumentPaneGroupControl>())
				{
					var documentGroupModel = areaHost.Model as LayoutDocumentPaneGroup;
					if (!documentGroupModel.Children.Any(c => c.IsVisible))
						_areas.Add(new DropArea<LayoutDocumentPaneGroupControl>(areaHost, DropAreaType.DocumentPaneGroup));
				}
			}

			return _areas;
		}

		/// <summary>
		/// Finds all <see cref="LayoutAnchorable"/> objects (tool windows) within a
		/// <see cref="LayoutFloatingWindow"/> (if any) and return them.
		/// </summary>
		/// <param name="draggingWindow"></param>
		/// <returns></returns>
		private IEnumerable<LayoutAnchorable> GetAnchorableInFloatingWindow(LayoutFloatingWindowControl draggingWindow)
		{
			if (!(draggingWindow.Model is LayoutAnchorableFloatingWindow layoutAnchorableFloatingWindow)) yield break;
			//big part of code for getting type

			if (layoutAnchorableFloatingWindow.SinglePane is LayoutAnchorablePane layoutAnchorablePane && (layoutAnchorableFloatingWindow.IsSinglePane && layoutAnchorablePane.SelectedContent != null))
			{
				var layoutAnchorable = ((LayoutAnchorablePane)layoutAnchorableFloatingWindow.SinglePane).SelectedContent as LayoutAnchorable;
				yield return layoutAnchorable;
			}
			else
				foreach (var item in GetLayoutAnchorable(layoutAnchorableFloatingWindow.RootPanel))
					yield return item;
		}

		/// <summary>
		/// Finds all <see cref="LayoutAnchorable"/> objects (toolwindows) within a
		/// <see cref="LayoutAnchorablePaneGroup"/> (if any) and return them.
		/// </summary>
		/// <param name="layoutAnchPaneGroup"></param>
		/// <returns>All the anchorable items found.</returns>
		/// <seealso cref="LayoutAnchorable"/>
		/// <seealso cref="LayoutAnchorablePaneGroup"/>
		internal IEnumerable<LayoutAnchorable> GetLayoutAnchorable(LayoutAnchorablePaneGroup layoutAnchPaneGroup)
		{
			if (layoutAnchPaneGroup == null) yield break;
			foreach (var anchorable in layoutAnchPaneGroup.Descendents().OfType<LayoutAnchorable>())
				yield return anchorable;
		}

		#endregion IOverlayWindowHost Interface

		#region Public Methods

		/// <summary>Return the LayoutItem wrapper for the content passed as argument.</summary>
		/// <param name="content">LayoutContent to search</param>
		/// <returns>Either a <see cref="LayoutAnchorableItem"/> or <see cref="LayoutDocumentItem"/> which contains the <see cref="LayoutContent"/> passed as argument.</returns>
		public LayoutItem GetLayoutItemFromModel(LayoutContent content)
		{
			return _layoutItems.FirstOrDefault(item => item.LayoutElement == content);
		}

		public LayoutFloatingWindowControl CreateFloatingWindow(LayoutContent contentModel, bool isContentImmutable)
		{
			if (contentModel is LayoutAnchorable anchorable)
			{
				if (!(contentModel.Parent is ILayoutPane))
				{
					var pane = new LayoutAnchorablePane(anchorable)
					{
						FloatingTop = contentModel.FloatingTop,
						FloatingLeft = contentModel.FloatingLeft,
						FloatingWidth = contentModel.FloatingWidth,
						FloatingHeight = contentModel.FloatingHeight
					};
					return CreateFloatingWindowForLayoutAnchorableWithoutParent(pane, isContentImmutable);
				}
			}

			return CreateFloatingWindowCore(contentModel, isContentImmutable);
		}

		#endregion Public Methods

		#region Internal Methods

		/// <summary>
		/// Method is invoked to create the actual visible UI element from a given layout model. It is invoked when:
		///
		/// 1. New UI items are created and layed out or
		/// 2. Layout is deserialized, and the previous UI items are restored to the screen
		///    (using the model who's information was serialized to XML).
		/// </summary>
		/// <param name="model">The layout element.</param>
		/// <returns></returns>
		internal UIElement CreateUIElementForModel(ILayoutElement model)
		{
			if (model is LayoutPanel)
				return new LayoutPanelControl(model as LayoutPanel);
			if (model is LayoutAnchorablePaneGroup)
				return new LayoutAnchorablePaneGroupControl(model as LayoutAnchorablePaneGroup);
			if (model is LayoutDocumentPaneGroup)
				return new LayoutDocumentPaneGroupControl(model as LayoutDocumentPaneGroup);

			if (model is LayoutAnchorSide)
			{
				var templateModelView = new LayoutAnchorSideControl(model as LayoutAnchorSide);
				templateModelView.SetBinding(TemplateProperty, new Binding(AnchorSideTemplateProperty.Name) { Source = this });
				return templateModelView;
			}
			if (model is LayoutAnchorGroup)
			{
				var templateModelView = new LayoutAnchorGroupControl(model as LayoutAnchorGroup);
				templateModelView.SetBinding(TemplateProperty, new Binding(AnchorGroupTemplateProperty.Name) { Source = this });
				return templateModelView;
			}

			if (model is LayoutDocumentPane)
			{
				var templateModelView = new LayoutDocumentPaneControl(model as LayoutDocumentPane, IsVirtualizingDocument);
				templateModelView.SetBinding(StyleProperty, new Binding(DocumentPaneControlStyleProperty.Name) { Source = this });
				return templateModelView;
			}
			if (model is LayoutAnchorablePane)
			{
				var templateModelView = new LayoutAnchorablePaneControl(model as LayoutAnchorablePane, IsVirtualizingAnchorable);
				templateModelView.SetBinding(StyleProperty, new Binding(AnchorablePaneControlStyleProperty.Name) { Source = this });
				return templateModelView;
			}

			if (model is LayoutAnchorableFloatingWindow)
			{
				if (DesignerProperties.GetIsInDesignMode(this)) return null;
				var modelFW = model as LayoutAnchorableFloatingWindow;
				var newFW = new LayoutAnchorableFloatingWindowControl(modelFW)
				{
					//Owner = Window.GetWindow(this)
				};
				
				newFW.UpdateOwnership();

				// Fill list before calling Show (issue #254)
				_fwList.Add(newFW);

				// Floating Window can also contain only Pane Groups at its base (issue #27) so we check for
				// RootPanel (which is a LayoutAnchorablePaneGroup) and make sure the window is positioned back
				// in current (or nearest) monitor
				var panegroup = modelFW.RootPanel;
				if (panegroup != null)
				{
					panegroup.KeepInsideNearestMonitor();  // Check position is valid in current setup

					newFW.Left = panegroup.FloatingLeft;   // Position the window to previous or nearest valid position
					newFW.Top = panegroup.FloatingTop;
					newFW.Width = panegroup.FloatingWidth;
					newFW.Height = panegroup.FloatingHeight;
				}

				newFW.ShowInTaskbar = false;

				Dispatcher.BeginInvoke(new Action(() =>
				{
					if (newFW.Content != null || (newFW.Model as LayoutAnchorableFloatingWindow)?.IsVisible == true)
						newFW.Show();
					else
						newFW.Hide();
				}), DispatcherPriority.Send);

				if (panegroup != null && panegroup.IsMaximized)
					newFW.WindowState = WindowState.Maximized;
				return newFW;
			}

			if (model is LayoutDocumentFloatingWindow)
			{
				if (DesignerProperties.GetIsInDesignMode(this))
					return null;
				var modelFW = model as LayoutDocumentFloatingWindow;
				var newFW = new LayoutDocumentFloatingWindowControl(modelFW)
				{
					//Owner = Window.GetWindow(this)
				};
				
				newFW.UpdateOwnership();

				// Fill list before calling Show (issue #254)
				_fwList.Add(newFW);

				var paneForExtensions = modelFW.RootPanel;
				if (paneForExtensions != null)
				{
					//ensure that floating window position is inside current (or nearest) monitor
					paneForExtensions.KeepInsideNearestMonitor();

					newFW.Left = paneForExtensions.FloatingLeft;
					newFW.Top = paneForExtensions.FloatingTop;
					newFW.Width = paneForExtensions.FloatingWidth;
					newFW.Height = paneForExtensions.FloatingHeight;
				}
				newFW.ShowInTaskbar = false;
				newFW.Show();
				// Do not set the WindowState before showing or it will be lost
				if (paneForExtensions != null && paneForExtensions.IsMaximized)
					newFW.WindowState = WindowState.Maximized;
				return newFW;
			}
			if (model is LayoutDocument layoutDocument)
			{
				var templateModelView = new LayoutDocumentControl { Model = layoutDocument };
				return templateModelView;
			}
			return null;
		}

		/// <summary>Method is invoked to pop put an Anchorable that was in AutoHide mode.</summary>
		/// <param name="anchor"><see cref="LayoutAnchorControl"/> to pop out of the side panel.</param>
		internal void ShowAutoHideWindow(LayoutAnchorControl anchor)
		{
			_autoHideWindowManager.ShowAutoHideWindow(anchor);
		}

		internal void HideAutoHideWindow(LayoutAnchorControl anchor) => _autoHideWindowManager.HideAutoWindow(anchor);

		internal FrameworkElement GetAutoHideAreaElement() => _autohideArea;

		/// <summary>
		/// Executes when the user starts to drag a <see cref="LayoutDocument"/> or
		/// <see cref="LayoutAnchorable"/> by dragging its TabItem Header.
		/// </summary>
		/// <param name="contentModel"></param>
		/// <param name="startDrag"></param>
		internal void StartDraggingFloatingWindowForContent(LayoutContent contentModel, bool startDrag = true)
		{
			// Ensure window can float only if corresponding property is set accordingly
			if (contentModel == null) return;
			if (!contentModel.CanFloat) return;
			LayoutFloatingWindowControl fwc = null;

			// For last document re-use floating window
			if (contentModel.Parent.ChildrenCount == 1)
			{
				foreach (var fw in _fwList)
				{
					var found = fw.Model.Descendents().OfType<LayoutDocument>().Any(doc => doc == contentModel);
					if (!found) continue;
					if (fw.Model.Descendents().OfType<LayoutDocument>().Count() + fw.Model.Descendents().OfType<LayoutAnchorable>().Count() == 1)
						fwc = fw;
					break;
				}
			}

			var show = fwc == null; // Do not show already visible floating window
			if (fwc == null)
				fwc = CreateFloatingWindow(contentModel, false);

			if (fwc != null)
			{
				Dispatcher.BeginInvoke(new Action(() =>
				{
					// Activate only inactive document
					if (startDrag) fwc.AttachDrag();
					fwc.Show();
				}), DispatcherPriority.Send);
			}
		}

		/// <summary>
		/// Executes when the user starts to drag a docked <see cref="LayoutAnchorable"/> (tool window)
		/// by dragging its title bar (top header of a tool window).
		/// </summary>
		/// <param name="paneModel"></param>
		internal void StartDraggingFloatingWindowForPane(LayoutAnchorablePane paneModel)
		{
			var fwc = CreateFloatingWindowForLayoutAnchorableWithoutParent(paneModel, false);
			if (fwc == null) return;
			fwc.AttachDrag();
			fwc.Show();
		}

		internal IEnumerable<LayoutFloatingWindowControl> GetFloatingWindowsByZOrder()
		{
			var parentWindow = Window.GetWindow(this);
			var windowParentHandle = parentWindow != null ? new WindowInteropHelper(parentWindow).Handle : Process.GetCurrentProcess().MainWindowHandle;
			var currentHandle = Win32Helper.GetWindow(windowParentHandle, (uint)Win32Helper.GetWindow_Cmd.GW_HWNDFIRST);
			while (currentHandle != IntPtr.Zero)
			{
				var ctrl = _fwList.FirstOrDefault(fw => new WindowInteropHelper(fw).Handle == currentHandle);
				if (ctrl != null && ctrl.Model.Root != null && ctrl.Model.Root.Manager == this)
					yield return ctrl;

				currentHandle = Win32Helper.GetWindow(currentHandle, (uint)Win32Helper.GetWindow_Cmd.GW_HWNDNEXT);
			}
		}

		internal void RemoveFloatingWindow(LayoutFloatingWindowControl floatingWindow) => _fwList.Remove(floatingWindow);

		internal void ExecuteCloseCommand(LayoutDocument document)
		{
			if (DocumentClosing != null)
			{
				var argsClosing = new DocumentClosingEventArgs(document);
				DocumentClosing(this, argsClosing);
				if (argsClosing.Cancel) return;
			}

			//
			// Determine the index of the document that will be removed.
			//
			int indexOfDocumentToRemove = GetIndexOfDocument(document);

			if (!document.CloseDocument()) return;

			RemoveViewFromLogicalChild(document);
			if (document.Content is UIElement uIElement)
				RemoveLogicalChild(uIElement);
			DocumentClosed?.Invoke(this, new DocumentClosedEventArgs(document));

			//get rid of the closed document content
			document.Content = null;

			int indexOfDocumentToSelect = indexOfDocumentToRemove - 1;

			if (indexOfDocumentToSelect < 0)
			{
				indexOfDocumentToSelect = 0;
			}

			//
			// Determine the new active document and activate it.
			// This doesn't only update the layout, but also all related (dependency) properties.
			//
			LayoutDocument layoutDocument = GetDocumentOnIndex(indexOfDocumentToSelect);

			if (layoutDocument != null)
			{
				layoutDocument.IsActive = true;
			}
		}

		private LayoutDocument GetDocumentOnIndex(int indexToFind)
		{
			if (indexToFind < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(indexToFind));
			}

			int index = 0;

			foreach (LayoutDocument layoutDocument in this.Layout.Descendents().OfType<LayoutDocument>())
			{
				if (index == indexToFind)
				{
					return layoutDocument;
				}

				index++;
			}

			return null;
		}

		private int GetIndexOfDocument(LayoutDocument documentToFind)
		{
			if (documentToFind == null)
			{
				throw new ArgumentNullException(nameof(documentToFind));
			}

			int index = 0;

			foreach (LayoutDocument layoutDocument in this.Layout.Descendents().OfType<LayoutDocument>())
			{
				if (layoutDocument == documentToFind)
				{
					return index;
				}

				index++;
			}

			//
			// Not found.
			//
			return -1;
		}

		internal void ExecuteCloseAllButThisCommand(LayoutContent contentSelected)
		{
			foreach (var contentToClose in Layout.Descendents().OfType<LayoutContent>().Where(d => d != contentSelected && (d.Parent is LayoutDocumentPane || d.Parent is LayoutDocumentFloatingWindow)).ToArray())
				Close(contentToClose);
		}

		internal void ExecuteCloseAllCommand(LayoutContent contentSelected)
		{
			foreach (var contentToClose in Layout.Descendents().OfType<LayoutContent>().Where(d => (d.Parent is LayoutDocumentPane || d.Parent is LayoutDocumentFloatingWindow)).ToArray())
				Close(contentToClose);
		}

		internal void ExecuteCloseCommand(LayoutAnchorable anchorable)
		{
			if (!(anchorable is LayoutAnchorable model)) return;
			model.CloseAnchorable();
			RemoveViewFromLogicalChild(anchorable);
		}

		internal void ExecuteHideCommand(LayoutAnchorable anchorable) => anchorable?.Hide();

		internal void ExecuteAutoHideCommand(LayoutAnchorable _anchorable) => _anchorable.ToggleAutoHide();

		/// <summary>
		/// Method executes when the user clicks the Float button in the context menu of an <see cref="LayoutAnchorable"/>.
		///
		/// This removes the content from the docked <see cref="LayoutAnchorable"/> and inserts it into a
		/// draggable <see cref="LayoutFloatingWindowControl"/>.
		/// </summary>
		/// <param name="contentToFloat"></param>
		internal void ExecuteFloatCommand(LayoutContent contentToFloat) => contentToFloat.Float();

		internal void ExecuteDockCommand(LayoutAnchorable anchorable) => anchorable.Dock();

		internal void ExecuteDockAsDocumentCommand(LayoutContent content) => content.DockAsDocument();

		internal void ExecuteContentActivateCommand(LayoutContent content) => content.IsActive = true;

		#endregion Internal Methods

		#region Overrides

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
			_autohideArea = GetTemplateChild("PART_AutoHideArea") as FrameworkElement;
		}

		protected override Size ArrangeOverride(Size arrangeBounds)
		{
			_areas = null;
			return base.ArrangeOverride(arrangeBounds);
		}

		protected override void OnPreviewKeyDown(KeyEventArgs e)
		{
			if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
			{
				if (e.IsDown && e.Key == Key.Tab)
				{
					if (CanShowNavigatorWindow && !IsNavigatorWindowActive)
					{
						ShowNavigatorWindow();
						e.Handled = true;
					}
				}
			}

			base.OnPreviewKeyDown(e);
		}

		#endregion Overrides

		#region Private Methods

		/// <summary>
		/// OnContextMenuPropertyChanged(Fix ContextMenu's Resources is null,drop down menu style error)
		/// </summary>
		/// <param name="d"></param>
		/// <param name="e"></param>
		private static void OnContextMenuPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (e.NewValue is ContextMenu contextMenu)
			{
				contextMenu.Resources = ((DockingManager)d).Resources;
			}
		}

		private void OnLayoutRootPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
				case nameof(LayoutRoot.RootPanel):
					{
						if (IsInitialized)
						{
							var layoutRootPanel = CreateUIElementForModel(Layout.RootPanel) as LayoutPanelControl;
							LayoutRootPanel = layoutRootPanel;
						}
						break;
					}
				case nameof(LayoutRoot.ActiveContent):
					{
						//set focus on active element only after a layout pass is completed
						//it's possible that it is not yet visible in the visual tree
						//if (_setFocusAsyncOperation == null)
						//{
						//    _setFocusAsyncOperation = Dispatcher.BeginInvoke(new Action(() =>
						// {
						if (Layout.ActiveContent != null)
							FocusElementManager.SetFocusOnLastElement(Layout.ActiveContent);
						//_setFocusAsyncOperation = null;
						//  } ), DispatcherPriority.Input );
						//}

						if (!_insideInternalSetActiveContent)
							ActiveContent = Layout.ActiveContent?.Content;
						break;
					}
			}
		}

		private static void OnLayoutRootUpdated(object sender, EventArgs e) => CommandManager.InvalidateRequerySuggested();

		private void OnLayoutChanging(LayoutRoot newLayout) => LayoutChanging?.Invoke(this, EventArgs.Empty);

		private void DockingManager_Loaded(object sender, RoutedEventArgs e)
		{
			if (DesignerProperties.GetIsInDesignMode(this)) return;
			if (Layout.Manager == this)
			{
				LayoutRootPanel = CreateUIElementForModel(Layout.RootPanel) as LayoutPanelControl;
				LeftSidePanel = CreateUIElementForModel(Layout.LeftSide) as LayoutAnchorSideControl;
				TopSidePanel = CreateUIElementForModel(Layout.TopSide) as LayoutAnchorSideControl;
				RightSidePanel = CreateUIElementForModel(Layout.RightSide) as LayoutAnchorSideControl;
				BottomSidePanel = CreateUIElementForModel(Layout.BottomSide) as LayoutAnchorSideControl;

				// In order to prevent resource leaks, unsubscribe from SizeChanged event for case when we have no stored Layout settings.
				SizeChanged -= OnSizeChanged;
				SizeChanged += OnSizeChanged;
			}

			SetupAutoHideWindow();

			foreach (var fwc in _fwHiddenList)
			{
				fwc.EnableBindings();
				if (fwc.KeepContentVisibleOnClose)
				{
					fwc.Show();
					fwc.KeepContentVisibleOnClose = false;
				}

				_fwList.Add(fwc);
			}
			_fwHiddenList.Clear();

			// load floating windows not already loaded! (issue #59 & #254)
			var items = new List<LayoutFloatingWindow>(Layout.FloatingWindows.Where(fw => !_fwList.Any(fwc => fwc.Model == fw)));
			foreach (var fw in items)
				CreateUIElementForModel(fw);

			//create the overlaywindow if it's possible
			if (IsVisible)
				CreateOverlayWindow();
			FocusElementManager.SetupFocusManagement(this);
		}

		/// <summary>Method executes when the <see cref="DockingManager"/> control has changed its height and/or width.</summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnSizeChanged(object sender, SizeChangedEventArgs e)
		{
			// Lets make sure this always remains non-negative to avoid crach in layout system
			var width = Math.Max(ActualWidth - GridSplitterWidth - RightSidePanel.ActualWidth - LeftSidePanel.ActualWidth, 0);
			var height = Math.Max(ActualHeight - GridSplitterHeight - TopSidePanel.ActualHeight - BottomSidePanel.ActualHeight, 0);

			LayoutRootPanel.AdjustFixedChildrenPanelSizes(new Size(width, height));
		}

		private void DockingManager_Unloaded(object sender, RoutedEventArgs e)
		{
			SizeChanged -= OnSizeChanged;

			if (DesignerProperties.GetIsInDesignMode(this)) return;
			_autoHideWindowManager?.HideAutoWindow();

			AutoHideWindow?.Dispose();

			foreach (var fw in _fwList.ToArray())
			{
				////fw.Owner = null;
				//fw.SetParentWindowToNull();
				//fw.KeepContentVisibleOnClose = true;
				//// To avoid calling Close method multiple times.
				//fw.InternalClose(true);

				// Unloaded can occure not only after closing of the application, but after switching between tabs.
				// For such case it's better to hide the floating windows instead of closing it.
				// We clear bindings on visibility during the owner is unloaded.
				if (fw.IsVisible)
				{
					fw.KeepContentVisibleOnClose = true;
					fw.Hide();
				}
				fw.DisableBindings();
				_fwHiddenList.Add(fw);
			}

			_fwList.Clear();

			DestroyOverlayWindow();
			FocusElementManager.FinalizeFocusManagement(this);
		}

		private void SetupAutoHideWindow()
		{
			if (_autoHideWindowManager != null)
				_autoHideWindowManager.HideAutoWindow();
			else
				_autoHideWindowManager = new AutoHideWindowManager(this);

			AutoHideWindow?.Dispose();
			SetAutoHideWindow(new LayoutAutoHideWindowControl());
		}

		private void CreateOverlayWindow()
		{
			if (_overlayWindow == null)
			{
				_overlayWindow = new OverlayWindow(this);
			}
			_overlayWindow.Owner = Window.GetWindow(this);

			var rectWindow = new Rect(this.PointToScreenDPIWithoutFlowDirection(new Point()), this.TransformActualSizeToAncestor());
			_overlayWindow.Left = rectWindow.Left;
			_overlayWindow.Top = rectWindow.Top;
			_overlayWindow.Width = rectWindow.Width;
			_overlayWindow.Height = rectWindow.Height;
		}

		private void DestroyOverlayWindow()
		{
			if (_overlayWindow == null) return;
			_overlayWindow.Close();
			_overlayWindow = null;
		}

		private void AttachDocumentsSource(LayoutRoot layout, IEnumerable documentsSource)
		{
			if (documentsSource == null) return;
			if (layout == null) return;

			//if (layout.Descendents().OfType<LayoutDocument>().Any())
			//    throw new InvalidOperationException("Unable to set the DocumentsSource property if LayoutDocument objects are already present in the model");
			var documentsImported = layout.Descendents().OfType<LayoutDocument>().Select(d => d.Content).ToArray();
			var documents = documentsSource as IEnumerable;
			var listOfDocumentsToImport = new List<object>(documents.OfType<object>());

			foreach (var document in listOfDocumentsToImport.ToArray())
			{
				if (documentsImported.Contains(document))
					listOfDocumentsToImport.Remove(document);
			}

			LayoutDocumentPane documentPane = null;
			if (layout.LastFocusedDocument != null)
				documentPane = layout.LastFocusedDocument.Parent as LayoutDocumentPane;

			if (documentPane == null)
				documentPane = layout.Descendents().OfType<LayoutDocumentPane>().FirstOrDefault();

			//if (documentPane == null)
			//    throw new InvalidOperationException("Layout must contains at least one LayoutDocumentPane in order to host documents");

			_suspendLayoutItemCreation = true;
			foreach (var documentContentToImport in listOfDocumentsToImport)
			{
				//documentPane.Children.Add(new LayoutDocument() { Content = documentToImport });
				var documentToImport = new LayoutDocument { Content = documentContentToImport };

				var added = false;
				if (LayoutUpdateStrategy != null)
					added = LayoutUpdateStrategy.BeforeInsertDocument(layout, documentToImport, documentPane);

				if (!added)
				{
					if (documentPane == null)
						throw new InvalidOperationException("Layout must contains at least one LayoutDocumentPane in order to host documents");

					documentPane.Children.Add(documentToImport);
				}

				LayoutUpdateStrategy?.AfterInsertDocument(layout, documentToImport);
				CreateDocumentLayoutItem(documentToImport);
			}
			_suspendLayoutItemCreation = false;
			if (documentsSource is INotifyCollectionChanged documentsSourceAsNotifier)
				documentsSourceAsNotifier.CollectionChanged += DocumentsSourceElementsChanged;
		}

		private void DocumentsSourceElementsChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (Layout == null) return;
			//When deserializing documents are created automatically by the deserializer
			if (SuspendDocumentsSourceBinding) return;

			//handle remove
			if (e.Action == NotifyCollectionChangedAction.Remove ||
				e.Action == NotifyCollectionChangedAction.Replace)
			{
				if (e.OldItems != null)
				{
					var documentsToRemove = Layout.Descendents().OfType<LayoutDocument>().Where(d => e.OldItems.Contains(d.Content)).ToArray();
					foreach (var documentToRemove in documentsToRemove)
					{
						documentToRemove.Parent.RemoveChild(documentToRemove);
						RemoveViewFromLogicalChild(documentToRemove);
					}
				}
			}

			//handle add
			if (e.NewItems != null && (e.Action == NotifyCollectionChangedAction.Add || e.Action == NotifyCollectionChangedAction.Replace))
			{
				if (e.NewItems != null)
				{
					LayoutDocumentPane documentPane = null;
					if (Layout.LastFocusedDocument != null)
					{
						documentPane = Layout.LastFocusedDocument.Parent as LayoutDocumentPane;
					}
					if (documentPane == null)
					{
						documentPane = Layout.Descendents().OfType<LayoutDocumentPane>().FirstOrDefault();
					}
					//if (documentPane == null)
					//    throw new InvalidOperationException("Layout must contains at least one LayoutDocumentPane in order to host documents");
					_suspendLayoutItemCreation = true;
					foreach (var documentContentToImport in e.NewItems)
					{
						var documentToImport = new LayoutDocument
						{
							Content = documentContentToImport
						};

						var added = false;
						if (LayoutUpdateStrategy != null)
						{
							added = LayoutUpdateStrategy.BeforeInsertDocument(Layout, documentToImport, documentPane);
						}

						if (!added)
						{
							if (documentPane == null)
								throw new InvalidOperationException("Layout must contains at least one LayoutDocumentPane in order to host documents");

							documentPane.Children.Add(documentToImport);
							added = true;
						}

						LayoutUpdateStrategy?.AfterInsertDocument(Layout, documentToImport);
						var root = documentToImport.Root;
						if (root != null && root.Manager == this)
						{
							CreateDocumentLayoutItem(documentToImport);
						}
					}
					_suspendLayoutItemCreation = false;
				}
			}

			if (e.Action == NotifyCollectionChangedAction.Reset)
			{
				//Remove documents that are no longer in the DocumentSource.
				var documentsToRemove = GetItemsToRemoveAfterReset<LayoutDocument>(DocumentsSource);
				foreach (var documentToRemove in documentsToRemove)
				{
					(documentToRemove.Parent as ILayoutContainer).RemoveChild(
						documentToRemove);
					RemoveViewFromLogicalChild(documentToRemove);
				}
			}

			Layout?.CollectGarbage();
		}

		/// <summary>
		/// Iterate through source to find only the items that need to be removed.
		/// </summary>
		/// <remarks>
		/// Previous implementations cleared all documents from the layout. The current
		/// guidance is that the collection has changed signficantly, so only remove the
		/// documents that are no longer in the DocumentSource.
		/// </remarks>
		/// <typeparam name="TLayoutType"></typeparam>
		/// <param name="source">Either DocumentsSource or AnchorablesSource</param>
		/// <returns></returns>
		private TLayoutType[] GetItemsToRemoveAfterReset<TLayoutType>(IEnumerable source)
			where TLayoutType : LayoutContent
		{
			//Find remaining items in source
			var itemsThatRemain = new HashSet<object>(source.Cast<object>(), ReferenceEqualityComparer.Default);
			//Find the removed items that are not in the remaining collection
			return Layout.Descendents()
						 .OfType<TLayoutType>()
						 .Where(x => !itemsThatRemain.Contains(x.Content))
						 .ToArray();
		}

		private void DetachDocumentsSource(LayoutRoot layout, IEnumerable documentsSource)
		{
			if (documentsSource == null) return;
			if (layout == null) return;

			var documentsToRemove = layout.Descendents().OfType<LayoutDocument>()
				.Where(d => documentsSource.Contains(d.Content)).ToArray();

			foreach (var documentToRemove in documentsToRemove)
			{
				(documentToRemove.Parent as ILayoutContainer).RemoveChild(
					documentToRemove);
				RemoveViewFromLogicalChild(documentToRemove);
			}

			if (documentsSource is INotifyCollectionChanged documentsSourceAsNotifier)
				documentsSourceAsNotifier.CollectionChanged -= DocumentsSourceElementsChanged;
		}

		private void Close(LayoutContent contentToClose)
		{
			if (!contentToClose.CanClose) return;

			var layoutItem = GetLayoutItemFromModel(contentToClose);
			if (layoutItem.CloseCommand != null)
			{
				if (layoutItem.CloseCommand.CanExecute(null))
					layoutItem.CloseCommand.Execute(null);
			}
			else
			{
				if (contentToClose is LayoutDocument document)
					ExecuteCloseCommand(document);
				else if (contentToClose is LayoutAnchorable anchorable)
					ExecuteCloseCommand(anchorable);
			}
		}

		private void AttachAnchorablesSource(LayoutRoot layout, IEnumerable anchorablesSource)
		{
			if (anchorablesSource == null) return;
			if (layout == null) return;

			//if (layout.Descendents().OfType<LayoutAnchorable>().Any())
			//    throw new InvalidOperationException("Unable to set the AnchorablesSource property if LayoutAnchorable objects are already present in the model");
			var anchorablesImported = layout.Descendents().OfType<LayoutAnchorable>().Select(d => d.Content).ToArray();
			var anchorables = anchorablesSource as IEnumerable;
			var listOfAnchorablesToImport = new List<object>(anchorables.OfType<object>());

			foreach (var document in listOfAnchorablesToImport.ToArray())
			{
				if (anchorablesImported.Contains(document))
					listOfAnchorablesToImport.Remove(document);
			}
			LayoutAnchorablePane anchorablePane = null;
			if (layout.ActiveContent != null)
			{
				//look for active content parent pane
				anchorablePane = layout.ActiveContent.Parent as LayoutAnchorablePane;
			}
			if (anchorablePane == null)
			{
				//look for a pane on the right side
				anchorablePane = layout.Descendents().OfType<LayoutAnchorablePane>().FirstOrDefault(pane => !pane.IsHostedInFloatingWindow && pane.GetSide() == AnchorSide.Right);
			}
			if (anchorablePane == null)
			{
				//look for an available pane
				anchorablePane = layout.Descendents().OfType<LayoutAnchorablePane>().FirstOrDefault();
			}
			_suspendLayoutItemCreation = true;
			foreach (var anchorableContentToImport in listOfAnchorablesToImport)
			{
				var anchorableToImport = new LayoutAnchorable { Content = anchorableContentToImport };
				var added = false;
				if (LayoutUpdateStrategy != null)
					added = LayoutUpdateStrategy.BeforeInsertAnchorable(layout, anchorableToImport, anchorablePane);

				if (!added)
				{
					if (anchorablePane == null)
					{
						var mainLayoutPanel = new LayoutPanel { Orientation = Orientation.Horizontal };
						if (layout.RootPanel != null)
						{
							mainLayoutPanel.Children.Add(layout.RootPanel);
						}

						layout.RootPanel = mainLayoutPanel;
						anchorablePane = new LayoutAnchorablePane { DockWidth = new GridLength(200.0, GridUnitType.Pixel) };
						mainLayoutPanel.Children.Add(anchorablePane);
					}

					anchorablePane.Children.Add(anchorableToImport);
					added = true;
				}

				LayoutUpdateStrategy?.AfterInsertAnchorable(layout, anchorableToImport);
				CreateAnchorableLayoutItem(anchorableToImport);
			}
			_suspendLayoutItemCreation = false;
			if (anchorablesSource is INotifyCollectionChanged anchorablesSourceAsNotifier)
				anchorablesSourceAsNotifier.CollectionChanged += AnchorablesSourceElementsChanged;
		}

		private void AnchorablesSourceElementsChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (Layout == null) return;

			//When deserializing documents are created automatically by the deserializer
			if (SuspendAnchorablesSourceBinding) return;

			//handle remove
			if (e.Action == NotifyCollectionChangedAction.Remove || e.Action == NotifyCollectionChangedAction.Replace)
			{
				if (e.OldItems != null)
				{
					var anchorablesToRemove = Layout.Descendents().OfType<LayoutAnchorable>().Where(d => e.OldItems.Contains(d.Content)).ToArray();
					foreach (var anchorableToRemove in anchorablesToRemove)
					{
						anchorableToRemove.Content = null;
						anchorableToRemove.Parent.RemoveChild(anchorableToRemove);
						RemoveViewFromLogicalChild(anchorableToRemove);
					}
				}
			}

			//handle add
			if (e.NewItems != null && (e.Action == NotifyCollectionChangedAction.Add || e.Action == NotifyCollectionChangedAction.Replace))
			{
				if (e.NewItems != null)
				{
					LayoutAnchorablePane anchorablePane = null;
					if (Layout.ActiveContent != null)
					{
						//look for active content parent pane
						anchorablePane = Layout.ActiveContent.Parent as LayoutAnchorablePane;
					}
					if (anchorablePane == null)
					{
						//look for a pane on the right side
						anchorablePane = Layout.Descendents().OfType<LayoutAnchorablePane>().FirstOrDefault(pane => !pane.IsHostedInFloatingWindow && pane.GetSide() == AnchorSide.Right);
					}
					if (anchorablePane == null)
					{
						//look for an available pane
						anchorablePane = Layout.Descendents().OfType<LayoutAnchorablePane>().FirstOrDefault();
					}
					_suspendLayoutItemCreation = true;
					foreach (var anchorableContentToImport in e.NewItems)
					{
						var anchorableToImport = new LayoutAnchorable { Content = anchorableContentToImport };
						var added = false;
						if (LayoutUpdateStrategy != null)
							added = LayoutUpdateStrategy.BeforeInsertAnchorable(Layout, anchorableToImport, anchorablePane);
						if (!added)
						{
							if (anchorablePane == null)
							{
								var mainLayoutPanel = new LayoutPanel { Orientation = Orientation.Horizontal };
								if (Layout.RootPanel != null)
								{
									mainLayoutPanel.Children.Add(Layout.RootPanel);
								}
								Layout.RootPanel = mainLayoutPanel;
								anchorablePane = new LayoutAnchorablePane { DockWidth = new GridLength(200.0, GridUnitType.Pixel) };
								mainLayoutPanel.Children.Add(anchorablePane);
							}
							anchorablePane.Children.Add(anchorableToImport);
							added = true;
						}
						LayoutUpdateStrategy?.AfterInsertAnchorable(Layout, anchorableToImport);
						var root = anchorableToImport.Root;
						if (root != null && root.Manager == this)
							CreateAnchorableLayoutItem(anchorableToImport);
					}
					_suspendLayoutItemCreation = false;
				}
			}

			if (e.Action == NotifyCollectionChangedAction.Reset)
			{
				//Remove anchorables that are no longer in the AnchorablesSource.
				var anchorablesToRemove = GetItemsToRemoveAfterReset<LayoutAnchorable>(AnchorablesSource);
				foreach (var anchorableToRemove in anchorablesToRemove)
				{
					(anchorableToRemove.Parent as ILayoutContainer).RemoveChild(
						anchorableToRemove);
					RemoveViewFromLogicalChild(anchorableToRemove);
				}
			}

			Layout?.CollectGarbage();
		}

		private void DetachAnchorablesSource(LayoutRoot layout, IEnumerable anchorablesSource)
		{
			if (anchorablesSource == null) return;
			if (layout == null) return;

			var anchorablesToRemove = layout.Descendents().OfType<LayoutAnchorable>()
				.Where(d => anchorablesSource.Contains(d.Content)).ToArray();

			foreach (var anchorableToRemove in anchorablesToRemove)
			{
				anchorableToRemove.Parent.RemoveChild(anchorableToRemove);
				RemoveViewFromLogicalChild(anchorableToRemove);
			}

			if (anchorablesSource is INotifyCollectionChanged anchorablesSourceAsNotifier)
				anchorablesSourceAsNotifier.CollectionChanged -= AnchorablesSourceElementsChanged;
		}

		private void RemoveViewFromLogicalChild(LayoutContent layoutContent)
		{
			if (layoutContent == null) return;
			var layoutItem = GetLayoutItemFromModel(layoutContent);
			if (layoutItem == null) return;
			if (layoutItem.IsViewExists()) InternalRemoveLogicalChild(layoutItem.View);
		}

		private void InternalSetActiveContent(object contentObject)
		{
			// BugFix for first issue in #59
			var list = Layout.Descendents().OfType<LayoutContent>().ToList();
			var layoutContent = list.FirstOrDefault(lc => lc == contentObject || lc.Content == contentObject);
			_insideInternalSetActiveContent = true;
			Layout.ActiveContent = layoutContent;
			_insideInternalSetActiveContent = false;
		}

		#region LayoutItems

		/// <summary>
		/// Implements the EventHandler for the <see cref="LayoutRoot.ElementRemoved"/> event.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Layout_ElementRemoved(object sender, LayoutElementEventArgs e)
		{
			if (_suspendLayoutItemCreation) return;
			CollectLayoutItemsDeleted();
		}

		/// <summary>
		/// Implements the EventHandler for the <see cref="LayoutRoot.ElementAdded"/> event.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Layout_ElementAdded(object sender, LayoutElementEventArgs e)
		{
			if (_suspendLayoutItemCreation) return;
			foreach (var content in Layout.Descendents().OfType<LayoutContent>().ToList())
			{
				if (content is LayoutDocument)
					CreateDocumentLayoutItem(content as LayoutDocument);
				else //if (content is LayoutAnchorable)
					CreateAnchorableLayoutItem(content as LayoutAnchorable);
			}
			CollectLayoutItemsDeleted();
		}

		/// <summary>
		/// Detach and remove all LayoutItems that are no longer part of the <see cref="LayoutRoot"/>.
		/// </summary>
		private void CollectLayoutItemsDeleted()
		{
			if (_collectLayoutItemsOperations != null) return;
			_collectLayoutItemsOperations = Dispatcher.BeginInvoke(new Action(() =>
			{
				_collectLayoutItemsOperations = null;
				foreach (var itemToRemove in _layoutItems.Where(item => item.LayoutElement.Root != Layout).ToArray())
				{
					itemToRemove.Detach();
					_layoutItems.Remove(itemToRemove);
				}
			}));
		}

		/// <summary>
		/// Detaches all LayoutItems from their content and clears the private collection of LayoutItems.
		/// </summary>
		private void DetachLayoutItems()
		{
			if (Layout == null) return;
			_layoutItems.ForEach<LayoutItem>(i => i.Detach());
			_layoutItems.Clear();
			Layout.ElementAdded -= Layout_ElementAdded;
			Layout.ElementRemoved -= Layout_ElementRemoved;
		}

		/// <summary>
		/// Attaches a:
		/// - <see cref="LayoutDocumentItem"/> to each <see cref="<see cref="LayoutDocumentItem"/> and
		/// - <see cref="LayoutAnchorableItem"/> to each <see cref="<see cref="LayoutAnchorable"/>
		///
		/// in the <see cref="LayoutRoot"/> property.
		/// </summary>
		private void AttachLayoutItems()
		{
			if (Layout == null) return;
			foreach (var document in Layout.Descendents().OfType<LayoutDocument>().ToArray())
				CreateDocumentLayoutItem(document);

			foreach (var anchorable in Layout.Descendents().OfType<LayoutAnchorable>().ToArray())
				CreateAnchorableLayoutItem(anchorable);

			Layout.ElementAdded += Layout_ElementAdded;
			Layout.ElementRemoved += Layout_ElementRemoved;
		}

		/// <summary>
		/// Applies the corresponding style to a <see cref="LayoutItem"/> either from:
		/// 1) <see cref="LayoutItemContainerStyle"/> property or
		/// 2) <see cref="LayoutItemContainerStyleSelector"/> property
		/// </summary>
		/// <param name="layoutItem"></param>
		private void ApplyStyleToLayoutItem(LayoutItem layoutItem)
		{
			layoutItem._ClearDefaultBindings();
			if (LayoutItemContainerStyle != null)
				layoutItem.Style = LayoutItemContainerStyle;
			else if (LayoutItemContainerStyleSelector != null)
				layoutItem.Style = LayoutItemContainerStyleSelector.SelectStyle(layoutItem.Model, layoutItem);
			layoutItem._SetDefaultBindings();
		}

		/// <summary>
		/// Creates a <see cref="LayoutAnchorableItem"/> for each <see cref="LayoutAnchorable"/>
		/// or returns the existing <see cref="LayoutAnchorableItem"/> if there is already one.
		/// </summary>
		/// <param name="contentToAttach"></param>
		private void CreateAnchorableLayoutItem(LayoutAnchorable contentToAttach)
		{
			if (_layoutItems.Any(item => item.LayoutElement == contentToAttach))
			{
				foreach (var item in _layoutItems) ApplyStyleToLayoutItem(item);
				return;
			}

			var layoutItem = new LayoutAnchorableItem();
			layoutItem.Attach(contentToAttach);
			_layoutItems.Add(layoutItem);
			ApplyStyleToLayoutItem(layoutItem);
			if (contentToAttach?.Content is UIElement) InternalAddLogicalChild(contentToAttach.Content);
		}

		/// <summary>
		/// Creates a <see cref="LayoutDocumentItem"/> for each <see cref="LayoutDocument"/>
		/// or returns the existing <see cref="LayoutDocument"/> if there is already one.
		/// </summary>
		/// <param name="contentToAttach"></param>
		private void CreateDocumentLayoutItem(LayoutDocument contentToAttach)
		{
			if (_layoutItems.Any(item => item.LayoutElement == contentToAttach))
			{
				foreach (var item in _layoutItems) ApplyStyleToLayoutItem(item);
				return;
			}

			var layoutItem = new LayoutDocumentItem();
			layoutItem.Attach(contentToAttach);
			_layoutItems.Add(layoutItem);
			ApplyStyleToLayoutItem(layoutItem);
			if (contentToAttach?.Content is UIElement) InternalAddLogicalChild(contentToAttach.Content);
		}

		#endregion LayoutItems

		private void ShowNavigatorWindow()
		{
			if (_navigatorWindow == null)
				_navigatorWindow = new NavigatorWindow(this) { Owner = Window.GetWindow(this), WindowStartupLocation = WindowStartupLocation.CenterOwner };
			_navigatorWindow.ShowDialog();
			_navigatorWindow = null;
		}

		private LayoutFloatingWindowControl CreateFloatingWindowForLayoutAnchorableWithoutParent(LayoutAnchorablePane paneModel, bool isContentImmutable)
		{
			if (paneModel.Children.Any(c => !c.CanFloat))
				return null;
			var paneAsPositionableElement = paneModel as ILayoutPositionableElement;
			var paneAsWithActualSize = paneModel as ILayoutPositionableElementWithActualSize;

			var fwWidth = paneAsPositionableElement.FloatingWidth;
			var fwHeight = paneAsPositionableElement.FloatingHeight;
			var fwLeft = paneAsPositionableElement.FloatingLeft;
			var fwTop = paneAsPositionableElement.FloatingTop;

			if (fwWidth == 0.0)
				fwWidth = paneAsWithActualSize.ActualWidth + 10;       //10 includes BorderThickness and Margins inside LayoutAnchorableFloatingWindowControl.
			if (fwHeight == 0.0)
				fwHeight = paneAsWithActualSize.ActualHeight + 10;   //10 includes BorderThickness and Margins inside LayoutAnchorableFloatingWindowControl.

			var destPane = new LayoutAnchorablePane
			{
				DockWidth = paneAsPositionableElement.DockWidth,
				DockHeight = paneAsPositionableElement.DockHeight,
				DockMinHeight = paneAsPositionableElement.DockMinHeight,
				DockMinWidth = paneAsPositionableElement.DockMinWidth,
				FloatingLeft = paneAsPositionableElement.FloatingLeft,
				FloatingTop = paneAsPositionableElement.FloatingTop,
				FloatingWidth = paneAsPositionableElement.FloatingWidth,
				FloatingHeight = paneAsPositionableElement.FloatingHeight,
			};

			var savePreviousContainer = paneModel.FindParent<LayoutFloatingWindow>() == null;
			var currentSelectedContentIndex = paneModel.SelectedContentIndex;
			while (paneModel.Children.Count > 0)
			{
				var contentModel = paneModel.Children[paneModel.Children.Count - 1];

				if (savePreviousContainer)
				{
					((ILayoutPreviousContainer)contentModel).PreviousContainer = paneModel;
					contentModel.PreviousContainerIndex = paneModel.Children.Count - 1;
				}

				paneModel.RemoveChildAt(paneModel.Children.Count - 1);
				destPane.Children.Insert(0, contentModel);
			}

			if (destPane.Children.Count > 0) destPane.SelectedContentIndex = currentSelectedContentIndex;
			LayoutFloatingWindow fw;
			LayoutFloatingWindowControl fwc;
			fw = new LayoutAnchorableFloatingWindow
			{
				RootPanel = new LayoutAnchorablePaneGroup(destPane)
				{
					DockHeight = destPane.DockHeight,
					DockWidth = destPane.DockWidth,
					DockMinHeight = destPane.DockMinHeight,
					DockMinWidth = destPane.DockMinWidth,
				}
			};

			Layout.FloatingWindows.Add(fw);

			fwc = new LayoutAnchorableFloatingWindowControl((LayoutAnchorableFloatingWindow)fw, isContentImmutable)
			{
				Width = fwWidth,
				Height = fwHeight,
				Top = fwTop,
				Left = fwLeft
			};
			//fwc.Owner = Window.GetWindow(this);
			//fwc.SetParentToMainWindowOf(this);
			_fwList.Add(fwc);
			Layout.CollectGarbage();
			InvalidateArrange();
			return fwc;
		}

		private LayoutFloatingWindowControl CreateFloatingWindowCore(LayoutContent contentModel, bool isContentImmutable)
		{
			if (!contentModel.CanFloat) return null;
			if (contentModel is LayoutAnchorable contentModelAsAnchorable && contentModelAsAnchorable.IsAutoHidden)
				contentModelAsAnchorable.ToggleAutoHide();

			var parentPane = contentModel.Parent as ILayoutPane;
			var parentPaneAsPositionableElement = contentModel.Parent as ILayoutPositionableElement;
			var parentPaneAsWithActualSize = contentModel.Parent as ILayoutPositionableElementWithActualSize;
			var contentModelParentChildrenIndex = parentPane.Children.ToList().IndexOf(contentModel);

			if (contentModel.FindParent<LayoutFloatingWindow>() == null)
			{
				((ILayoutPreviousContainer)contentModel).PreviousContainer = parentPane;
				contentModel.PreviousContainerIndex = contentModelParentChildrenIndex;
			}

			parentPane.RemoveChildAt(contentModelParentChildrenIndex);

			var fwWidth = contentModel.FloatingWidth;
			var fwHeight = contentModel.FloatingHeight;

			if (fwWidth == 0.0)
				fwWidth = parentPaneAsPositionableElement.FloatingWidth;
			if (fwHeight == 0.0)
				fwHeight = parentPaneAsPositionableElement.FloatingHeight;

			if (fwWidth == 0.0)
				fwWidth = parentPaneAsWithActualSize.ActualWidth + 10;      //10 includes BorderThickness and Margins inside LayoutDocumentFloatingWindowControl.
			if (fwHeight == 0.0)
				fwHeight = parentPaneAsWithActualSize.ActualHeight + 10;    //10 includes BorderThickness and Margins inside LayoutDocumentFloatingWindowControl.

			LayoutFloatingWindow fw;
			LayoutFloatingWindowControl fwc;
			if (contentModel is LayoutAnchorable)
			{
				var anchorableContent = contentModel as LayoutAnchorable;
				fw = new LayoutAnchorableFloatingWindow
				{
					RootPanel = new LayoutAnchorablePaneGroup(new LayoutAnchorablePane(anchorableContent)
					{
						DockWidth = parentPaneAsPositionableElement.DockWidth,
						DockHeight = parentPaneAsPositionableElement.DockHeight,
						DockMinHeight = parentPaneAsPositionableElement.DockMinHeight,
						DockMinWidth = parentPaneAsPositionableElement.DockMinWidth,
						FloatingLeft = parentPaneAsPositionableElement.FloatingLeft,
						FloatingTop = parentPaneAsPositionableElement.FloatingTop,
						FloatingWidth = parentPaneAsPositionableElement.FloatingWidth,
						FloatingHeight = parentPaneAsPositionableElement.FloatingHeight,
					})
				};

				Layout.FloatingWindows.Add(fw);
				fwc = new LayoutAnchorableFloatingWindowControl((LayoutAnchorableFloatingWindow)fw, isContentImmutable)
				{
					Width = fwWidth,
					Height = fwHeight,
					Left = contentModel.FloatingLeft,
					Top = contentModel.FloatingTop
				};
			}
			else
			{
				var anchorableDocument = contentModel as LayoutDocument;
				fw = new LayoutDocumentFloatingWindow
				{
					RootPanel = new LayoutDocumentPaneGroup(new LayoutDocumentPane(anchorableDocument)
					{
						DockWidth = parentPaneAsPositionableElement.DockWidth,
						DockHeight = parentPaneAsPositionableElement.DockHeight,
						DockMinHeight = parentPaneAsPositionableElement.DockMinHeight,
						DockMinWidth = parentPaneAsPositionableElement.DockMinWidth,
						FloatingLeft = parentPaneAsPositionableElement.FloatingLeft,
						FloatingTop = parentPaneAsPositionableElement.FloatingTop,
						FloatingWidth = parentPaneAsPositionableElement.FloatingWidth,
						FloatingHeight = parentPaneAsPositionableElement.FloatingHeight,
					})
				};

				Layout.FloatingWindows.Add(fw);
				fwc = new LayoutDocumentFloatingWindowControl((LayoutDocumentFloatingWindow)fw, isContentImmutable)
				{
					Width = fwWidth,
					Height = fwHeight,
					Left = contentModel.FloatingLeft,
					Top = contentModel.FloatingTop
				};
			}
			//fwc.Owner = Window.GetWindow(this);
			//fwc.SetParentToMainWindowOf(this);
			_fwList.Add(fwc);
			Layout.CollectGarbage();
			UpdateLayout();
			return fwc;
		}

		#endregion Private Methods
	}
}