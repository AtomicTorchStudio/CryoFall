namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Vehicle
{
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Vehicle.Data;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class HUDVehicleEnergyIndicator : BaseUserControl
    {
        protected override void InitControl()
        {
            this.DataContext = new ViewModelHUDVehicleEnergyIndicator();
        }
    }
}