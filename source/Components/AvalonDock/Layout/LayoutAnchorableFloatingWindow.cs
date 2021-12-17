/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Markup;
using System.Xml;
using System.Xml.Serialization;

namespace AvalonDock.Layout
{
	/// <summary>Implements the model for a floating window control that can host an anchorable control (tool window) in a floating window.</summary>
	[Serializable]
	[ContentProperty(nameof(RootPanel))]
	public class LayoutAnchorableFloatingWindow : LayoutFloatingWindow, ILayoutElementWithVisibility
	{
		#region fields

		private LayoutAnchorablePaneGroup _rootPanel;

		[NonSerialized]
		private bool _isVisible = true;

		#endregion fields

		#region Events

		/// <summary>Event is invoked when the visibility of this object has changed.</summary>
		public event EventHandler IsVisibleChanged;

		#endregion Events

		#region Properties

		public bool IsSinglePane => RootPanel != null && RootPanel.Descendents().OfType<ILayoutAnchorablePane>().Count(p => p.IsVisible) == 1;

		/// <summary>Gets/sets whether this object is in a state where it is visible in the UI or not.</summary>
		[XmlIgnore]
		public bool IsVisible
		{
			get => _isVisible;
			private set
			{
				if (value == _isVisible) return;
				RaisePropertyChanging(nameof(IsVisible));
				_isVisible = value;
				RaisePropertyChanged(nameof(IsVisible));
				IsVisibleChanged?.Invoke(this, EventArgs.Empty);
			}
		}

		public LayoutAnchorablePaneGroup RootPanel
		{
			get => _rootPanel;
			set
			{
				if (value == _rootPanel) return;
				RaisePropertyChanging(nameof(RootPanel));
				if (_rootPanel != null) _rootPanel.ChildrenTreeChanged -= _rootPanel_ChildrenTreeChanged;
				_rootPanel = value;
				if (_rootPanel != null)
				{
					_rootPanel.Parent = this;
					_rootPanel.ChildrenTreeChanged += _rootPanel_ChildrenTreeChanged;
				}

				RaisePropertyChanged(nameof(RootPanel));
				RaisePropertyChanged(nameof(IsSinglePane));
				RaisePropertyChanged(nameof(SinglePane));
				RaisePropertyChanged(nameof(Children));
				RaisePropertyChanged(nameof(ChildrenCount));
				((ILayoutElementWithVisibility)this).ComputeVisibility();
			}
		}

		public ILayoutAnchorablePane SinglePane
		{
			get
			{
				if (!IsSinglePane) return null;
				var singlePane = RootPanel.Descendents().OfType<LayoutAnchorablePane>().Single(p => p.IsVisible);
				singlePane.UpdateIsDirectlyHostedInFloatingWindow();
				return singlePane;
			}
		}

		#endregion Properties

		#region ILayoutElementWithVisibility Interface

		/// <inheritdoc />
		void ILayoutElementWithVisibility.ComputeVisibility() => ComputeVisibility();

		#endregion ILayoutElementWithVisibility Interface

		#region Overrides

		/// <inheritdoc />
		public override IEnumerable<ILayoutElement> Children
		{
			get { if (ChildrenCount == 1) yield return RootPanel; }
		}

		/// <inheritdoc />
		public override void RemoveChild(ILayoutElement element)
		{
			Debug.Assert(element == RootPanel && element != null);
			RootPanel = null;
		}

		/// <inheritdoc />
		public override void ReplaceChild(ILayoutElement oldElement, ILayoutElement newElement)
		{
			Debug.Assert(oldElement == RootPanel && oldElement != null);
			RootPanel = newElement as LayoutAnchorablePaneGroup;
		}

		/// <inheritdoc />
		public override int ChildrenCount => RootPanel == null ? 0 : 1;

		/// <inheritdoc />
		public override bool IsValid => RootPanel != null;

		/// <inheritdoc />
		public override void ReadXml(XmlReader reader)
		{
			reader.MoveToContent();
			if (reader.IsEmptyElement)
			{
				reader.Read();
				ComputeVisibility();
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
				if (reader.LocalName.Equals(nameof(LayoutAnchorablePaneGroup)))
					serializer = XmlSerializer.FromTypes(new[] { typeof(LayoutAnchorablePaneGroup) })[0];
				else
				{
					var type = LayoutRoot.FindType(reader.LocalName);
					if (type == null)
						throw new ArgumentException("AvalonDock.LayoutAnchorableFloatingWindow doesn't know how to deserialize " + reader.LocalName);
					serializer = XmlSerializer.FromTypes(new[] { type })[0];
				}
				RootPanel = (LayoutAnchorablePaneGroup)serializer.Deserialize(reader);
			}
			reader.ReadEndElement();
		}

#if TRACE
		/// <inheritdoc />
		public override void ConsoleDump(int tab)
		{
			System.Diagnostics.Trace.Write(new string(' ', tab * 4));
			System.Diagnostics.Trace.WriteLine("FloatingAnchorableWindow()");

			RootPanel.ConsoleDump(tab + 1);
		}
#endif

		#endregion Overrides

		#region Private Methods

		private void _rootPanel_ChildrenTreeChanged(object sender, ChildrenTreeChangedEventArgs e)
		{
			RaisePropertyChanged(nameof(IsSinglePane));
			RaisePropertyChanged(nameof(SinglePane));
		}

		private void ComputeVisibility() => IsVisible = RootPanel != null && RootPanel.IsVisible;

		#endregion Private Methods
	}
}