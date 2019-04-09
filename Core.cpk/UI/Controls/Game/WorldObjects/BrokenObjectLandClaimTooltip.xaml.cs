namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.LandClaim;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Data;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class BrokenObjectLandClaimTooltip : BaseUserControl
    {
        private ViewModelBrokenObjectLandClaimTooltip viewModel;

        public IStaticWorldObject ObjectLandClaim { get; set; }

        public ObjectLandClaimPublicState ObjectLandClaimPublicState { get; set; }

        protected override void OnLoaded()
        {
            this.viewModel =
                new ViewModelBrokenObjectLandClaimTooltip(this.ObjectLandClaim, this.ObjectLandClaimPublicState);
            this.DataContext = this.viewModel;
        }

        protected override void OnUnloaded()
        {
            this.DataContext = null;
            this.viewModel?.Dispose();
            this.viewModel = null;
        }
    }
}