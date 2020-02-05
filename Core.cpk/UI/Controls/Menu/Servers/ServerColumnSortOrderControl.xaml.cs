namespace AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Servers
{
    using System.Windows;
    using System.Windows.Input;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Servers.Data;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class ServerColumnSortOrderControl : BaseUserControl
    {
        public static readonly DependencyProperty IsTargetOrderReversedProperty =
            DependencyProperty.Register(nameof(IsTargetOrderReversed),
                                        typeof(bool),
                                        typeof(ServerColumnSortOrderControl),
                                        new PropertyMetadata(default(bool)));

        public static readonly DependencyProperty CurrentSortOrderProperty =
            DependencyProperty.Register(nameof(CurrentSortOrder),
                                        typeof(ServersListSortType),
                                        typeof(ServerColumnSortOrderControl),
                                        new PropertyMetadata(default(ServersListSortType),
                                                             AnySortPropertyChangedCallback));

        public static readonly DependencyProperty IsCurrentSortOrderReversedProperty =
            DependencyProperty.Register("IsCurrentSortOrderReversed",
                                        typeof(bool),
                                        typeof(ServerColumnSortOrderControl),
                                        new PropertyMetadata(default(bool),
                                                             AnySortPropertyChangedCallback));

        public static readonly DependencyProperty TargetSortOrderProperty =
            DependencyProperty.Register(nameof(TargetSortOrder),
                                        typeof(ServersListSortType),
                                        typeof(ServerColumnSortOrderControl),
                                        new PropertyMetadata(default(ServersListSortType)));

        private FrameworkElement layoutRoot;

        private ViewModelServerColumnSortOrderControl viewModel;

        public ServersListSortType CurrentSortOrder
        {
            get => (ServersListSortType)this.GetValue(CurrentSortOrderProperty);
            set => this.SetValue(CurrentSortOrderProperty, value);
        }

        public bool IsCurrentSortOrderReversed
        {
            get => (bool)this.GetValue(IsCurrentSortOrderReversedProperty);
            set => this.SetValue(IsCurrentSortOrderReversedProperty, value);
        }

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
        }

        private static void AnySortPropertyChangedCallback(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            ((ServerColumnSortOrderControl)d).Refresh();
        }

        private void ClickHandler(object sender, MouseButtonEventArgs e)
        {
            if (this.CurrentSortOrder != this.TargetSortOrder)
            {
                // select the target sort type
                this.SetCurrentValue(CurrentSortOrderProperty, this.TargetSortOrder);
                // ensure the sort order is direct
                this.SetCurrentValue(IsCurrentSortOrderReversedProperty, false);
                return;
            }

            // the target sort order is already selected, reverse the sort order
            this.SetCurrentValue(IsCurrentSortOrderReversedProperty, !this.IsCurrentSortOrderReversed);
        }

        private void Refresh()
        {
            var isSelected = this.CurrentSortOrder == this.TargetSortOrder;
            this.viewModel?.Refresh(
                isSelected,
                isReversed: isSelected
                            && (this.IsCurrentSortOrderReversed ^ this.IsTargetOrderReversed));
        }
    }
}