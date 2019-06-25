namespace MLibTest.Models
{
    using System;

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
        public string XmlContent { get; }

        public bool LoadwasSuccesful { get; }

        public Exception LoadError { get; }
        #endregion properties
    }
}
