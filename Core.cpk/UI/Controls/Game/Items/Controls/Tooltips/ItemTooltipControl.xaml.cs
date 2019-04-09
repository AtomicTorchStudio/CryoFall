namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls.Tooltips
{
    using System.Windows;
    using System.Windows.Controls;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls.Tooltips.Data;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class ItemTooltipControl : BaseUserControl
    {
        public static readonly DependencyProperty ProtoItemProperty =
            DependencyProperty.Register(nameof(ProtoItem),
                                        typeof(IProtoItem),
                                        typeof(ItemTooltipControl),
                                        new PropertyMetadata(default(IProtoItem), PropertyChangedHandler));

        public static readonly DependencyProperty ItemProperty =
            DependencyProperty.Register(nameof(Item),
                                        typeof(IItem),
                                        typeof(ItemTooltipControl),
                                        new PropertyMetadata(default(IItem), PropertyChangedHandler));

        private Grid layoutRoot;

        private ViewModelItemTooltip viewModel;

        public ItemTooltipControl()
        {
        }

        private ItemTooltipControl(IItem item, IProtoItem protoItem)
        {
            this.Item = item;
            this.ProtoItem = protoItem;
        }

        public IItem Item
        {
            get => (IItem)this.GetValue(ItemProperty);
            set => this.SetValue(ItemProperty, value);
        }

        public IProtoItem ProtoItem
        {
            get => (IProtoItem)this.GetValue(ProtoItemProperty);
            set => this.SetValue(ProtoItemProperty, value);
        }

        public static FrameworkElement Create(IItem item)
        {
            return new ItemTooltipControl(item, item.ProtoItem);
        }

        public static FrameworkElement Create(IProtoItem protoItem)
        {
            return new ItemTooltipControl(null, protoItem);
        }

        protected override void InitControl()
        {
            this.layoutRoot = this.GetByName<Grid>("LayoutRoot");
        }

        protected override void OnLoaded()
        {
            this.Refresh();
        }

        protected override void OnUnloaded()
        {
            this.DestroyViewModel();
        }

        private static void PropertyChangedHandler(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ItemTooltipControl)d).Refresh();
        }

        private void DestroyViewModel()
        {
            if (this.viewModel == null)
            {
                return;
            }

            this.layoutRoot.DataContext = null;
            this.viewModel?.Dispose();
            this.viewModel = null;
        }

        private void Refresh()
        {
            this.DestroyViewModel();
            if (!this.isLoaded)
            {
                return;
            }

            var item = this.Item;
            var protoItem = this.Item?.ProtoItem ?? this.ProtoItem;
            if (item == null
                && protoItem == null)
            {
                return;
            }

            this.viewModel = new ViewModelItemTooltip(item, protoItem);
            this.layoutRoot.DataContext = this.viewModel;
        }
    }
}