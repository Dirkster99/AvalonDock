#nullable disable
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace AvalonDock.Core.Serialization
{
	/// <summary>
	/// Represents a docking manager for layout serialization purposes.
	/// Implemented by the WPF DockingManager.
	/// </summary>
	public interface ISerializableDockingManager
	{
		/// <summary>Gets or sets the layout root.</summary>
		ISerializableLayoutRoot Layout { get; set; }

		/// <summary>Gets or sets a value indicating whether document source binding is suspended during deserialization.</summary>
		bool SuspendDocumentsSourceBinding { get; set; }

		/// <summary>Gets or sets a value indicating whether anchorable source binding is suspended during deserialization.</summary>
		bool SuspendAnchorablesSourceBinding { get; set; }

		/// <summary>Gets the DTO mapper for converting between layout tree and serialization DTOs.</summary>
		ILayoutDtoMapper DtoMapper { get; }
	}

	/// <summary>
	/// Represents a layout root that supports serialization traversal.
	/// </summary>
	public interface ISerializableLayoutRoot
	{
		/// <summary>Gets all descendant elements in the layout tree.</summary>
		/// <returns>An enumerable of all descendant layout elements.</returns>
		IEnumerable<ISerializableLayoutElement> Descendents();

		/// <summary>Removes empty containers and collects garbage from the layout tree.</summary>
		void CollectGarbage();
	}

	/// <summary>
	/// Base interface for all layout elements participating in serialization.
	/// </summary>
	public interface ISerializableLayoutElement
	{
		/// <summary>Fixes the cached root reference after deserialization.</summary>
		void FixCachedRootOnDeserialize();
	}

	/// <summary>
	/// Represents a layout content item (document or anchorable) for serialization.
	/// </summary>
	public interface ISerializableLayoutContent : ISerializableLayoutElement
	{
		/// <summary>Gets or sets the unique content identifier used to match items during deserialization.</summary>
		string ContentId { get; set; }

		/// <summary>Gets or sets the title.</summary>
		string Title { get; set; }

		/// <summary>Gets or sets the content object (typically a ViewModel).</summary>
		object Content { get; set; }

		/// <summary>Gets or sets the icon source.</summary>
		object IconSource { get; set; }

		/// <summary>Closes this content item, removing it from the layout.</summary>
		void Close();
	}

	/// <summary>
	/// Represents a layout anchorable for serialization.
	/// </summary>
	public interface ISerializableLayoutAnchorable : ISerializableLayoutContent
	{
		/// <summary>Hides the anchorable.</summary>
		/// <param name="cancelable">Whether the hide operation can be cancelled.</param>
		/// <returns>True if the operation was cancelled.</returns>
		bool HideAnchorable(bool cancelable);
	}

	/// <summary>
	/// Represents a layout document for serialization.
	/// </summary>
	[SuppressMessage("Design", "CA1040:Avoid empty interfaces", Justification = "Marker interface for serialization type safety")]
	public interface ISerializableLayoutDocument : ISerializableLayoutContent
	{
	}

	/// <summary>
	/// Represents a layout container for serialization fixup.
	/// </summary>
	[SuppressMessage("Design", "CA1040:Avoid empty interfaces", Justification = "Marker interface for serialization type safety")]
	public interface ISerializableLayoutContainer
	{
	}

	/// <summary>
	/// Represents an element that tracks its previous container (for restoring position).
	/// </summary>
	public interface ISerializablePreviousContainer : ISerializableLayoutElement
	{
		/// <summary>Gets or sets the previous container.</summary>
		ISerializableLayoutContainer PreviousContainer { get; set; }

		/// <summary>Gets or sets the previous container's serializable ID.</summary>
		string PreviousContainerId { get; set; }
	}

	/// <summary>
	/// Represents a layout pane with a serializable identifier.
	/// </summary>
	public interface ISerializableLayoutPane
	{
		/// <summary>Gets or sets the serializable ID for this pane.</summary>
		string Id { get; set; }
	}
}