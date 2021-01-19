namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls.Tooltips
{
    using System.Windows;
    using AtomicTorch.CBND.CoreMod.Items.Weapons;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls.Tooltips.Data;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class ItemTooltipCompatibleAmmoControl : BaseUserControl
    {
        public static readonly DependencyProperty ProtoItemWeaponProperty
            = DependencyProperty.Register(nameof(ProtoItemWeapon),
                                          typeof(IProtoItemWeapon),
                                          typeof(ItemTooltipCompatibleAmmoControl),
                                          new PropertyMetadata(null, ProtoItemWeaponPropertyChangedHandler));

        private FrameworkElement layoutRoot;

        private ViewModelItemTooltipCompatibleAmmoControl viewModel;

        public IProtoItemWeapon ProtoItemWeapon
        {
            get => (IProtoItemWeapon)this.GetValue(ProtoItemWeaponProperty);
            set => this.SetValue(ProtoItemWeaponProperty, value);
        }

        protected override void InitControl()
        {
            this.layoutRoot = this.GetByName<FrameworkElement>("LayoutRoot");
        }

        protected override void OnLoaded()
        {
            this.RefreshViewModel();
        }

        protected override void OnUnloaded()
        {
            this.layoutRoot.DataContext = null;
            this.viewModel?.Dispose();
            this.viewModel = null;
        }

        private static void ProtoItemWeaponPropertyChangedHandler(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var control = (ItemTooltipCompatibleAmmoControl)d;
            control.RefreshViewModel();
        }

        private void RefreshViewModel()
        {
            if (this.layoutRoot is null)
            {
                return;
            }

            this.layoutRoot.DataContext = null;
            this.viewModel?.Dispose();

            if (!this.isLoaded)
            {
                return;
            }

            if (this.ProtoItemWeapon is not null)
            {
                this.layoutRoot.DataContext
                    = this.viewModel
                          = new ViewModelItemTooltipCompatibleAmmoControl(this.ProtoItemWeapon);
            }
        }
    }
}