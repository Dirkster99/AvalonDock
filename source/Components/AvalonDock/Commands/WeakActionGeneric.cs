using System;
using System.Diagnostics.CodeAnalysis;

namespace AvalonDock.Commands
{
	internal class WeakAction<T> : WeakAction, IExecuteWithObject
	{
		#region Private Fields

		private Action<T> _staticAction;

		#endregion Private Fields

		#region Public Constructors

		/// <summary>
		/// Initializes a new instance of the WeakAction class.
		/// </summary>
		/// <param name="action">The action that will be associated to this instance.</param>
		public WeakAction(Action<T> action)
			: this(action?.Target, action)
		{
		}

		/// <summary>
		/// Initializes a new instance of the WeakAction class.
		/// </summary>
		/// <param name="target">The action's owner.</param>
		/// <param name="action">The action that will be associated to this instance.</param>
		[SuppressMessage(
			"Microsoft.Design",
			"CA1062:Validate arguments of public methods",
			MessageId = "1",
			Justification = "Method should fail with an exception if action is null.")]
		public WeakAction(object target, Action<T> action)
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

		#region Public Properties

		/// <summary>
		/// Gets a value indicating whether the Action's owner is still alive, or if it was collected
		/// by the Garbage Collector already.
		/// </summary>
		public override bool IsAlive
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
		/// Gets the name of the method that this WeakAction represents.
		/// </summary>
		public override string MethodName
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

		#endregion Public Properties

		#region Public Methods

		/// <summary>
		/// Executes the action. This only happens if the action's owner
		/// is still alive. The action's parameter is set to default(T).
		/// </summary>
		public void Execute()
		{
			Execute(default);
		}

		/// <summary>
		/// Executes the action. This only happens if the action's owner
		/// is still alive.
		/// </summary>
		/// <param name="parameter">A parameter to be passed to the action.</param>
		public void Execute(T parameter)
		{
			if (_staticAction != null)
			{
				_staticAction(parameter);
				return;
			}

			var actionTarget = ActionTarget;

			if (IsAlive)
			{
				if (Method != null
					&& ActionReference != null
					&& actionTarget != null)
				{
					try
					{
						Method.Invoke(
						actionTarget,
						new object[]
						{
							parameter
						});
					}
					catch { }
				}
			}
		}

		/// <summary>
		/// Executes the action with a parameter of type object. This parameter
		/// will be casted to T. This method implements <see cref="IExecuteWithObject.ExecuteWithObject" />
		/// and can be useful if you store multiple WeakAction{T} instances but don't know in advance
		/// what type T represents.
		/// </summary>
		/// <param name="parameter">The parameter that will be passed to the action after
		/// being casted to T.</param>
		public void ExecuteWithObject(object parameter)
		{
			var parameterCasted = (T)parameter;
			Execute(parameterCasted);
		}

		/// <summary>
		/// Sets all the actions that this WeakAction contains to null,
		/// which is a signal for containing objects that this WeakAction
		/// should be deleted.
		/// </summary>
		public new void MarkForDeletion()
		{
			_staticAction = null;
			base.MarkForDeletion();
		}

		#endregion Public Methods
	}
}