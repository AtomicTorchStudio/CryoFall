namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Vehicle.Data
{
    using AtomicTorch.CBND.CoreMod.ItemContainers.Vehicles;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Data;
    using AtomicTorch.CBND.CoreMod.Vehicles;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.World;

    public class ViewModelHUDMechHotbarControl : BaseViewModel
    {
        public ViewModelHUDMechHotbarControl(IDynamicWorldObject vehicle)
        {
            var protoVehicle = (IProtoVehicle)vehicle.ProtoGameObject;

            var structurePointsMax = protoVehicle.SharedGetStructurePointsMax(vehicle);
            this.ViewModelStructurePoints = new ViewModelStructurePointsBarControl()
            {
                ObjectStructurePointsData = new ObjectStructurePointsData(vehicle, structurePointsMax)
            };

            this.ViewModelVehicleEnergy = new ViewModelVehicleEnergy(vehicle);

            this.EquipmentItemsContainer = vehicle.GetPrivateState<VehicleMechPrivateState>()
                                                  .EquipmentItemsContainer;
        }

        public IItemsContainer EquipmentItemsContainer { get; }

        public bool HasSecondWeaponSlot
            => ((BaseItemsContainerMechEquipment)this.EquipmentItemsContainer.ProtoItemsContainer)
               .WeaponSlotsCount
               > 1;

        public ViewModelStructurePointsBarControl ViewModelStructurePoints { get; }

        public ViewModelVehicleEnergy ViewModelVehicleEnergy { get; }
    }
}