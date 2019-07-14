namespace MLibTest.Models
{
    using System;

    /// <summary>
    /// Implements a class that contains the items that describe the results
    /// of loading the AvalonDock layout from persistence.
    /// </summary>
    internal class LayoutLoaderResult
    {
        #region ctors
        /// <summary>
        /// Class constructor
        /// </summary>
        public LayoutLoaderResult(string paramXmlContent,
                                  bool paramLoadwasSuccesful,
                                  System.Exception paramLoadError
                                  )
            : this()
        {
            this.XmlContent = paramXmlContent;
            this.LoadwasSuccesful = paramLoadwasSuccesful;
            this.LoadError = paramLoadError;
        }

        protected LayoutLoaderResult()
        {
        }
        #endregion ctors

        #region properties
        /// <summary>
        /// Gets the Xml definition of the AvalonDock layout with a string object.
        /// </summary>
        public string XmlContent { get; }

        /// <summary>
        /// Determines whether loading the layout was successful or not.
        /// </summary>
        public bool LoadwasSuccesful { get; }

        /// <summary>
        /// Gets an <see cref="Exception"/> that might be available if layout loading
        /// was not succesful and additional error information is available.
        /// </summary>
        public Exception LoadError { get; }
        #endregion properties
    }
}
