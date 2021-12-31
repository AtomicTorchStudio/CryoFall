namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Crafting
{
    using System.Windows;
    using AtomicTorch.CBND.CoreMod.Items;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Crafting.Data;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class CraftingSkinNotOwnedControl : BaseUserControl
    {
        public static readonly DependencyProperty ProtoItemSkinProperty =
            DependencyProperty.Register(nameof(ProtoItemSkin),
                                        typeof(IProtoItemWithSkinData),
                                        typeof(CraftingSkinNotOwnedControl),
                                        new PropertyMetadata(default(IProtoItemWithSkinData),
                                                             ProtoItemSkinChangedHandler));

        private FrameworkElement layoutRoot;

        private ViewModelCraftingSkinNotOwnedControl viewModel;

        public IProtoItemWithSkinData ProtoItemSkin
        {
            get => (IProtoItemWithSkinData)this.GetValue(ProtoItemSkinProperty);
            set => this.SetValue(ProtoItemSkinProperty, value);
        }

        protected override void InitControl()
        {
            this.layoutRoot = this.GetByName<FrameworkElement>("LayoutRoot");
        }

        protected override void OnLoaded()
        {
            this.layoutRoot.DataContext = this.viewModel = new ViewModelCraftingSkinNotOwnedControl();
            Api.Client.Microtransactions.SkinsDataReceived += this.SkinsDataReceivedHandler;
            this.Refresh(force: false);
        }

        protected override void OnUnloaded()
        {
            this.layoutRoot.DataContext = null;
            this.viewModel.Dispose();
            this.viewModel = null;

            Api.Client.Microtransactions.SkinsDataReceived -= this.SkinsDataReceivedHandler;
        }

        private static void ProtoItemSkinChangedHandler(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((CraftingSkinNotOwnedControl)d).Refresh(force: false);
        }

        private void Refresh(bool force)
        {
            if (!this.isLoaded)
            {
                return;
            }

            if (force)
            {
                this.viewModel.ProtoItemSkin = null;
            }

            this.viewModel.ProtoItemSkin = this.ProtoItemSkin;
        }

        private void SkinsDataReceivedHandler()
        {
            this.Refresh(force: true);
        }
    }
}