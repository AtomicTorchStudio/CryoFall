namespace AtomicTorch.CBND.CoreMod.Items.Tools.Special
{
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Systems.VehicleRepairKitSystem;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Vehicle;
    using AtomicTorch.CBND.CoreMod.Vehicles;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using JetBrains.Annotations;

    [UsedImplicitly]
    public static class ClientItemVehicleRepairKitTooltipHelper
    {
        private static IDynamicWorldObject currentTooltipVehicle;

        private static IComponentAttachedControl tooltip;

        private static void Update()
        {
            IDynamicWorldObject vehicle = null;
            IProtoVehicle protoVehicle = null;

            if (ClientHotbarSelectedItemManager.SelectedItem?.ProtoItem
                    is IProtoItemVehicleRepairKit
                && ClientCurrentCharacterHelper.PublicState.CurrentPublicActionState
                    is not VehicleRepairActionState.PublicState)
            {
                vehicle = VehicleRepairKitSystem.ClientGetObjectToRepairAtCurrentMousePosition(
                    showErrorIfNoCompatibleVehicle: false);
                protoVehicle = vehicle?.ProtoGameObject as IProtoVehicle;
            }

            if (currentTooltipVehicle != vehicle)
            {
                tooltip?.Destroy();
                tooltip = null;
                currentTooltipVehicle = vehicle;
            }

            if (protoVehicle is null)
            {
                return;
            }

            tooltip ??= VehicleRepairKitTooltip.CreateAndAttach(vehicle);
        }

        private class Bootstrapper : BaseBootstrapper
        {
            public override void ClientInitialize()
            {
                ClientUpdateHelper.UpdateCallback += Update;
            }
        }
    }
}