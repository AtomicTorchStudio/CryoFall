namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls.HotbarOverlays
{
    using System.Windows;
    using System.Windows.Input;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls.HotbarOverlays.Data;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls.Tooltips;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class HotbarItemFishingRodOverlayControl : BaseUserControl
    {
        private readonly IItem item;

        private FrameworkElement tooltip;

        private ViewModelHotbarItemFishingRodOverlayControl viewModel;

        public HotbarItemFishingRodOverlayControl()
        {
        }

        public HotbarItemFishingRodOverlayControl(IItem item)
        {
            this.item = item;
        }

        protected override void InitControl()
        {
            this.DataContext = this.viewModel = new ViewModelHotbarItemFishingRodOverlayControl(
                                       baitChangedCallback: this.RefreshTooltip)
                                   {
                                       Item = this.item
                                   };
        }

        protected override void OnLoaded()
        {
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

            var protoItemBait = this.viewModel.ProtoItemBait;
            if (protoItemBait == null)
            {
                return;
            }

            this.tooltip = ItemTooltipControl.Create(protoItemBait);
            ToolTipServiceExtend.SetToolTip(this, this.tooltip);
        }
    }
}