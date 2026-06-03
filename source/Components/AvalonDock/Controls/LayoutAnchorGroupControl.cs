using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using AvalonDock.Layout;

namespace AvalonDock.Controls
{
	/// <summary>
	/// Represents the layout Anchor Group Control.
	/// </summary>
	public class LayoutAnchorGroupControl : Control, ILayoutControl
	{
		private ObservableCollection<LayoutAnchorControl> _childViews = new ObservableCollection<LayoutAnchorControl>();
		private LayoutAnchorGroup _model;

		/// <summary>
		/// Initializes static members of the <see cref="LayoutAnchorGroupControl"/> class.
		/// </summary>
		static LayoutAnchorGroupControl()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(LayoutAnchorGroupControl), new FrameworkPropertyMetadata(typeof(LayoutAnchorGroupControl)));
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="LayoutAnchorGroupControl"/> class.
		/// </summary>
		/// <param name="model">The model.</param>
		internal LayoutAnchorGroupControl(LayoutAnchorGroup model)
		{
			_model = model;
			CreateChildrenViews();

			_model.Children.CollectionChanged += (s, e) => OnModelChildrenCollectionChanged(e);
		}

		/// <summary>
		/// Gets the children.
		/// </summary>
		public ObservableCollection<LayoutAnchorControl> Children
		{
			get
			{
				return _childViews;
			}
		}

		/// <summary>
		/// Gets the model.
		/// </summary>
		public ILayoutElement Model
		{
			get
			{
				return _model;
			}
		}

		private void CreateChildrenViews()
		{
			var manager = _model.Root.Manager;
			foreach (var childModel in _model.Children)
			{
				var lac = new LayoutAnchorControl(childModel);
				lac.SetBinding(LayoutAnchorControl.TemplateProperty, new Binding(DockingManager.AnchorTemplateProperty.Name) { Source = manager });
				_childViews.Add(lac);
			}
		}

		private void OnModelChildrenCollectionChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove ||
				e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Replace)
			{
				if (e.OldItems != null)
				{
					{
						foreach (var childModel in e.OldItems)
							_childViews.Remove(_childViews.First(cv => cv.Model == childModel));
					}
				}
			}

			if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
				_childViews.Clear();

			if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add ||
				e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Replace)
			{
				if (e.NewItems != null)
				{
					var manager = _model.Root.Manager;
					int insertIndex = e.NewStartingIndex;
					foreach (LayoutAnchorable childModel in e.NewItems)
					{
						var lac = new LayoutAnchorControl(childModel);
						lac.SetBinding(LayoutAnchorControl.TemplateProperty, new Binding(DockingManager.AnchorTemplateProperty.Name) { Source = manager });
						_childViews.Insert(insertIndex++, lac);
					}
				}
			}
		}
	}
}