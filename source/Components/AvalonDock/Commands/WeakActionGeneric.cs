using System;
using System.Diagnostics.CodeAnalysis;

namespace AvalonDock.Commands
{
	/// <summary>
	/// Represents the weak Action.
	/// </summary>
	/// <typeparam name="T">The t type.</typeparam>
	internal class WeakAction<T> : WeakAction, IExecuteWithObject
	{
		private Action<T> _staticAction;

		/// <summary>
		/// Initializes a new instance of the <see cref="WeakAction{T}"/> class.
		/// </summary>
		/// <param name="action">The action.</param>
		public WeakAction(Action<T> action)
			: this(action?.Target, action)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="WeakAction{T}"/> class.
		/// </summary>
		/// <param name="target">The target.</param>
		/// <param name="action">The action.</param>
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

		/// <inheritdoc/>
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

		/// <inheritdoc/>
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

		/// <summary>
		/// Executes the execute operation.
		/// </summary>
		public void Execute()
		{
			Execute(default);
		}

		/// <summary>
		/// Executes the execute operation.
		/// </summary>
		/// <param name="parameter">The converter parameter.</param>
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
					catch
					{
					}
				}
			}
		}

		/// <summary>
		/// Executes the execute With Object operation.
		/// </summary>
		/// <param name="parameter">The converter parameter.</param>
		public void ExecuteWithObject(object parameter)
		{
			var parameterCasted = (T)parameter;
			Execute(parameterCasted);
		}

		/// <summary>
		/// Executes the mark For Deletion operation.
		/// </summary>
		public new void MarkForDeletion()
		{
			_staticAction = null;
			base.MarkForDeletion();
		}
	}
}