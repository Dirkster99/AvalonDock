namespace MLibTest.Models
{
	using System;

	/// <summary>
	/// Implements an event that is invoked when a layout is loaded.
	/// </summary>
	internal class LayoutLoadedEventArgs : EventArgs
	{
		/// <summary>
		/// Class constructor
		/// </summary>
		public LayoutLoadedEventArgs(LayoutLoaderResult paramResult)
		{
			Result = paramResult;
		}

		/// <summary>
		/// Gets the layout result event including exceptions (if any)
		/// and whether loading was succesful...
		/// </summary>
		public LayoutLoaderResult Result { get; }
	}
}
