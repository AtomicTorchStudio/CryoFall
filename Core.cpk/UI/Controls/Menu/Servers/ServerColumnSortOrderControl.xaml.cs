namespace AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Servers
{
    using System.Windows;
    using System.Windows.Input;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Servers.Data;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class ServerColumnSortOrderControl : BaseUserControl
    {
        public static readonly DependencyProperty IsTargetOrderReversedProperty =
            DependencyProperty.Register(nameof(IsTargetOrderReversed),
                                        typeof(bool),
                                        typeof(ServerColumnSortOrderControl),
                                        new PropertyMetadata(default(bool)));

        public static readonly DependencyProperty TargetSortOrderProperty =
            DependencyProperty.Register(nameof(TargetSortOrder),
                                        typeof(ServersListSortType),
                                        typeof(ServerColumnSortOrderControl),
                                        new PropertyMetadata(default(ServersListSortType)));

        public static readonly DependencyProperty ViewModelServersListProperty =
            DependencyProperty.Register(nameof(ViewModelServersList),
                                        typeof(ViewModelServersList),
                                        typeof(ServerColumnSortOrderControl),
                                        new PropertyMetadata(default(ViewModelServersList),
                                                             ViewModelServersListPropertyChangedHandler));

        private bool isEventSubscribed;

        private ViewModelServersList lastViewModelServersList;

        private FrameworkElement layoutRoot;

        private ViewModelServerColumnSortOrderControl viewModel;

        public bool IsTargetOrderReversed
        {
            get => (bool)this.GetValue(IsTargetOrderReversedProperty);
            set => this.SetValue(IsTargetOrderReversedProperty, value);
        }

        public ServersListSortType TargetSortOrder
        {
            get => (ServersListSortType)this.GetValue(TargetSortOrderProperty);
            set => this.SetValue(TargetSortOrderProperty, value);
        }

        public ViewModelServersList ViewModelServersList
        {
            get => (ViewModelServersList)this.GetValue(ViewModelServersListProperty);
            set => this.SetValue(ViewModelServersListProperty, value);
        }

        protected override void InitControl()
        {
            this.layoutRoot = this.GetByName<FrameworkElement>("LayoutRoot");
        }

        protected override void OnLoaded()
        {
            this.layoutRoot.DataContext = this.viewModel = new ViewModelServerColumnSortOrderControl();
            this.Refresh();

            this.MouseLeftButtonUp += this.ClickHandler;
        }

        protected override void OnUnloaded()
        {
            this.layoutRoot.DataContext = null;
            this.viewModel?.Dispose();
            this.viewModel = null;

            this.MouseLeftButtonUp -= this.ClickHandler;

            this.UnsubscribeEvents();
            this.lastViewModelServersList = null;
        }

        private static void ViewModelServersListPropertyChangedHandler(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            ((ServerColumnSortOrderControl)d).Refresh();
        }

        private void ClickHandler(object sender, MouseButtonEventArgs e)
        {
            var viewModelServersList = this.ViewModelServersList;
            if (viewModelServersList.SortType != this.TargetSortOrder)
            {
                // select the target sort type
                viewModelServersList.SortType = this.TargetSortOrder;
                // ensure the sort order is direct
                viewModelServersList.IsSortOrderReversed = this.IsTargetOrderReversed;
                return;
            }

            // the target sort order is already selected, reverse the sort order
            viewModelServersList.IsSortOrderReversed = !viewModelServersList.IsSortOrderReversed;
        }

        private void ListSortTypeOrOrderChangedHander()
        {
            this.RefreshViewModel();
        }

        private void Refresh()
        {
            this.UnsubscribeEvents();
            this.lastViewModelServersList = null;

            if (!this.isLoaded)
            {
                return;
            }

            var viewModelServersList = this.ViewModelServersList;
            this.lastViewModelServersList = viewModelServersList;
            this.SubscribeEvents();

            this.RefreshViewModel();
        }

        private void RefreshViewModel()
        {
            var isSelected = this.lastViewModelServersList.SortType == this.TargetSortOrder;
            this.viewModel?.Refresh(
                isSelected,
                isReversed: isSelected
                            && (this.lastViewModelServersList.IsSortOrderReversed ^ this.IsTargetOrderReversed));
        }

        private void SubscribeEvents()
        {
            this.UnsubscribeEvents();

            if (this.lastViewModelServersList is not null)
            {
                this.lastViewModelServersList.SortTypeOrOrderChanged += this.ListSortTypeOrOrderChangedHander;
            }
        }

        private void UnsubscribeEvents()
        {
            if (!this.isEventSubscribed)
            {
                return;
            }

            this.isEventSubscribed = false;
            this.lastViewModelServersList.SortTypeOrOrderChanged -= this.ListSortTypeOrOrderChangedHander;
        }
    }
}