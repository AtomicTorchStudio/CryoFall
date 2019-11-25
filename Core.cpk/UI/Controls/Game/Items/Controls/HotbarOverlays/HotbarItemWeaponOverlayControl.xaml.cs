namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls.HotbarOverlays
{
    using System.Windows;
    using System.Windows.Input;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls.HotbarOverlays.Data;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls.Tooltips;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class HotbarItemWeaponOverlayControl : BaseUserControl
    {
        private readonly IItem item;

        private FrameworkElement tooltip;

        private ViewModelHotbarItemWeaponOverlayControl viewModel;

        public HotbarItemWeaponOverlayControl()
        {
        }

        public HotbarItemWeaponOverlayControl(IItem item)
        {
            this.item = item;
        }

        protected override void OnLoaded()
        {
            this.DataContext = this.viewModel = new ViewModelHotbarItemWeaponOverlayControl(
                                       ammoChangedCallback: this.RefreshTooltip)
                                   {
                                       Item = this.item
                                   };

            this.MouseEnter += this.MouseEnterOrLeaveHandler;
            this.MouseLeave += this.MouseEnterOrLeaveHandler;
        }

        protected override void OnUnloaded()
        {
            this.DataContext = null;
            this.viewModel.Dispose();
            this.viewModel = null;

            this.MouseEnter -= this.MouseEnterOrLeaveHandler;
            this.MouseLeave -= this.MouseEnterOrLeaveHandler;
        }

        private void DestroyTooltip()
        {
            if (this.tooltip == null)
            {
                return;
            }

            ToolTipServiceExtend.SetToolTip(this, null);
            this.tooltip = null;
        }

        private void MouseEnterOrLeaveHandler(object sender, MouseEventArgs e)
        {
            this.RefreshTooltip();
        }

        private void RefreshTooltip()
        {
            this.DestroyTooltip();

            if (!this.IsMouseOver)
            {
                return;
            }

            var protoItemAmmo = this.viewModel.ProtoItemAmmo;
            if (protoItemAmmo == null)
            {
                return;
            }

            this.tooltip = ItemTooltipControl.Create(protoItemAmmo);
            ToolTipServiceExtend.SetToolTip(this, this.tooltip);
        }
    }
}