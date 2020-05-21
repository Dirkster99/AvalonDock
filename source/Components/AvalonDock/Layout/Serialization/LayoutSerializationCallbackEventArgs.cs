/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using System.ComponentModel;

namespace AvalonDock.Layout.Serialization
{
	/// <summary>
	/// Implements an event that can be used to communicate between deserialization method
	/// and client application that a new item (LayoutAnchorable or Document) is about to
	/// be constructed and should be attached to a corresponding viewmodel.
	/// 
	/// The client application can use this event to Cancel reloading the item or
	/// attach (a viewmodel) content to the view item that is about to be reloaded and presented in the UI.
	/// 
	/// Use the Cancel property to indicate the case in which an item should not be deserialized.
	/// </summary>
	public class LayoutSerializationCallbackEventArgs : CancelEventArgs
	{
		#region constructors
		/// <summary>
		/// Class constructor from <see cref="LayoutContent"/> and <paramref name="previousContent"/> object.
		/// </summary>
		/// <param name="model">The model of the view that has been deserialized.</param>
		/// <param name="previousContent">The content if it was available in previous layout.</param>
		public LayoutSerializationCallbackEventArgs(LayoutContent model, object previousContent)
		{
			Cancel = false;            // reloading an item is not by cancelled by default
			Model = model;
			Content = previousContent;
		}

		#endregion constructors

		#region Properties
		/// <summary>
		/// Gets the model of the view that is about to be deserialized.
		/// </summary>
		public LayoutContent Model
		{
			get; private set;
		}

		/// <summary>
		/// Gets/sets the content for the <see cref="Model"/> that is about to be deserialized.
		/// </summary>
		public object Content { get; set; }

		#endregion Properties
	}
}
