using System;
using System.Collections.Generic;

namespace AvalonDock.Layout
{
	/// <summary>
	/// Provides a base class for layout floating window.
	/// </summary>
	[Serializable]
	public abstract class LayoutFloatingWindow : LayoutElement, ILayoutContainer
	{
		/// <summary>
		/// Gets the children.
		/// </summary>
		public abstract IEnumerable<ILayoutElement> Children { get; }

		/// <summary>
		/// Gets the children count.
		/// </summary>
		public abstract int ChildrenCount { get; }

		/// <summary>
		/// Gets a value indicating whether this instance is valid.
		/// </summary>
		public abstract bool IsValid { get; }

		/// <summary>
		/// Removes the child.
		/// </summary>
		/// <param name="element">The layout element.</param>
		public abstract void RemoveChild(ILayoutElement element);

		/// <summary>
		/// Replaces the child.
		/// </summary>
		/// <param name="oldElement">The existing layout element.</param>
		/// <param name="newElement">The replacement layout element.</param>
		public abstract void ReplaceChild(ILayoutElement oldElement, ILayoutElement newElement);
	}
}