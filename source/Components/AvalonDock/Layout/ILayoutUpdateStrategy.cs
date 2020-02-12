/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

namespace AvalonDock.Layout
{
	/// <summary>Defines the interface of a layout initializer class which can be invoked by the
	/// <see cref="DockingManager"/> before/after inserting a new Anchorable/Document to call
	/// custom client application code and determine whether the <see cref="DockingManager"/>
	/// should go ahead and insert the new Anchorable/Document or not.
	///
	/// The layout initializer object should be bound to the <see cref="DockingManager.LayoutUpdateStrategy"/> dependency property.</summary>
	public interface ILayoutUpdateStrategy
	{
		/// <summary>Is invoked before inserting a <see cref="LayoutAnchorable"/> to determine whether the <see cref="DockingManager"/> should insert this item at the given position or not.</summary>
		/// <param name="layout"></param>
		/// <param name="anchorableToShow"></param>
		/// <param name="destinationContainer"></param>
		/// <returns>False if the <see cref="DockingManager"/> should perform the indicated insertion and true if not (the client code should perform the insertion).</returns>
		bool BeforeInsertAnchorable(LayoutRoot layout,
									LayoutAnchorable anchorableToShow,
									ILayoutContainer destinationContainer);

		/// <summary>Is invoked after inserting a <see cref="LayoutAnchorable"/> to inform the client application and give it a chance to set the initial dimensions (DockWidth or DockHeight, depending on location) of the inserted Anchorable.</summary>
		/// <param name="layout"></param>
		/// <param name="anchorableToShow"></param>
		void AfterInsertAnchorable(LayoutRoot layout,
								   LayoutAnchorable anchorableShown);

		/// <summary>Is invoked before inserting a <see cref="LayoutDocument"/>  to determine whether the <see cref="DockingManager"/> should insert this item at the given position or not.</summary>
		/// <param name="layout"></param>
		/// <param name="anchorableToShow"></param>
		/// <param name="destinationContainer"></param>
		/// <returns>False if the <see cref="DockingManager"/> should perform the indicated insertion and true if not (the client code should perform the insertion).</returns>
		bool BeforeInsertDocument(LayoutRoot layout,
								  LayoutDocument anchorableToShow,
								  ILayoutContainer destinationContainer);

		/// <summary>Is invoked after inserting a <see cref="LayoutAnchorable"/> to inform the client application and give it a chance to set the initial dimensions (DockWidth or DockHeight, depending on location) of the inserted <see cref="LayoutDocument"/>.</summary>
		/// <param name="layout"></param>
		/// <param name="anchorableToShow"></param>
		void AfterInsertDocument(LayoutRoot layout,
								 LayoutDocument anchorableShown);
	}
}
