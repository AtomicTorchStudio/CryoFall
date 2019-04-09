namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.HUD
{
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.HUD.Data;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class HUDEnergyChargeIndicator : BaseUserControl
    {
        protected override void InitControl()
        {
            this.DataContext = new ViewModelHUDEnergyChargeIndicator();
        }
    }
}