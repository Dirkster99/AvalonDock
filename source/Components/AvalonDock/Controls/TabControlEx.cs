using System;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace AvalonDock.Controls
{
    /// <summary>
    /// This control added to mitigate issue with tab (document) switching speed
    /// See this https://stackoverflow.com/questions/2080764/how-to-preserve-control-state-within-tab-items-in-a-tabcontrol
    /// and this https://stackoverflow.com/questions/31030293/cefsharp-in-tabcontrol-not-working/37171847#37171847
    /// 
    /// by implmenting an option to enable virtualization for tabbed document containers.
    /// </summary>
    [TemplatePart(Name = "PART_ItemsHolder", Type = typeof(Panel))]
    public class TabControlEx : TabControl
    {
        #region fields
        private Panel ItemsHolderPanel = null;
        private readonly bool _IsVirtualizing;
        #endregion fields

        #region constructors
        /// <summary>
        /// Class constructor from virtualization parameter.
        /// </summary>
        /// <param name="isVirtualizing">Whether tabbed items are virtualized or not.</param>
        public TabControlEx(bool isVirtualizing)
            : this()
        {
            _IsVirtualizing = isVirtualizing;
        }

        /// <summary>
        /// Class constructor
        /// </summary>
        protected TabControlEx()
            : base()
        {
            // This is necessary so that we get the initial databound selected item
            ItemContainerGenerator.StatusChanged += ItemContainerGenerator_StatusChanged;
        }
        #endregion constructors

        #region methods
        /// <summary>
        /// If containers are done, generate the selected item
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ItemContainerGenerator_StatusChanged(object sender, EventArgs e)
        {
            if (this.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
            {
                this.ItemContainerGenerator.StatusChanged -= ItemContainerGenerator_StatusChanged;
                UpdateSelectedItem();
            }
        }

        /// <summary>
        /// Get the ItemsHolder and generate any children
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            ItemsHolderPanel = GetTemplateChild("PART_ItemsHolder") as Panel;
            UpdateSelectedItem();
        }

        /// <summary>
        /// When the items change we remove any generated panel children and add any new ones as necessary
        /// </summary>
        /// <param name="e"></param>
        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(e);

            if (ItemsHolderPanel == null)
                return;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Reset:
                    ItemsHolderPanel.Children.Clear();
                    break;

                case NotifyCollectionChangedAction.Add:
                case NotifyCollectionChangedAction.Remove:
                    if (e.OldItems != null)
                    {
                        foreach (var item in e.OldItems)
                        {
                            ContentPresenter cp = FindChildContentPresenter(item);
                            if (cp != null)
                                ItemsHolderPanel.Children.Remove(cp);
                        }
                    }

                    // Don't do anything with new items because we don't want to
                    // create visuals that aren't being shown

                    UpdateSelectedItem();
                    break;

                case NotifyCollectionChangedAction.Replace:
                    throw new NotImplementedException("Replace not implemented yet");
            }
        }

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            base.OnSelectionChanged(e);
            UpdateSelectedItem();
        }

        private void UpdateSelectedItem()
        {
            if (ItemsHolderPanel == null)
                return;

            // Generate a ContentPresenter if necessary
            TabItem item = GetSelectedTabItem();
            if (item != null)
                CreateChildContentPresenter(item);

            // show the right child
            foreach (ContentPresenter child in ItemsHolderPanel.Children)
                child.Visibility = ((child.Tag as TabItem).IsSelected) ? Visibility.Visible : Visibility.Collapsed;
        }

        private ContentPresenter CreateChildContentPresenter(object item)
        {
            if (item == null)
                return null;

            ContentPresenter cp = FindChildContentPresenter(item);

            if (cp != null)
                return cp;

            // the actual child to be added.  cp.Tag is a reference to the TabItem
            cp = new ContentPresenter();
            cp.Content = (item is TabItem) ? (item as TabItem).Content : item;
            cp.ContentTemplate = this.SelectedContentTemplate;
            cp.ContentTemplateSelector = this.SelectedContentTemplateSelector;
            cp.ContentStringFormat = this.SelectedContentStringFormat;
            cp.Visibility = Visibility.Collapsed;
            cp.Tag = (item is TabItem) ? item : (this.ItemContainerGenerator.ContainerFromItem(item));
            ItemsHolderPanel.Children.Add(cp);
            return cp;
        }

        private ContentPresenter FindChildContentPresenter(object data)
        {
            if (data is TabItem)
                data = (data as TabItem).Content;

            if (data == null)
                return null;

            if (ItemsHolderPanel == null)
                return null;

            foreach (ContentPresenter cp in ItemsHolderPanel.Children)
            {
                if (cp.Content == data)
                    return cp;
            }

            return null;
        }

        protected TabItem GetSelectedTabItem()
        {
            object selectedItem = base.SelectedItem;
            if (selectedItem == null)
                return null;

            TabItem item = selectedItem as TabItem;
            if (item == null)
                item = base.ItemContainerGenerator.ContainerFromIndex(base.SelectedIndex) as TabItem;

            return item;
        }
        #endregion methods
    }
}
