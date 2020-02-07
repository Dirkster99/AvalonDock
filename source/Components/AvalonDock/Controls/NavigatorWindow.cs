/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using AvalonDock.Layout;
using AvalonDock.Themes;
using System.Windows.Controls;

namespace AvalonDock.Controls
{
	/// <inheritdoc />
	/// <summary>
	/// Implements a floating window for navigating between documents and toolwindows in AvalonDock.
	/// The floating navigator window can be invoked with CTRL+TAB.
	/// </summary>
	/// <seealso cref="Window"/>
	[TemplatePart(Name = PART_AnchorableListBox, Type = typeof(ListBox))]
	[TemplatePart(Name = PART_DocumentListBox, Type = typeof(ListBox))]
	public class NavigatorWindow : Window
	{
		#region fields
		private const string PART_AnchorableListBox = "PART_AnchorableListBox";
		private const string PART_DocumentListBox = "PART_DocumentListBox";

		private ResourceDictionary currentThemeResourceDictionary; // = null
		private DockingManager _manager;
		private bool _isSelectingDocument;
		private ListBox _anchorableListBox;
		private ListBox _documentListBox;
		private bool _internalSetSelectedDocument = false;
		private bool _internalSetSelectedAnchorable = false;
		#endregion fields

		#region Constructors

