using System;

namespace AvalonDock.Controls
{
	/// <summary>
	/// Represents the reentrant Flag.
	/// </summary>
	internal class ReentrantFlag
	{
		private bool _flag = false;

		/// <summary>
		/// Gets a value indicating whether can Enter.
		/// </summary>
		public bool CanEnter
		{
			get
			{
				return !_flag;
			}
		}

		/// <summary>
		/// Executes the enter operation.
		/// </summary>
		/// <returns>The result of the operation.</returns>
		public _ReentrantFlagHandler Enter()
		{
			if (_flag)
				throw new InvalidOperationException();
			return new _ReentrantFlagHandler(this);
		}

		/// <summary>
		/// Represents the reentrant Flag Handler.
		/// </summary>
		public class _ReentrantFlagHandler : IDisposable
		{
			private ReentrantFlag _owner;

			/// <summary>
			/// Initializes a new instance of the <see cref="_ReentrantFlagHandler"/> class.
			/// </summary>
			/// <param name="owner">The owner.</param>
			public _ReentrantFlagHandler(ReentrantFlag owner)
			{
				_owner = owner;
				_owner._flag = true;
			}

			/// <summary>
			/// Executes the dispose operation.
			/// </summary>
			public void Dispose()
			{
				_owner._flag = false;
			}
		}
	}
}