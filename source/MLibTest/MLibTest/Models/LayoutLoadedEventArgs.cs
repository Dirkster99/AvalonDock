namespace MLibTest.Models
{
    using System;

    /// <summary>
    /// Implements an event that is invoked when a layout is loaded.
    /// </summary>
    internal class LayoutLoadedEventArgs : EventArgs
    {
        #region ctors
        /// <summary>
        /// Class constructor
        /// </summary>
        public LayoutLoadedEventArgs(LayoutLoaderResult paramResult)
        {
            Result = paramResult;
        }
        #endregion ctors

        #region properties
        /// <summary>
        /// Gets the layout result event including exceptions (if any)
        /// and whether loading was succesful...
        /// </summary>
        public LayoutLoaderResult Result { get; }
        #endregion properties
    }
}
