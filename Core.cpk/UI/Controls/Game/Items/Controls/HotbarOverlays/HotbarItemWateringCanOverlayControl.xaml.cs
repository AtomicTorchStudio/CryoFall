namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls.HotbarOverlays
{
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls.HotbarOverlays.Data;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class HotbarItemWateringCanOverlayControl : BaseUserControl
    {
        private readonly IItem item;

        private ViewModelHotbarItemWateringCanOverlayControl viewModel;

        public HotbarItemWateringCanOverlayControl()
        {
        }

        public HotbarItemWateringCanOverlayControl(IItem item)
        {
            this.item = item;
        }

        protected override void OnLoaded()
        {
            this.DataContext = this.viewModel = new ViewModelHotbarItemWateringCanOverlayControl()
            {
                Item = this.item
            };
        }

        protected override void OnUnloaded()
        {
            this.DataContext = null;
            this.viewModel.Dispose();
            this.viewModel = null;
        }
    }
}