using System;
using System.Windows.Controls;
using System.Windows.Markup;

namespace AvalonDock.Layout
{
	/// <summary>
	/// Represents a layout document pane group.
	/// </summary>
	[ContentProperty(nameof(Children))]
	[Serializable]
	public class LayoutDocumentPaneGroup : LayoutPositionableGroup<ILayoutDocumentPane>, ILayoutDocumentPane, ILayoutOrientableGroup
	{
		private Orientation _orientation;

		/// <summary>
		/// Initializes a new instance of the <see cref="LayoutDocumentPaneGroup"/> class.
		/// </summary>
		public LayoutDocumentPaneGroup()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="LayoutDocumentPaneGroup"/> class.
		/// </summary>
		/// <param name="documentPane">The document pane.</param>
		public LayoutDocumentPaneGroup(LayoutDocumentPane documentPane)
		{
			Children.Add(documentPane);
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

		/// <inheritdoc/>
		protected override bool GetVisibility() => true;

#if TRACE
		/// <inheritdoc />
		public override void ConsoleDump(int tab)
		{
			System.Diagnostics.Trace.Write(new string(' ', tab * 4));
			System.Diagnostics.Trace.WriteLine(string.Format("DocumentPaneGroup({0})", Orientation));

			foreach (LayoutElement child in Children)
				child.ConsoleDump(tab + 1);
		}
#endif

	}
}