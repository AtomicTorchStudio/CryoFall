namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls.SlotOverlays
{
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls.SlotOverlays.Data;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class ItemSlotMedicineCooldownOverlayControl : BaseUserControl
    {
        private ViewModelItemSlotMedicineCooldownOverlayControl viewModel;

        protected override void InitControl()
        {
            this.DataContext = this.viewModel = new ViewModelItemSlotMedicineCooldownOverlayControl();
        }

        protected override void OnUnloaded()
        {
            this.DataContext = null;
            this.viewModel.Dispose();
            this.viewModel = null;
        }
    }
}