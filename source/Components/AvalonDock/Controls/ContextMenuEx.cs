using System.Windows.Controls;
using System.Windows.Data;

namespace AvalonDock.Controls
{
	/// <summary>
	/// Represents the context Menu Ex.
	/// </summary>
	public class ContextMenuEx : ContextMenu
	{
		/// <summary>
		/// Initializes static members of the <see cref="ContextMenuEx"/> class.
		/// </summary>
		static ContextMenuEx()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ContextMenuEx"/> class.
		/// </summary>
		public ContextMenuEx()
		{
		}

		/// <inheritdoc/>
		protected override System.Windows.DependencyObject GetContainerForItemOverride()
		{
			return new MenuItemEx();
		}

		/// <inheritdoc/>
		protected override void OnOpened(System.Windows.RoutedEventArgs e)
		{
			BindingOperations.GetBindingExpression(this, ItemsSourceProperty).UpdateTarget();

			base.OnOpened(e);
		}
	}
}