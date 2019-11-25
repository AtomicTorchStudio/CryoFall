namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Vehicle
{
    using System;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.HUD;
    using AtomicTorch.CBND.GameApi.Data.World;

    public class ClientVehicleMechCurrentPlayerUIController : IDisposable
    {
        private readonly HUDMechHotbarControl controlMechHotbar;

        public ClientVehicleMechCurrentPlayerUIController(IDynamicWorldObject vehicle)
        {
            this.controlMechHotbar = new HUDMechHotbarControl(vehicle);
            HUDLayoutControl.Instance.GridHotbarChildren.Add(this.controlMechHotbar);
        }

        public void Dispose()
        {
            HUDLayoutControl.Instance?.GridHotbarChildren.Remove(this.controlMechHotbar);
        }
    }
}