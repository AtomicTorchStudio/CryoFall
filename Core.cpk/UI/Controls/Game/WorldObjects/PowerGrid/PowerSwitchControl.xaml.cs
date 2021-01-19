namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.PowerGrid
{
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.PowerGrid.Data;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class PowerSwitchControl : BaseUserControl
    {
        private static readonly TextureResource IconElectricityTextureResource
            = new("Icons/IconElectricity");

        private IStaticWorldObject worldObject;

        public Brush ElectricityIcon
            => Api.Client.UI.GetTextureBrush(IconElectricityTextureResource);

        public static PowerSwitchControl Create(IStaticWorldObject worldObject)
        {
            return new() { worldObject = worldObject };
        }

        protected override void OnLoaded()
        {
            if (this.worldObject is not null)
            {
                this.DataContext = new ViewModelPowerSwitch(this.worldObject);
            }
        }

        protected override void OnUnloaded()
        {
            if (this.worldObject is null)
            {
                return;
            }

            var viewModel = (ViewModelPowerSwitch)this.DataContext;
            this.DataContext = null;
            viewModel.Dispose();
        }
    }
}