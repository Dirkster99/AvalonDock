namespace MLibTest.Models
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Implements base methods and events for loading an AvalonDock layout
    /// in a background thread.
    /// </summary>
    internal class LayoutLoader : IDisposable
    {
        #region fields
        private SemaphoreSlim _LayoutSemaphore;
        private readonly string _layoutFileName;
        private LayoutLoaderResult _LayoutLoaded;
        #endregion fields

        #region ctors
        /// <summary>
        /// Class constructor
        /// </summary>
        public LayoutLoader(string layoutFileName)
            : this()
        {
            _layoutFileName = layoutFileName;
        }

        /// <summary>
        /// Hidden class constructor
        /// </summary>
        protected LayoutLoader()
        {
            _LayoutSemaphore = new SemaphoreSlim(1, 1);
        }
        #endregion ctors

        #region events
        /// <summary>
        /// Implements an event that is raised when the AvalonDock layout
        /// was successfully loaded.
        /// </summary>
        public EventHandler<LayoutLoadedEventArgs> LayoutLoadedEvent;
        private bool _Disposed;
        #endregion events

        #region methods
        /// <summary>
        /// Loads the AvalonDockLayout with a background task and makes the result
        /// available in a private <see cref="_LayoutLoaded"/> field.
        /// 
        /// The result can later be queried with the <see cref="GetLayoutString"/> method
        /// which will eiter return the available result of connect to an eventhandler
        /// The will return the result via <see cref="LayoutLoadedEvent"/> as soon as it
        /// is available.
        /// </summary>
        public void LoadLayout()
        {
            try
            {
                Task.Factory.StartNew<LayoutLoaderResult>(() => LoadAvalonDockLayoutToString()
                    ).ContinueWith(async r =>
                    {
                        await this._LayoutSemaphore.WaitAsync();
                        try
                        {
                            this._LayoutLoaded = r.Result;

                            // Send an event if event subscriber is available
                            // if MainWindow is already successfull constructed and waiting for Xml Layout
                            LayoutLoadedEvent?.Invoke(this, new LayoutLoadedEventArgs(r.Result));
                        }
                        finally
                        {
                            this._LayoutSemaphore.Release();
                        }
                    });
            }
            catch (Exception exc)
            {
                this._LayoutLoaded = new LayoutLoaderResult(null, false, exc);
            }
        }

        #region IDisposable

        /// <summary>
        /// Standard dispose method of the <seealso cref="IDisposable" /> interface.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Source: http://www.codeproject.com/Articles/15360/Implementing-IDisposable-and-the-Dispose-Pattern-P
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (_Disposed == false)
            {
                if (disposing == true)
                {
                    // Dispose of the curently displayed content
                    _LayoutSemaphore.Dispose();
                }

                // There are no unmanaged resources to release, but
                // if we add them, they need to be released here.
            }

            _Disposed = true;

            //// If it is available, make the call to the
            //// base class's Dispose(Boolean) method
            ////base.Dispose(disposing);
        }
        #endregion IDisposable

        /// <summary>
        /// Loads the layout object queried via <see cref="LoadLayout"/> method or
        /// connects the caller to the eventhandler to return the result object at
        /// a later stage (if it was not available at this time).
        /// </summary>
        /// <param name="loadEventHandler"></param>
        /// <returns></returns>
        internal async Task<LayoutLoaderResult> GetLayoutString(EventHandler<LayoutLoadedEventArgs> loadEventHandler)
        {
            await this._LayoutSemaphore.WaitAsync();
            try
            {
                if (this._LayoutLoaded != null)
                    return this._LayoutLoaded;
                else
                {
                    // Attach event to return result later
                    LayoutLoadedEvent += loadEventHandler;

                    return null;
                }
            }
            finally
            {
                this._LayoutSemaphore.Release();
            }
        }

        /// <summary>
        /// Reads the AvalonDock layout into a string and returns it with additional
        /// information wrapped into a <see cref="LayoutLoaderResult"/>.
        /// </summary>
        /// <returns></returns>
        private LayoutLoaderResult LoadAvalonDockLayoutToString()
        {
            string path = GetFullPathToLayout();

            if (System.IO.File.Exists(path) == false)
                return null;

            try
            {
                //Thread.Sleep(2000);
                return new LayoutLoaderResult(System.IO.File.ReadAllText(path), true, null);
            }
            catch (Exception exc)
            {
                return new LayoutLoaderResult(null, false, exc);
            }
        }

        /// <summary>
        /// Gets whether a standard layout file is available for loading.
        /// </summary>
        /// <returns></returns>
        internal bool CanLoadLayout()
        {
            return System.IO.File.Exists(GetFullPathToLayout());
        }

        /// <summary>
        /// Gets the full path to the layout file that stores the AvalonDock layout
        /// and is used to store/restore the layout of the controls.
        /// </summary>
        /// <returns></returns>
        internal string GetFullPathToLayout()
        {
            return System.IO.Path.GetFullPath(_layoutFileName);
        }
        #endregion methods
    }
}
