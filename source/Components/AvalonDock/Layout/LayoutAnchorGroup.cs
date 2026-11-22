using System;
using System.Windows.Markup;
using System.Xml.Serialization;

namespace AvalonDock.Layout
{
	/// <summary>
	/// Represents a layout anchor group.
	/// </summary>
	[ContentProperty(nameof(Children))]
	[Serializable]
	public class LayoutAnchorGroup : LayoutGroup<LayoutAnchorable>, ILayoutPreviousContainer, ILayoutPaneSerializable, Core.Serialization.ISerializableLayoutPane, Core.Serialization.ISerializablePreviousContainer
	{
		/// <inheritdoc/>
		protected override bool GetVisibility() => Children.Count > 0;

		[field: NonSerialized]
		private ILayoutContainer _previousContainer = null;

		/// <inheritdoc/>
		[XmlIgnore]
		ILayoutContainer ILayoutPreviousContainer.PreviousContainer
		{
			get => _previousContainer;
			set
			{
				if (value == _previousContainer) return;
				_previousContainer = value;
				RaisePropertyChanged(nameof(ILayoutPreviousContainer.PreviousContainer));
				if (_previousContainer is ILayoutPaneSerializable paneSerializable && paneSerializable.Id == null)
					paneSerializable.Id = Guid.NewGuid().ToString();
			}
		}

		/// <inheritdoc/>
		string ILayoutPreviousContainer.PreviousContainerId { get; set; }

		private string _id;

		/// <inheritdoc/>
		string ILayoutPaneSerializable.Id { get => _id; set => _id = value; }

		/// <inheritdoc/>
		string Core.Serialization.ISerializableLayoutPane.Id { get => _id; set => _id = value; }

		/// <inheritdoc/>
		Core.Serialization.ISerializableLayoutContainer Core.Serialization.ISerializablePreviousContainer.PreviousContainer
		{
			get => _previousContainer as Core.Serialization.ISerializableLayoutContainer;
			set => ((ILayoutPreviousContainer)this).PreviousContainer = value as ILayoutContainer;
		}

		/// <inheritdoc/>
		string Core.Serialization.ISerializablePreviousContainer.PreviousContainerId
		{
			get => ((ILayoutPreviousContainer)this).PreviousContainerId;
			set => ((ILayoutPreviousContainer)this).PreviousContainerId = value;
		}
	}
}