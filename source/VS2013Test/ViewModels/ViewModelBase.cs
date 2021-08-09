using System.ComponentModel;

namespace AvalonDock.VS2013Test.ViewModels
{
	class ViewModelBase : INotifyPropertyChanged
	{

		protected virtual void RaisePropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}


		public event PropertyChangedEventHandler PropertyChanged;
	}
}
