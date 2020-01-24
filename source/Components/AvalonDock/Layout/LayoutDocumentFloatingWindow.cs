/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using System;
using System.Collections.Generic;
using System.Windows.Markup;
using System.Diagnostics;
using System.Linq;
using System.Xml.Serialization;
using System.Xml;

namespace AvalonDock.Layout
{
	[ContentProperty("RootPanel")]
	[Serializable]
	public class LayoutDocumentFloatingWindow : LayoutFloatingWindow, ILayoutElementWithVisibility
	{
		#region Constructors

		public LayoutDocumentFloatingWindow()
		{
		}

		#endregion

		#region Properties

		#region RootPanel

		private LayoutDocumentPaneGroup _rootPanel = null;
		public LayoutDocumentPaneGroup RootPanel
		{
			get
			{
				return _rootPanel;
			}
			set
			{
				if( _rootPanel != value )
				{
					if (_rootPanel != null)
						_rootPanel.ChildrenTreeChanged -= new EventHandler<ChildrenTreeChangedEventArgs>(_rootPanel_ChildrenTreeChanged);

					_rootPanel = value;
					if (_rootPanel != null)
						_rootPanel.Parent = this;

					if (_rootPanel != null)
						_rootPanel.ChildrenTreeChanged += new EventHandler<ChildrenTreeChangedEventArgs>(_rootPanel_ChildrenTreeChanged);

					RaisePropertyChanged("RootPanel");
					RaisePropertyChanged("IsSinglePane");
					RaisePropertyChanged("SinglePane");
					RaisePropertyChanged("Children");
					RaisePropertyChanged("ChildrenCount");
					((ILayoutElementWithVisibility)this).ComputeVisibility();
				}
			}
		}

		void _rootPanel_ChildrenTreeChanged(object sender, ChildrenTreeChangedEventArgs e)
		{
			RaisePropertyChanged("IsSinglePane");
			RaisePropertyChanged("SinglePane");

		}

		public bool IsSinglePane
		{
			get
			{
				return RootPanel != null && RootPanel.Descendents().OfType<ILayoutDocumentPane>().Where(p => p.IsVisible).Count() == 1;
			}
		}

		public ILayoutDocumentPane SinglePane
		{
			get
			{
				if (!IsSinglePane)
					return null;

				var singlePane = RootPanel.Descendents().OfType<LayoutDocumentPane>().Single(p => p.IsVisible);
				//singlePane.UpdateIsDirectlyHostedInFloatingWindow();
				return singlePane;
			}
		}


		#endregion

		#endregion

		#region Overrides

		public override IEnumerable<ILayoutElement> Children
		{
			get
			{
				if (ChildrenCount == 1)
					yield return RootPanel;
			}
		}

		public override void RemoveChild(ILayoutElement element)
		{
			Debug.Assert( element == RootPanel && element != null );
			RootPanel = null;
		}

		public override void ReplaceChild(ILayoutElement oldElement, ILayoutElement newElement)
		{
			Debug.Assert( oldElement == RootPanel && oldElement != null );
			RootPanel = newElement as LayoutDocumentPaneGroup;
		}

		public override int ChildrenCount
		{
			get
			{
				return RootPanel == null ? 0 : 1;
			}
		}

		#region IsVisible
		[NonSerialized]
		private bool _isVisible = true;

		[XmlIgnore]
		public bool IsVisible
		{
			get { return _isVisible; }
			private set
			{
				if (_isVisible != value)
				{
					RaisePropertyChanging("IsVisible");
					_isVisible = value;
					RaisePropertyChanged("IsVisible");
					if (IsVisibleChanged != null)
						IsVisibleChanged(this, EventArgs.Empty);
				}
			}
		}

		public event EventHandler IsVisibleChanged;

		#endregion

		void ILayoutElementWithVisibility.ComputeVisibility()
		{
			IsVisible = RootPanel != null && RootPanel.IsVisible;
		}


		public override bool IsValid
		{
			get
			{
				return RootPanel != null;
			}
		}

		public override void ReadXml(XmlReader reader)
		{
			reader.MoveToContent();
			if (reader.IsEmptyElement)
			{
				reader.Read();
				return;
			}

			var localName = reader.LocalName;
			reader.Read();

			while (true)
			{
				if (reader.LocalName.Equals(localName) && (reader.NodeType == XmlNodeType.EndElement))
				{
					break;
				}

				if (reader.NodeType == XmlNodeType.Whitespace)
				{
					reader.Read();
					continue;
				}

				XmlSerializer serializer;
				if (reader.LocalName.Equals("LayoutDocument"))
				{
					serializer = new XmlSerializer(typeof(LayoutDocument));
				}
				else
				{
					var type = LayoutRoot.FindType(reader.LocalName);
					if (type == null)
					{
						throw new ArgumentException("AvalonDock.LayoutDocumentFloatingWindow doesn't know how to deserialize " + reader.LocalName);
					}
					serializer = new XmlSerializer(type);
				}

				RootPanel = (LayoutDocumentPaneGroup)serializer.Deserialize(reader);
			}

			reader.ReadEndElement();
		}

#if TRACE
		public override void ConsoleDump(int tab)
		{
			System.Diagnostics.Trace.Write(new string(' ', tab * 4));
			System.Diagnostics.Trace.WriteLine("FloatingDocumentWindow()");

			RootPanel.ConsoleDump(tab + 1);
		}
#endif

		#endregion
	}
}
