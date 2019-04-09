namespace AtomicTorch.CBND.CoreMod.UI.Controls.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Media;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public class TabControlCached : BaseSelector
    {
        public static readonly DependencyProperty ForceContentPreloadingProperty
            = DependencyProperty.Register(nameof(ForceContentPreloading),
                                          typeof(bool),
                                          typeof(TabControlCached),
                                          // force content preloading in non-Editor only by default
                                          new PropertyMetadata(defaultValue: !Api.IsEditor));

        private readonly Dictionary<TabItem, FrameworkElement> tabContentPresenters
            = new Dictionary<TabItem, FrameworkElement>();

        private Grid contentGrid;

        private UIElementCollection contentGridChildren;

        private ItemCollection items;

        static TabControlCached()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(TabControlCached),
                new FrameworkPropertyMetadata(typeof(TabControlCached)));
        }

        public bool ForceContentPreloading
        {
            get => (bool)this.GetValue(ForceContentPreloadingProperty);
            set => this.SetValue(ForceContentPreloadingProperty, value);
        }

        public void AddItem(TabItem tabItem)
        {
            this.items.Insert(0, tabItem);
            this.ShowSelectedItem();
        }

        public void RefreshItems()
        {
            // cleanup old items
            this.contentGridChildren.Clear();

            foreach (var contentPresenter in this.tabContentPresenters.Values)
            {
                DisposeContentPresenter(contentPresenter);
            }

            this.tabContentPresenters.Clear();

            this.ShowSelectedItem();
        }

        public void Remove(TabItem tabItem)
        {
            this.items.Remove(tabItem);
            if (!this.tabContentPresenters.TryGetValue(tabItem, out var contentPresenter))
            {
                return;
            }

            DisposeContentPresenter(contentPresenter);
            this.tabContentPresenters.Remove(tabItem);

            this.ShowSelectedItem();
        }

        public void ShowSelectedItem()
        {
            var selectedIndex = this.SelectedIndex;
            var count = this.items.Count;

            if (this.ForceContentPreloading)
            {
                // pre-load all items
                for (var index = 0; index < count; index++)
                {
                    this.EnsureTabContentPresenterExist(index);
                }
            }

            for (var index = 0; index < count; index++)
            {
                var tabItem = (TabItem)this.items.GetItemAt(index);
                var isSelected = index == selectedIndex;

                this.tabContentPresenters.TryGetValue(tabItem, out var tabContentPresenter);
                if (tabContentPresenter == null
                    && isSelected)
                {
                    // need to create the content
                    tabContentPresenter = this.EnsureTabContentPresenterExist(index);
                }

                if (tabContentPresenter != null)
                {
                    tabContentPresenter.Visibility = isSelected
                                                         ? Visibility.Visible
                                                         : Visibility.Collapsed;
                }
            }
        }

        public void SortTabs(Comparison<TabItem> comparer)
        {
            var count = this.items.Count;

            var list = new List<TabItem>(count);
            for (var index = 0; index < count; index++)
            {
                list.Add((TabItem)this.items.GetItemAt(index));
            }

            var sortedList = list.ToList();
            sortedList.Sort(comparer);

            if (list.SequenceEqual(sortedList))
            {
                return;
            }

            var selectedItem = this.SelectedItem;
            this.items.Clear();

            foreach (var tabItem in sortedList)
            {
                this.items.Add(tabItem);
            }

            this.SelectedItem = selectedItem;
            this.ShowSelectedItem();
        }

        protected override void InitControl()
        {
            var templateRoot = (FrameworkElement)VisualTreeHelper.GetChild(this, 0);
            this.contentGrid = templateRoot.GetByName<Grid>("ContentGrid");
            this.contentGridChildren = this.contentGrid.Children;

            this.items = this.Items;

            this.RefreshItems();
        }

        protected override void OnLoaded()
        {
            this.SelectionChanged += this.SelectionChangedHandler;
            this.ShowSelectedItem();
        }

        protected override void OnUnloaded()
        {
            this.SelectionChanged -= this.SelectionChangedHandler;
        }

        private static void DisposeContentPresenter(FrameworkElement contentPresenter)
        {
            if (contentPresenter != null)
            {
                BindingOperations.ClearAllBindings(contentPresenter);
            }
        }

        private FrameworkElement EnsureTabContentPresenterExist(int index)
        {
            var tabItem = (TabItem)this.items.GetItemAt(index);
            if (this.tabContentPresenters.TryGetValue(tabItem, out var contentPresenter))
            {
                return contentPresenter;
            }

            var content = (FrameworkElement)tabItem.Content;
            if (content == null)
            {
                return null;
            }

            // create content presenter for the content of the tab item
            contentPresenter = new ContentPresenter();

            // bind the content of tab item to the content presenter content property
            BindingOperations.SetBinding(
                contentPresenter,
                ContentPresenter.ContentProperty,
                new Binding(ContentControl.ContentProperty.Name)
                {
                    Source = tabItem,
                    Mode = BindingMode.OneWay
                });

            this.contentGridChildren.Add(contentPresenter);
            this.tabContentPresenters.Add(tabItem, contentPresenter);
            return content;
        }

        private void SelectionChangedHandler(object sender, SelectionChangedEventArgs e)
        {
            this.ShowSelectedItem();
        }
    }
}