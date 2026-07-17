using System;
using System.Diagnostics.CodeAnalysis;

namespace AvalonDock.Commands
{
	/// <summary>
	/// Represents the weak Func.
	/// </summary>
	/// <typeparam name="T">The t type.</typeparam>
	/// <typeparam name="TResult">The result type.</typeparam>
	internal class WeakFunc<T, TResult> : WeakFunc<TResult>, IExecuteWithObjectAndResult
	{
		private Func<T, TResult> _staticFunc;

		/// <summary>
		/// Initializes a new instance of the <see cref="WeakFunc{T, TResult}"/> class.
		/// </summary>
		/// <param name="func">The func.</param>
		public WeakFunc(Func<T, TResult> func)
			: this(func?.Target, func)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="WeakFunc{T, TResult}"/> class.
		/// </summary>
		/// <param name="target">The target.</param>
		/// <param name="func">The func.</param>
		[SuppressMessage(
			"Microsoft.Design",
			"CA1062:Validate arguments of public methods",
			MessageId = "1",
			Justification = "Method should fail with an exception if func is null.")]
		public WeakFunc(object target, Func<T, TResult> func)
		{
			if (func.Method.IsStatic)
			{
				_staticFunc = func;

				if (target != null)
				{
					// Keep a reference to the target to control the
					// WeakAction's lifetime.
					Reference = new WeakReference(target);
				}

				return;
			}

			Method = func.Method;
			FuncReference = new WeakReference(func.Target);
			Reference = new WeakReference(target);
		}

		/// <inheritdoc/>
		public override bool IsAlive
		{
			get
			{
				if (_staticFunc == null
					&& Reference == null)
				{
					return false;
				}

				if (_staticFunc != null)
				{
					if (Reference != null)
					{
						return Reference.IsAlive;
					}

					return true;
				}

				return Reference.IsAlive;
			}
		}

		/// <inheritdoc/>
		public override string MethodName
		{
			get
			{
				if (_staticFunc != null)
				{
					return _staticFunc.Method.Name;
				}

				return Method.Name;
			}
		}

		/// <summary>
		/// Executes the execute operation.
		/// </summary>
		/// <returns>The result of the operation.</returns>
		public new TResult Execute()
		{
			return Execute(default);
		}

		/// <summary>
		/// Executes the execute operation.
		/// </summary>
		/// <param name="parameter">The converter parameter.</param>
		/// <returns>The result of the operation.</returns>
		public TResult Execute(T parameter)
		{
			if (_staticFunc != null)
			{
				return _staticFunc(parameter);
			}

			var funcTarget = FuncTarget;

			if (IsAlive)
			{
				if (Method != null
					&& FuncReference != null
					&& funcTarget != null)
				{
					return (TResult)Method.Invoke(
						funcTarget,
						new object[]
						{
							parameter
						});
				}
			}

			return default;
		}

		/// <summary>
		/// Executes the execute With Object operation.
		/// </summary>
		/// <param name="parameter">The converter parameter.</param>
		/// <returns>The result of the operation.</returns>
		public object ExecuteWithObject(object parameter)
		{
			var parameterCasted = (T)parameter;
			return Execute(parameterCasted);
		}

		/// <summary>
		/// Executes the mark For Deletion operation.
		/// </summary>
		public new void MarkForDeletion()
		{
			_staticFunc = null;
			base.MarkForDeletion();
		}
	}
}