		static NavigatorWindow()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(NavigatorWindow), new FrameworkPropertyMetadata(typeof(NavigatorWindow)));
			ShowActivatedProperty.OverrideMetadata(typeof(NavigatorWindow), new FrameworkPropertyMetadata(false));
			ShowInTaskbarProperty.OverrideMetadata(typeof(NavigatorWindow), new FrameworkPropertyMetadata(false));
		}

		internal NavigatorWindow(DockingManager manager)
		{
			_manager = manager;
			_internalSetSelectedDocument = true;
			SetAnchorables(_manager.Layout.Descendents().OfType<LayoutAnchorable>().Where(a => a.IsVisible).Select(d => (LayoutAnchorableItem)_manager.GetLayoutItemFromModel(d)).ToArray());
			SetDocuments(_manager.Layout.Descendents().OfType<LayoutDocument>().OrderByDescending(d => d.LastActivationTimeStamp.GetValueOrDefault()).Select(d => (LayoutDocumentItem)_manager.GetLayoutItemFromModel(d)).ToArray());
			_internalSetSelectedDocument = false;
			if (Documents.Length > 1)
			{
				InternalSetSelectedDocument(Documents[1]);
				_isSelectingDocument = true;
			}
			else if (Anchorables.Count() > 1)
			{
				InternalSetSelectedAnchorable(Anchorables.ToArray()[1]);
				_isSelectingDocument = false;
			}
			DataContext = this;
			Loaded += OnLoaded;
			Unloaded += OnUnloaded;
			UpdateThemeResources();
		}

		#endregion

		#region Properties

		#region Documents

		/// <summary><see cref="Documents"/> read-only dependency property.</summary>
		private static readonly DependencyPropertyKey DocumentsPropertyKey = DependencyProperty.RegisterReadOnly(nameof(Documents), typeof(IEnumerable<LayoutDocumentItem>), typeof(NavigatorWindow),
				new FrameworkPropertyMetadata(null));

		public static readonly DependencyProperty DocumentsProperty = DocumentsPropertyKey.DependencyProperty;

		/// <summary>
		/// Gets the <see cref="Documents"/> property. This dependency property 
		/// indicates the list of documents.
		/// </summary>
		public LayoutDocumentItem[] Documents => (LayoutDocumentItem[])GetValue(DocumentsProperty);

		#endregion

		#region Anchorables

		/// <summary><see cref="Anchorables"/> read-only dependency property.</summary>
		private static readonly DependencyPropertyKey AnchorablesPropertyKey = DependencyProperty.RegisterReadOnly(nameof(Anchorables), typeof(IEnumerable<LayoutAnchorableItem>), typeof(NavigatorWindow),
				new FrameworkPropertyMetadata((IEnumerable<LayoutAnchorableItem>)null));

		public static readonly DependencyProperty AnchorablesProperty = AnchorablesPropertyKey.DependencyProperty;

		/// <summary>
		/// Gets the <see cref="Anchorables"/> property. This dependency property 
		/// indicates the list of anchorables.
		/// </summary>
		public IEnumerable<LayoutAnchorableItem> Anchorables => (IEnumerable<LayoutAnchorableItem>)GetValue(AnchorablesProperty);

		#endregion

		#region SelectedDocument

		/// <summary><see cref="SelectedDocument"/> dependency property.</summary>
		public static readonly DependencyProperty SelectedDocumentProperty = DependencyProperty.Register(nameof(SelectedDocument), typeof(LayoutDocumentItem), typeof(NavigatorWindow),
				new FrameworkPropertyMetadata(null, OnSelectedDocumentChanged));

		/// <summary>
		/// Gets or sets the <see cref="SelectedDocument"/> property.  This dependency property 
		/// indicates the selected document.
		/// </summary>
		public LayoutDocumentItem SelectedDocument
		{
			get => (LayoutDocumentItem)GetValue(SelectedDocumentProperty);
			set => SetValue(SelectedDocumentProperty, value);
		}

		/// <summary>Handles changes to the <see cref="SelectedDocument"/> property.</summary>
		private static void OnSelectedDocumentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((NavigatorWindow)d).OnSelectedDocumentChanged(e);

		/// <summary>Provides derived classes an opportunity to handle changes to the <see cref="SelectedDocument"/> property.</summary>
		protected virtual void OnSelectedDocumentChanged(DependencyPropertyChangedEventArgs e)
		{
			if (_internalSetSelectedDocument|| SelectedDocument == null || !SelectedDocument.ActivateCommand.CanExecute(null)) return;
			Hide();
			SelectedDocument.ActivateCommand.Execute(null);
		}

		#endregion

		#region SelectedAnchorable

		/// <summary><see cref="SelectedAnchorable"/> dependency property.</summary>
		public static readonly DependencyProperty SelectedAnchorableProperty = DependencyProperty.Register(nameof(SelectedAnchorable), typeof(LayoutAnchorableItem), typeof(NavigatorWindow),
				new FrameworkPropertyMetadata(null, OnSelectedAnchorableChanged));

		/// <summary>
		/// Gets or sets the <see cref="SelectedAnchorable"/> property. This dependency property 
		/// indicates the selected anchorable.
		/// </summary>
		public LayoutAnchorableItem SelectedAnchorable
		{
			get => (LayoutAnchorableItem)GetValue(SelectedAnchorableProperty);
			set => SetValue(SelectedAnchorableProperty, value);
		}

		/// <summary>Handles changes to the <see cref="SelectedAnchorable"/> property.</summary>
		private static void OnSelectedAnchorableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((NavigatorWindow)d).OnSelectedAnchorableChanged(e);

		/// <summary>Provides derived classes an opportunity to handle changes to the <see cref="SelectedAnchorable"/> property.</summary>
		protected virtual void OnSelectedAnchorableChanged(DependencyPropertyChangedEventArgs e)
		{
			if (_internalSetSelectedAnchorable) return;
			// TODO: What goes on here??
			var selectedAnchorable = e.NewValue as LayoutAnchorableItem;
			if (SelectedAnchorable != null && SelectedAnchorable.ActivateCommand.CanExecute(null))
			{
				Close();
				SelectedAnchorable.ActivateCommand.Execute(null);
			}
		}

		#endregion

		#endregion

		#region Overrides   

		/// <inheritdoc />
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
			_anchorableListBox = GetTemplateChild(PART_AnchorableListBox) as ListBox;
			_documentListBox = GetTemplateChild(PART_DocumentListBox) as ListBox;
		}

		/// <inheritdoc />
		protected override void OnPreviewKeyDown(System.Windows.Input.KeyEventArgs e)
		{
			var shouldHandle = false;
			// Press Tab to switch Selected LayoutContent.
			if (e.Key == System.Windows.Input.Key.Tab)
			{
				// Selecting LayoutDocuments
				if (_isSelectingDocument)
				{
					if (SelectedDocument != null)
					{
						// Jump to next LayoutDocument
						// if we are on the last LayoutDocument and we have Anchorables we jump over to the Anchorables list
						// if there are no Anchorables we call SelectNextDocument which has logic in it to loop back to the top of the list
						var docIndex = Documents.IndexOf(SelectedDocument);
						if (docIndex < Documents.Length - 1 || !Anchorables.Any())
						{
							SelectNextDocument();
							shouldHandle = true;
						}
						// Jump to first LayoutAnchorable
						else if (Anchorables.Any())
						{
							_isSelectingDocument = false;
							InternalSetSelectedDocument(null);
							InternalSetSelectedAnchorable(Anchorables.First());
							shouldHandle = true;
						}
					}
					// There is no SelectedDocument, select the first one.
					else
					{
						if (Documents.Length > 0)
						{
							InternalSetSelectedDocument(Documents[0]);
							shouldHandle = true;
						}
					}
				}
				// Selecting LayoutAnchorables
				else
				{
					if (SelectedAnchorable != null)
					{
						// Jump to next LayoutAnchorable
						// if we are on the last LayoutAnchorable and we have one or more LayoutDocuments we jump over to the Documents list
						// if there are no Documents we call SelectNextAnchorable which has logic in it to loop back to the top of the list
						var anchorableIndex = Anchorables.ToArray().IndexOf(SelectedAnchorable);
						if (anchorableIndex < Anchorables.Count() - 1 || Documents.Count() == 0)
						{
							SelectNextAnchorable();
							shouldHandle = true;
						}
						// Jump to first LayoutDocument
						else if (Documents.Length > 0)
						{
							_isSelectingDocument = true;
							InternalSetSelectedAnchorable(null);
							InternalSetSelectedDocument(Documents[0]);
							shouldHandle = true;
						}
					}
					// There is no SelectedAnchorable, select the first one.
					else
					{
						if (Anchorables.Any())
						{
							InternalSetSelectedAnchorable(Anchorables.ToArray()[0]);
							shouldHandle = true;
						}
					}
				}
			}

			if (shouldHandle) e.Handled = true;
			base.OnPreviewKeyDown(e);
		}

		/// <inheritdoc />
		protected override void OnPreviewKeyUp(System.Windows.Input.KeyEventArgs e)
		{
			if (e.Key != System.Windows.Input.Key.Tab)
			{
				Close();
				if (SelectedDocument != null && SelectedDocument.ActivateCommand.CanExecute(null)) SelectedDocument.ActivateCommand.Execute(null);
				if (SelectedDocument == null && SelectedAnchorable != null && SelectedAnchorable.ActivateCommand.CanExecute(null)) SelectedAnchorable.ActivateCommand.Execute(null);
				e.Handled = true;
			}
			base.OnPreviewKeyUp(e);
		}


		#endregion

		#region Internal Methods

		/// <summary>
		/// Provides a secure method for setting the Anchorables property.  
		/// This dependency property indicates the list of anchorables.
		/// </summary>
		/// <param name="value">The new value for the property.</param>
		protected void SetAnchorables(IEnumerable<LayoutAnchorableItem> value) => SetValue(AnchorablesPropertyKey, value);

		/// <summary>
		/// Provides a secure method for setting the Documents property.  
		/// This dependency property indicates the list of documents.
		/// </summary>
		/// <param name="value">The new value for the property.</param>
		protected void SetDocuments(LayoutDocumentItem[] value) => SetValue(DocumentsPropertyKey, value);

		internal void UpdateThemeResources(Theme oldTheme = null)
		{
			if (oldTheme != null)
			{
				if (oldTheme is DictionaryTheme)
				{
					if (currentThemeResourceDictionary != null)
					{
						Resources.MergedDictionaries.Remove(currentThemeResourceDictionary);
						currentThemeResourceDictionary = null;
					}
				}
				else
				{
					var resourceDictionaryToRemove = Resources.MergedDictionaries.FirstOrDefault(r => r.Source == oldTheme.GetResourceUri());
					if (resourceDictionaryToRemove != null) Resources.MergedDictionaries.Remove(resourceDictionaryToRemove);
				}
			}

			if (_manager.Theme == null) return;
			if (_manager.Theme is DictionaryTheme dictionaryTheme)
			{
				currentThemeResourceDictionary = dictionaryTheme.ThemeResourceDictionary;
				Resources.MergedDictionaries.Add(currentThemeResourceDictionary);
			}
			else
				Resources.MergedDictionaries.Add(new ResourceDictionary() {Source = _manager.Theme.GetResourceUri()});
		}

		internal void SelectNextDocument()
		{
			if (SelectedDocument == null) return;
			var docIndex = Documents.IndexOf(SelectedDocument);
			docIndex++;
			if (docIndex == Documents.Length) docIndex = 0;
			InternalSetSelectedDocument(Documents[docIndex]);
		}

		internal void SelectNextAnchorable()
		{
			if (SelectedAnchorable == null) return;
			var anchorablesArray = Anchorables.ToArray();
			var anchorableIndex = anchorablesArray.IndexOf(SelectedAnchorable);
			anchorableIndex++;
			if (anchorableIndex == Anchorables.Count()) anchorableIndex = 0;
			InternalSetSelectedAnchorable(anchorablesArray[anchorableIndex]);
		}

		#endregion

		#region Private Methods

		private void InternalSetSelectedAnchorable(LayoutAnchorableItem anchorableToSelect)
		{
			_internalSetSelectedAnchorable = true;
			SelectedAnchorable = anchorableToSelect;
			_internalSetSelectedAnchorable = false;
			_anchorableListBox?.Focus();
		}

		private void InternalSetSelectedDocument(LayoutDocumentItem documentToSelect)
		{
			_internalSetSelectedDocument = true;
			SelectedDocument = documentToSelect;
			_internalSetSelectedDocument = false;
			if (_documentListBox != null && documentToSelect != null) _documentListBox.Focus();
		}

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			Loaded -= OnLoaded;
			if (_documentListBox != null && SelectedDocument != null) _documentListBox.Focus();
			else if (_anchorableListBox != null && SelectedAnchorable != null) _anchorableListBox.Focus();
			WindowStartupLocation = WindowStartupLocation.CenterOwner;
		}

		private void OnUnloaded(object sender, RoutedEventArgs e) => Unloaded -= OnUnloaded;

		#endregion
	}
}
