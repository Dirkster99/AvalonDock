using System.Collections;
using System.Collections.Generic;

namespace AvalonDock
{
	/// <summary>
	/// Represents the reference Equality Comparer.
	/// </summary>
	public sealed class ReferenceEqualityComparer : IEqualityComparer, IEqualityComparer<object>
	{
		/// <summary>
		/// Gets the default.
		/// </summary>
		public static ReferenceEqualityComparer Default { get; } = new ReferenceEqualityComparer();

		/// <summary>
		/// Executes the equals operation.
		/// </summary>
		/// <param name="x">The x.</param>
		/// <param name="y">The y.</param>
		/// <returns>true if the operation succeeds; otherwise, false.</returns>
		public new bool Equals(object x, object y) => ReferenceEquals(x, y);

		/// <summary>
		/// Gets the get Hash Code.
		/// </summary>
		/// <param name="obj">The obj.</param>
		/// <returns>The requested value.</returns>
		public int GetHashCode(object obj) => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
	}
}