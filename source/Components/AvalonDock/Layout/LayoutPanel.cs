/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace AvalonDock.Layout
{
	/// <summary>Implements the layout model for the <see cref="Controls.LayoutPanelControl"/>.</summary>
	[ContentProperty(nameof(Children))]
	[Serializable]
	public class LayoutPanel : LayoutPositionableGroup<ILayoutPanelElement>, ILayoutPanelElement, ILayoutOrientableGroup
	{
		#region fields

		private Orientation _orientation;

		#endregion fields

		#region Constructors

		/// <summary>Class constructor</summary>
		public LayoutPanel()
		{
		}

		/// <summary>Class constructor</summary>
		/// <param name="firstChild"></param>
		public LayoutPanel(ILayoutPanelElement firstChild)
		{
			Children.Add(firstChild);
		}

		#endregion Constructors

		#region Properties

		/// <summary>Gets/sets the orientation for this panel.</summary>
		public Orientation Orientation
		{
			get => _orientation;
			set
			{
				if (value == _orientation) return;
				RaisePropertyChanging(nameof(Orientation));
				_orientation = value;
				RaisePropertyChanged(nameof(Orientation));
			}
		}

		#region CanDock

		/// <summary>
		/// Using a DependencyProperty as the backing store for thhe <see cref="CanDock"/> property.
		/// </summary>
		public static readonly DependencyProperty CanDockProperty =
			DependencyProperty.Register("CanDock", typeof(bool),
				typeof(LayoutPanel), new PropertyMetadata(true));

		/// <summary>
		/// Gets/sets dependency property that determines whether docking of dragged items
		/// is enabled or not. This property can be used disable/enable docking of
		/// dragged FloatingWindowControls.
		///
		/// This property should only be set to false if:
		/// <see cref="LayoutAnchorable.CanMove"/> and <see cref="LayoutDocument.CanMove"/>
		/// are false since users will otherwise be able to:
		/// 1) Drag an item away
		/// 2) But won't be able to dock it agin.
		/// </summary>
		public bool CanDock
		{
			get { return (bool)GetValue(CanDockProperty); }
			set { SetValue(CanDockProperty, value); }
		}

		#endregion CanDock

		#endregion Properties

		#region Overrides

		/// <inheritdoc />
		protected override bool GetVisibility() => Children.Any(c => c.IsVisible);

		/// <inheritdoc />
		public override void WriteXml(System.Xml.XmlWriter writer)
		{
			writer.WriteAttributeString(nameof(Orientation), Orientation.ToString());

			if (CanDock == false)
				writer.WriteAttributeString(nameof(CanDock), CanDock.ToString());

			base.WriteXml(writer);
		}

		/// <inheritdoc />
		/// <summary>
		/// This method is never invoked - <see cref="LayoutRoot"/>.ReadRootPanel()
		/// for implementation of this reader.
		/// </summary>
		public override void ReadXml(System.Xml.XmlReader reader)
		{
			if (reader.MoveToAttribute(nameof(Orientation)))
				Orientation = (Orientation)Enum.Parse(typeof(Orientation), reader.Value, true);
			if (reader.MoveToAttribute(nameof(CanDock)))
			{
				var canDockStr = reader.GetAttribute("CanDock");
				if (canDockStr != null)
					CanDock = bool.Parse(canDockStr);
			}
			base.ReadXml(reader);
		}

#if TRACE
		/// <inheritdoc />
		public override void ConsoleDump(int tab)
		{
			System.Diagnostics.Trace.Write(new string(' ', tab * 4));
			System.Diagnostics.Trace.WriteLine(string.Format("Panel({0})", Orientation));

			foreach (LayoutElement child in Children)
				child.ConsoleDump(tab + 1);
		}
#endif

		#endregion Overrides
	}
}