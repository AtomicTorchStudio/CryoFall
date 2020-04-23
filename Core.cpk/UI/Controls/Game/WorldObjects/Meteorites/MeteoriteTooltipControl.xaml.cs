namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Meteorites
{
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Meteorites.Data;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class MeteoriteTooltipControl : BaseUserControl
    {
        private IStaticWorldObject lastWorldObjectMeteorite;

        private ViewModelMeteoriteTooltipControl viewModel;

        public void Setup(IStaticWorldObject worldObjectMeteorite)
        {
            this.lastWorldObjectMeteorite = worldObjectMeteorite;
            this.RefreshViewModel();
        }

        protected override void OnLoaded()
        {
            this.RefreshViewModel();
        }

        protected override void OnUnloaded()
        {
            this.RefreshViewModel();
        }

        private void RefreshViewModel()
        {
            if (!this.isLoaded
                || this.lastWorldObjectMeteorite == null)
            {
                if (this.viewModel == null)
                {
                    return;
                }

                this.DataContext = null;
                this.viewModel.Dispose();
                this.viewModel = null;
                return;
            }

            this.viewModel = new ViewModelMeteoriteTooltipControl(this.lastWorldObjectMeteorite);
            this.DataContext = this.viewModel;
        }
    }
}