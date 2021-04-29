namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Vehicle.Data
{
    using AtomicTorch.CBND.CoreMod.Systems.VehicleNamesSystem;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public class ViewModelVehicleNameEditorControl : BaseViewModel
    {
        private readonly IDynamicWorldObject vehicle;

        public ViewModelVehicleNameEditorControl(IDynamicWorldObject vehicle)
        {
            this.vehicle = vehicle;
            this.Name = VehicleNamesSystem.ClientTryGetVehicleName(vehicle.Id)
                        ?? string.Empty;
        }

        public BaseCommand CommandSave
            => new ActionCommand(this.ExecuteCommandSave);

        public string Name { get; set; }

        public int NameMaxLength => VehicleNamesSystem.NameLengthMax;

        public string NameRequirements
            => string.Format(CoreStrings.VehicleNameEditor_NameRequirements_Format,
                             VehicleNamesSystem.NameLengthMin,
                             VehicleNamesSystem.NameLengthMax);

        private void ExecuteCommandSave()
        {
            VehicleNamesSystem.ClientSetVehicleName(this.vehicle, this.Name);
            Client.UI.BlurFocus();
        }
    }
}