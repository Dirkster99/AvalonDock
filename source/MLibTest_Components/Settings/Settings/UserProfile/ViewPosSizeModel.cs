namespace Settings.UserProfile
{
    using System;
    using System.Windows;
    using System.Xml.Serialization;

    /// <summary>
    /// Simple wrapper class for allowing windows to persist their
    /// position, height, and width between user sessions in Properties.Default...
    /// 
    /// The storing of Positions is extended to store collections of
    /// window names and positions rather than just one window
    /// </summary>
    [Serializable]
    [XmlRoot(ElementName = "ControlPos", IsNullable = true)]
    public class ViewPosSizeModel : Settings.Interfaces.IViewPosSizeModel
    {
        #region fields
        private double mX, mY, mWidth, mHeight;
        private bool mIsMaximized;
        #endregion fields

        #region constructors
        /// <summary>
        /// Standard class constructor
        /// </summary>
        public ViewPosSizeModel()
        {
            mX = 0;
            mY = 0;
            mWidth = 0;
            mHeight = 0;
            mIsMaximized = false;
            DefaultConstruct = true;
        }

        /// <summary>
        /// Class cosntructor from coordinates of control
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public ViewPosSizeModel(double x,
                                double y,
                                double width,
                                double height,
                                bool isMaximized = false)
        {
            mX = x;
            mY = y;
            mWidth = width;
            mHeight = height;
            mIsMaximized = isMaximized;
            DefaultConstruct = false;
        }

        /// <summary>
        /// Construct from a single object*s propoerties for convinience.
        /// </summary>
        /// <param name="vs"></param>
        public ViewPosSizeModel(ViewSize vs)
            : this(vs.X, vs.Y, vs.Width, vs.Height)
        {
        }
        #endregion constructors

        #region properties
        /// <summary>
        /// Gets a default view size that is used when everything else fails.
        /// </summary>
        public static ViewSize DefaultSize
        {
            get
            {
                return new ViewSize(50, 50, 800, 550);
            }
        }

        /// <summary>
        /// Get whether this object was created through the default constructor or not
        /// (default data values can be easily overwritten by actual data).
        /// </summary>
        [XmlIgnore]
        public bool DefaultConstruct { get; private set; }

        /// <summary>
        /// Get/set X coordinate of control
        /// </summary>
        [XmlAttribute(AttributeName = "X")]
        public double X
        {
            get
            {
                return mX;
            }

            set
            {
                if (mX != value)
                {
                    mX = value;
                }
            }
        }

        /// <summary>
        /// Get/set Y coordinate of control
        /// </summary>
        [XmlAttribute(AttributeName = "Y")]
        public double Y
        {
            get
            {
                return mY;
            }

            set
            {
                if (mY != value)
                {
                    mY = value;
                }
            }
        }

        /// <summary>
        /// Get/set width of control
        /// </summary>
        [XmlAttribute(AttributeName = "Width")]
        public double Width
        {
            get
            {
                return mWidth;
            }

            set
            {
                if (mWidth != value)
                {
                    mWidth = value;
                }
            }
        }

        /// <summary>
        /// Get/set height of control
        /// </summary>
        [XmlAttribute(AttributeName = "Height")]
        public double Height
        {
            get
            {
                return mHeight;
            }

            set
            {
                if (mHeight != value)
                {
                    mHeight = value;
                }
            }
        }

        /// <summary>
        /// Get/set whether view is amximized or not
        /// </summary>
        [XmlAttribute(AttributeName = "IsMaximized")]
        public bool IsMaximized
        {
            get
            {
                return mIsMaximized;
            }

            set
            {
                if (mIsMaximized != value)
                {
                    mIsMaximized = value;
                }
            }
        }
        #endregion properties

        #region methods
        /// <summary>
        /// Convinience function to set the position of a view to a valid position
        /// </summary>
        public void SetValidPos(double SystemParameters_VirtualScreenLeft,
                                double SystemParameters_VirtualScreenTop)
        {
            // Restore the position with a valid position
            if (X < SystemParameters_VirtualScreenLeft)
                X = SystemParameters_VirtualScreenLeft;

            if (Y < SystemParameters_VirtualScreenTop)
                Y = SystemParameters_VirtualScreenTop;
        }

        /// <summary>
        /// Sets a Windows positions according to the data
        /// saved in this object.
        /// </summary>
        /// <param name="view"></param>
        public void SetWindowsState(IViewSize view)
        {
            if (view == null)
                return;

            view.Left = this.X;
            view.Top = this.Y;
            view.Width = this.Width;
            view.Height = this.Height;
            view.WindowState = (this.IsMaximized == true ? WindowState.Maximized : WindowState.Normal);
        }

        /// <summary>
        /// Sets a Windows positions according to the data
        /// saved in this object.
        /// </summary>
        /// <param name="view"></param>
        public void GetWindowsState(IViewSize view)
        {
            if (view == null)
                return;

            // Persist window position, width and height from this session
            this.X = view.Left;
            this.Y = view.Top;
            this.Width = view.Width;
            this.Height = view.Height;
            this.IsMaximized = (view.WindowState == WindowState.Maximized ? true : false);
        }
        #endregion methods
    }
}
