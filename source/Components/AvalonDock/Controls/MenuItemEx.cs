/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace AvalonDock.Controls
{
	/// <inheritdoc />
	/// <summary>
	/// Implements a <see cref="MenuItem"/> extension that is used to display
	/// menu items in the <see cref="ContextMenuEx"/> of the <see cref="LayoutDocumentPaneControl"/>.
	/// </summary>
	/// <seealso cref="MenuItem"/>
	public class MenuItemEx : MenuItem
	{
		#region fields

		private bool _reentrantFlag = false;

		#endregion fields

		#region Constructors

		static MenuItemEx()
		{
			IconProperty.OverrideMetadata(typeof(MenuItemEx), new FrameworkPropertyMetadata(OnIconPropertyChanged));
		}

		#endregion Constructors

		#region Properties

		#region IconTemplate

		/// <summary><see cref="IconTemplate "/> dependency property.</summary>
		public static readonly DependencyProperty IconTemplateProperty = DependencyProperty.Register(nameof(IconTemplate), typeof(DataTemplate), typeof(MenuItemEx),
				new FrameworkPropertyMetadata(null, OnIconTemplateChanged));

		/// <summary>Gets/sets the <see cref="DataTemplate"/> for the icon in the menu item.</summary>
		[Bindable(true), Description("Gets/sets the data template for the icon in the menu item.."), Category("Menu")]
		public DataTemplate IconTemplate
		{
			get => (DataTemplate)GetValue(IconTemplateProperty);
			set => SetValue(IconTemplateProperty, value);
		}

		/// <summary>Handles changes to the <see cref="IconTemplate "/> property.</summary>
		private static void OnIconTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((MenuItemEx)d).OnIconTemplateChanged(e);

		/// <summary>Provides derived classes an opportunity to handle changes to the <see cref="IconTemplate "/> property.</summary>
		protected virtual void OnIconTemplateChanged(DependencyPropertyChangedEventArgs e) => UpdateIcon();

		#endregion IconTemplate

		#region IconTemplateSelector

		/// <summary><see cref="IconTemplateSelector"/> dependency property.</summary>
		public static readonly DependencyProperty IconTemplateSelectorProperty = DependencyProperty.Register(nameof(IconTemplateSelector), typeof(DataTemplateSelector), typeof(MenuItemEx),
				new FrameworkPropertyMetadata(null, OnIconTemplateSelectorChanged));

		/// <summary>Gets/sets the <see cref="DataTemplateSelector"/> for the icon in the menu item.</summary>
		[Bindable(true), Description("Gets/sets the DataTemplateSelector for the icon in the menu item."), Category("Menu")]
		public DataTemplateSelector IconTemplateSelector
		{
			get => (DataTemplateSelector)GetValue(IconTemplateSelectorProperty);
			set => SetValue(IconTemplateSelectorProperty, value);
		}

		/// <summary>Handles changes to the <see cref="IconTemplateSelector"/> property.</summary>
		private static void OnIconTemplateSelectorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((MenuItemEx)d).OnIconTemplateSelectorChanged(e);

		/// <summary>Provides derived classes an opportunity to handle changes to the <see cref="IconTemplateSelector"/> property.</summary>
		protected virtual void OnIconTemplateSelectorChanged(DependencyPropertyChangedEventArgs e) => UpdateIcon();

		#endregion IconTemplateSelector

		#endregion Properties

		#region Private Mehods

		private static void OnIconPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
		{
			if (e.NewValue != null) ((MenuItemEx)sender).UpdateIcon();
		}

		private void UpdateIcon()
		{
			if (_reentrantFlag) return;
			_reentrantFlag = true;
			if (IconTemplateSelector != null)
			{
				var dataTemplateToUse = IconTemplateSelector.SelectTemplate(Icon, this);
				if (dataTemplateToUse != null) Icon = dataTemplateToUse.LoadContent();
			}
			else if (IconTemplate != null)
				Icon = IconTemplate.LoadContent();
			_reentrantFlag = false;
		}

		#endregion Private Mehods
	}
}