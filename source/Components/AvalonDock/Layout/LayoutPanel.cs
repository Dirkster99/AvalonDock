using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace AvalonDock.Layout
{
	/// <summary>
	/// Represents a layout panel.
	/// </summary>
	[ContentProperty(nameof(Children))]
	[Serializable]
	public class LayoutPanel : LayoutPositionableGroup<ILayoutPanelElement>, ILayoutPanelElement, ILayoutOrientableGroup
	{
		private Orientation _orientation;

		/// <summary>
		/// Initializes a new instance of the <see cref="LayoutPanel"/> class.
		/// </summary>
		public LayoutPanel()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="LayoutPanel"/> class.
		/// </summary>
		/// <param name="firstChild">The first child.</param>
		public LayoutPanel(ILayoutPanelElement firstChild)
		{
			Children.Add(firstChild);
		}

		/// <summary>
		/// Gets or sets the orientation.
		/// </summary>
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

		/// <summary>
		/// Using a DependencyProperty as the backing store for thhe <see cref="CanDock"/> property.
		/// </summary>
		public static readonly DependencyProperty CanDockProperty =
			DependencyProperty.Register("CanDock", typeof(bool),
				typeof(LayoutPanel), new PropertyMetadata(true));

		/// <summary>
		/// Gets or sets a value indicating whether this instance can dock.
		/// </summary>
		public bool CanDock
		{
			get { return (bool)GetValue(CanDockProperty); }
			set { SetValue(CanDockProperty, value); }
		}

		/// <inheritdoc/>
		protected override bool GetVisibility() => Children.Any(c => c.IsVisible);

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

	}
}