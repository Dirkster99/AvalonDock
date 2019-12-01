namespace MLibTest.ViewModels.Base
{
	using System;
	using System.ComponentModel;
	using System.Linq.Expressions;

	/// <summary>
	/// Implements a base class for all viewmodel classes
	/// that implements <seealso cref="INotifyPropertyChanged"/> interface for binding.
	/// </summary>
	internal class ViewModelBase : ModelBase, INotifyPropertyChanged
	{
		/// <summary>
		/// Standard implementation of <seealso cref="INotifyPropertyChanged"/>.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Tell bound controls (via WPF binding) to refresh their display.
		/// 
		/// Sample call: this.NotifyPropertyChanged(() => this.IsSelected);
		/// where 'this' is derived from <seealso cref="ViewModelBase"/>
		/// and IsSelected is a property.
		/// </summary>
		/// <typeparam name="TProperty"></typeparam>
		/// <param name="property"></param>
		public void RaisePropertyChanged<TProperty>(Expression<Func<TProperty>> property)
		{
			var lambda = (LambdaExpression)property;
			MemberExpression memberExpression;

			if (lambda.Body is UnaryExpression)
			{
				var unaryExpression = (UnaryExpression)lambda.Body;
				memberExpression = (MemberExpression)unaryExpression.Operand;
			}
			else
				memberExpression = (MemberExpression)lambda.Body;

			this.RaisePropertyChanged(memberExpression.Member.Name);
		}

		/// <summary>
		/// Tell bound controls (via WPF binding) to refresh their display.
		/// Standard implementation through <seealso cref="INotifyPropertyChanged"/>.
		/// </summary>
		/// <param name="propertyName"></param>
		protected virtual void RaisePropertyChanged(string propertyName)
		{
			if (this.PropertyChanged != null)
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
