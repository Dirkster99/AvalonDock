namespace MLibTest.Demos
{
	using System.Windows;

	/// <summary>
	/// Implements an XAML proxy which can be used to bind items (TreeViewItem, ListViewItem etc)
	/// with a viewmodel that manages the collections.
	/// 
	/// Source: http://www.thomaslevesque.com/2011/03/21/wpf-how-to-bind-to-data-when-the-datacontext-is-not-inherited/
	///  Issue: http://stackoverflow.com/questions/9994241/mvvm-binding-command-to-contextmenu-item
	/// </summary>
	public class BindingProxy : Freezable
	{
		public static readonly DependencyProperty DataProperty =
			DependencyProperty.Register("Data", typeof(object), typeof(BindingProxy), new UIPropertyMetadata(null));

		/// <summary>
		/// Gets the data object this class is forwarding to everyone
		/// who has a reference to this object.
		/// </summary>
		public object Data
		{
			get => (object)this.GetValue(DataProperty);
			set => this.SetValue(DataProperty, value);
		}

		/// <summary>
		/// Overrides of Freezable
		/// </summary>
		/// <returns></returns>
		protected override Freezable CreateInstanceCore()
		{
			return new BindingProxy();
		}
	}
}
