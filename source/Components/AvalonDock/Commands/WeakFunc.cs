using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace AvalonDock.Commands
{
	/// <summary>
	/// Represents the weak Func.
	/// </summary>
	/// <typeparam name="TResult">The result type.</typeparam>
	internal class WeakFunc<TResult>
	{
		private Func<TResult> _staticFunc;

		/// <summary>
		/// Initializes a new instance of the <see cref="WeakFunc{TResult}"/> class.
		/// </summary>
		/// <param name="func">The func.</param>
		public WeakFunc(Func<TResult> func)
			: this(func?.Target, func)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="WeakFunc{TResult}"/> class.
		/// </summary>
		/// <param name="target">The target.</param>
		/// <param name="func">The func.</param>
		[SuppressMessage(
			"Microsoft.Design",
			"CA1062:Validate arguments of public methods",
			MessageId = "1",
			Justification = "Method should fail with an exception if func is null.")]
		public WeakFunc(object target, Func<TResult> func)
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

		/// <summary>
		/// Initializes a new instance of the <see cref="WeakFunc{TResult}"/> class.
		/// </summary>
		protected WeakFunc()
		{
		}

		/// <summary>
		/// Gets a value indicating whether is Alive.
		/// </summary>
		public virtual bool IsAlive
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

		/// <summary>
		/// Gets a value indicating whether is Static.
		/// </summary>
		public bool IsStatic
		{
			get
			{
				return _staticFunc != null;
			}
		}

		/// <summary>
		/// Gets the method Name.
		/// </summary>
		public virtual string MethodName
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
		/// Gets the target.
		/// </summary>
		public object Target
		{
			get
			{
				if (Reference == null)
				{
					return null;
				}

				return Reference.Target;
			}
		}

		/// <summary>
		/// Gets or sets the func Reference.
		/// </summary>
		protected WeakReference FuncReference
		{
			get;
			set;
		}

		/// <summary>
		/// Gets the func Target.
		/// </summary>
		protected object FuncTarget
		{
			get
			{
				if (FuncReference == null)
				{
					return null;
				}

				return FuncReference.Target;
			}
		}

		/// <summary>
		/// Gets or sets the method.
		/// </summary>
		protected MethodInfo Method
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the reference.
		/// </summary>
		protected WeakReference Reference
		{
			get;
			set;
		}

		/// <summary>
		/// Executes the execute operation.
		/// </summary>
		/// <returns>The result of the operation.</returns>
		public TResult Execute()
		{
			if (_staticFunc != null)
			{
				return _staticFunc();
			}

			var funcTarget = FuncTarget;

			if (IsAlive)
			{
				if (Method != null
					&& FuncReference != null
					&& funcTarget != null)
				{
					return (TResult)Method.Invoke(funcTarget, null);
				}
			}

			return default;
		}

		/// <summary>
		/// Executes the mark For Deletion operation.
		/// </summary>
		public void MarkForDeletion()
		{
			Reference = null;
			FuncReference = null;
			Method = null;
			_staticFunc = null;
		}
	}
}