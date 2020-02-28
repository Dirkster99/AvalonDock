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
	/// <summary>Implements the layout model for the <see cref="Controls.LayoutDocumentFloatingWindowControl"/>.</summary>
	[ContentProperty(nameof(RootPanel))]
	[Serializable]
	public class LayoutDocumentFloatingWindow : LayoutFloatingWindow, ILayoutElementWithVisibility
	{
		#region fields
		private LayoutDocumentPaneGroup _rootPanel = null;

		[NonSerialized]
		private bool _isVisible = true;
		#endregion fields

		public event EventHandler IsVisibleChanged;

		#region Properties

		#region RootPanel

		public LayoutDocumentPaneGroup RootPanel
		{
			get => _rootPanel;
			set
			{
				if (_rootPanel == value) return;
				if (_rootPanel != null) _rootPanel.ChildrenTreeChanged -= _rootPanel_ChildrenTreeChanged;

				_rootPanel = value;
				if (_rootPanel != null) _rootPanel.Parent = this;
				if (_rootPanel != null) _rootPanel.ChildrenTreeChanged += _rootPanel_ChildrenTreeChanged;

				RaisePropertyChanged(nameof(RootPanel));
				RaisePropertyChanged(nameof(IsSinglePane));
				RaisePropertyChanged(nameof(SinglePane));
				RaisePropertyChanged(nameof(Children));
				RaisePropertyChanged(nameof(ChildrenCount));
				((ILayoutElementWithVisibility)this).ComputeVisibility();
			}
		}

		private void _rootPanel_ChildrenTreeChanged(object sender, ChildrenTreeChangedEventArgs e)
		{
			RaisePropertyChanged(nameof(IsSinglePane));
			RaisePropertyChanged(nameof(SinglePane));
		}

		#endregion RootPanel

		#region IsSinglePane
		public bool IsSinglePane => RootPanel?.Descendents().OfType<LayoutDocumentPane>().Count(p => p.IsVisible) == 1;

		public LayoutDocumentPane SinglePane
		{
			get
			{
				if (!IsSinglePane) return null;
				var singlePane = RootPanel.Descendents().OfType<LayoutDocumentPane>().Single(p => p.IsVisible);
				//singlePane.UpdateIsDirectlyHostedInFloatingWindow();
				return singlePane;
			}
		}
		#endregion IsSinglePane

		[XmlIgnore]
		public bool IsVisible
		{
			get => _isVisible;
			private set
			{
				if (_isVisible == value) return;
				RaisePropertyChanging(nameof(IsVisible));
				_isVisible = value;
				RaisePropertyChanged(nameof(IsVisible));
				IsVisibleChanged?.Invoke(this, EventArgs.Empty);
			}
		}

		#endregion Properties

		#region Overrides

		/// <inheritdoc />
		public override IEnumerable<ILayoutElement> Children
		{
			get { if (ChildrenCount == 1) yield return RootPanel; }
		}

		/// <inheritdoc />
		public override void RemoveChild(ILayoutElement element)
		{
			Debug.Assert( element == RootPanel && element != null );
			RootPanel = null;
		}

		/// <inheritdoc />
		public override void ReplaceChild(ILayoutElement oldElement, ILayoutElement newElement)
		{
			Debug.Assert( oldElement == RootPanel && oldElement != null );
			RootPanel = newElement as LayoutDocumentPaneGroup;
		}

		/// <inheritdoc />
		public override int ChildrenCount => RootPanel == null ? 0 : 1;

		void ILayoutElementWithVisibility.ComputeVisibility() => IsVisible = RootPanel != null && RootPanel.IsVisible;

		/// <inheritdoc />
		public override bool IsValid => RootPanel != null;

		/// <inheritdoc />
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
				if (reader.LocalName.Equals(localName) && reader.NodeType == XmlNodeType.EndElement) break;
				if (reader.NodeType == XmlNodeType.Whitespace)
				{
					reader.Read();
					continue;
				}

				XmlSerializer serializer;
				if (reader.LocalName.Equals(nameof(LayoutDocument)))
					serializer = new XmlSerializer(typeof(LayoutDocument));
				else
				{
					var type = LayoutRoot.FindType(reader.LocalName);
					if (type == null) throw new ArgumentException("AvalonDock.LayoutDocumentFloatingWindow doesn't know how to deserialize " + reader.LocalName);
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

		#endregion Overrides
	}
}
