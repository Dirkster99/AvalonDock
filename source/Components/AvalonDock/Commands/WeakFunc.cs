using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace AvalonDock.Commands
{
	/// <summary>
	/// Class WeakFunc.
	/// </summary>
	/// <typeparam name="TResult">The type of the t result.</typeparam>
	internal class WeakFunc<TResult>
	{
		#region Private Fields

		/// <summary>
		/// The static function
		/// </summary>
		private Func<TResult> _staticFunc;

		#endregion Private Fields

		#region Public Constructors

		/// <summary>
		/// Initializes a new instance of the WeakFunc class.
		/// </summary>
		/// <param name="func">The Func that will be associated to this instance.</param>
		public WeakFunc(Func<TResult> func)
			: this(func?.Target, func)
		{
		}

		/// <summary>
		/// Initializes a new instance of the WeakFunc class.
		/// </summary>
		/// <param name="target">The Func's owner.</param>
		/// <param name="func">The Func that will be associated to this instance.</param>
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

		#endregion Public Constructors

		#region Protected Constructors

		/// <summary>
		/// Initializes an empty instance of the WeakFunc class.
		/// </summary>
		protected WeakFunc()
		{
		}

		#endregion Protected Constructors

		#region Public Properties

		/// <summary>
		/// Gets a value indicating whether the Func's owner is still alive, or if it was collected
		/// by the Garbage Collector already.
		/// </summary>
		/// <value><c>true</c> 如果 this instance is alive; 否则, <c>false</c>.</value>
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
		/// Get a value indicating whether the WeakFunc is static or not.
		/// </summary>
		/// <value><c>true</c> 如果 this instance is static; 否则, <c>false</c>.</value>
		public bool IsStatic
		{
			get
			{
				return _staticFunc != null;
			}
		}

		/// <summary>
		/// Gets the name of the method that this WeakFunc represents.
		/// </summary>
		/// <value>The name of the method.</value>
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
		/// Gets the Func's owner. This object is stored as a
		/// <see cref="WeakReference" />.
		/// </summary>
		/// <value>The target.</value>
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

		#endregion Public Properties

		#region Protected Properties

		/// <summary>
		/// Gets or sets a WeakReference to this WeakFunc's action's target.
		/// This is not necessarily the same as
		/// <see cref="Reference" />, for example if the
		/// method is anonymous.
		/// </summary>
		/// <value>The function reference.</value>
		protected WeakReference FuncReference
		{
			get;
			set;
		}

		/// <summary>
		/// Gets the owner of the Func that was passed as parameter.
		/// This is not necessarily the same as
		/// <see cref="Target" />, for example if the
		/// method is anonymous.
		/// </summary>
		/// <value>The function target.</value>
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
		/// Gets or sets the <see cref="MethodInfo" /> corresponding to this WeakFunc's
		/// method passed in the constructor.
		/// </summary>
		/// <value>The method.</value>
		protected MethodInfo Method
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a WeakReference to the target passed when constructing
		/// the WeakFunc. This is not necessarily the same as
		/// <see cref="FuncReference" />, for example if the
		/// method is anonymous.
		/// </summary>
		/// <value>The reference.</value>
		protected WeakReference Reference
		{
			get;
			set;
		}

		#endregion Protected Properties

		#region Public Methods

		/// <summary>
		/// Executes the action. This only happens if the Func's owner
		/// is still alive.
		/// </summary>
		/// <returns>The result of the Func stored as reference.</returns>
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
		/// Sets the reference that this instance stores to null.
		/// </summary>
		public void MarkForDeletion()
		{
			Reference = null;
			FuncReference = null;
			Method = null;
			_staticFunc = null;
		}

		#endregion Public Methods
	}
}