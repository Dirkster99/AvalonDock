using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace AvalonDock.Commands
{
	/// <summary>
	/// Class WeakAction.
	/// </summary>
	internal class WeakAction
	{
		#region Private Fields

		/// <summary>
		/// The static action
		/// </summary>
		private Action _staticAction;

		#endregion Private Fields

		#region Public Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="WeakAction" /> class.
		/// </summary>
		/// <param name="action">The action that will be associated to this instance.</param>
		public WeakAction(Action action)
			: this(action?.Target, action)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="WeakAction" /> class.
		/// </summary>
		/// <param name="target">The action's owner.</param>
		/// <param name="action">The action that will be associated to this instance.</param>
		[SuppressMessage(
			"Microsoft.Design",
			"CA1062:Validate arguments of public methods",
			MessageId = "1",
			Justification = "Method should fail with an exception if action is null.")]
		public WeakAction(object target, Action action)
		{
			if (action.Method.IsStatic)
			{
				_staticAction = action;

				if (target != null)
				{
					// Keep a reference to the target to control the
					// WeakAction's lifetime.
					Reference = new WeakReference(target);
				}

				return;
			}
			Method = action.Method;
			ActionReference = new WeakReference(action.Target);
			Reference = new WeakReference(target);
		}

		#endregion Public Constructors

		#region Protected Constructors

		/// <summary>
		/// Initializes an empty instance of the <see cref="WeakAction" /> class.
		/// </summary>
		protected WeakAction()
		{
		}

		#endregion Protected Constructors

		#region Public Properties

		/// <summary>
		/// Gets a value indicating whether the Action's owner is still alive, or if it was collected
		/// by the Garbage Collector already.
		/// </summary>
		/// <value><c>true</c> 如果 this instance is alive; 否则, <c>false</c>.</value>
		public virtual bool IsAlive
		{
			get
			{
				if (_staticAction == null
					&& Reference == null)
				{
					return false;
				}

				if (_staticAction != null)
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
		/// Gets a value indicating whether the WeakAction is static or not.
		/// </summary>
		/// <value><c>true</c> 如果 this instance is static; 否则, <c>false</c>.</value>
		public bool IsStatic
		{
			get
			{
#if SILVERLIGHT
                return (_action != null && _action.Target == null)
                    || _staticAction != null;
#else
				return _staticAction != null;
#endif
			}
		}

		/// <summary>
		/// Gets the name of the method that this WeakAction represents.
		/// </summary>
		/// <value>The name of the method.</value>
		public virtual string MethodName
		{
			get
			{
				if (_staticAction != null)
				{
					return _staticAction.Method.Name;
				}
				return Method.Name;
			}
		}

		/// <summary>
		/// Gets the Action's owner. This object is stored as a
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
		/// Gets or sets a WeakReference to this WeakAction's action's target.
		/// This is not necessarily the same as
		/// <see cref="Reference" />, for example if the
		/// method is anonymous.
		/// </summary>
		/// <value>The action reference.</value>
		protected WeakReference ActionReference
		{
			get;
			set;
		}

		/// <summary>
		/// The target of the weak reference.
		/// </summary>
		/// <value>The action target.</value>
		protected object ActionTarget
		{
			get
			{
				if (ActionReference == null)
				{
					return null;
				}

				return ActionReference.Target;
			}
		}

		/// <summary>
		/// Gets or sets the <see cref="MethodInfo" /> corresponding to this WeakAction's
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
		/// the WeakAction. This is not necessarily the same as
		/// <see cref="ActionReference" />, for example if the
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
		/// Executes the action. This only happens if the action's owner
		/// is still alive.
		/// </summary>
		public void Execute(object param = null)
		{
			if (_staticAction != null)
			{
				_staticAction();
				return;
			}

			var actionTarget = ActionTarget;

			if (IsAlive)
			{
				if (Method != null
					&& ActionReference != null
					&& actionTarget != null)
				{
					var paras = Method.GetParameters().Count();
					try
					{
						if (paras > 0)
						{
							Method.Invoke(actionTarget, new object[] { param });
						}
						else
						{
							Method.Invoke(actionTarget, null);
						}
					}
					catch { }
					// ReSharper disable RedundantJumpStatement
					return;
					// ReSharper restore RedundantJumpStatement
				}

#if SILVERLIGHT
                if (_action != null)
                {
                    _action();
                }
#endif
			}
		}

		/// <summary>
		/// Sets the reference that this instance stores to null.
		/// </summary>
		public void MarkForDeletion()
		{
			Reference = null;
			ActionReference = null;
			Method = null;
			_staticAction = null;

#if SILVERLIGHT
            _action = null;
#endif
		}

		#endregion Public Methods
	}
}