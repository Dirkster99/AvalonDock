using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace AvalonDock.Commands
{
	/// <summary>
	/// Represents the weak Action.
	/// </summary>
	internal class WeakAction
	{
		private Action _staticAction;

		/// <summary>
		/// Initializes a new instance of the <see cref="WeakAction"/> class.
		/// </summary>
		/// <param name="action">The action.</param>
		public WeakAction(Action action)
			: this(action?.Target, action)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="WeakAction"/> class.
		/// </summary>
		/// <param name="target">The target.</param>
		/// <param name="action">The action.</param>
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

		/// <summary>
		/// Initializes a new instance of the <see cref="WeakAction"/> class.
		/// </summary>
		protected WeakAction()
		{
		}

		/// <summary>
		/// Gets a value indicating whether is Alive.
		/// </summary>
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
		/// Gets a value indicating whether is Static.
		/// </summary>
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
		/// Gets the method Name.
		/// </summary>
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
		/// Gets or sets the action Reference.
		/// </summary>
		protected WeakReference ActionReference
		{
			get;
			set;
		}

		/// <summary>
		/// Gets the action Target.
		/// </summary>
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
		/// <param name="param">The param.</param>
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
					catch
					{
					}

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
		/// Executes the mark For Deletion operation.
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
	}
}