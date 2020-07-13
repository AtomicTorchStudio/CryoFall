namespace AtomicTorch.CBND.CoreMod.ItemContainers.Vehicles
{
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Vehicles;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.World;

    public class ItemsContainerFuelCores : ProtoItemsContainer
    {
        public override bool CanAddItem(CanAddItemContext context)
        {
            var protoItem = context.Item.ProtoItem;
            if (protoItem is IProtoItemFuelCell)
            {
                return true;
            }

            if (IsServer)
            {
                // server can place anything here
                return true;
            }

            return false;
        }

        public override void ServerOnItemAdded(IItemsContainer container, IItem item, ICharacter character)
        {
            ServerOnItemAddedOrRemoved(container, character);
        }

        public override void ServerOnItemRemoved(IItemsContainer container, IItem item, ICharacter character)
        {
            ServerOnItemAddedOrRemoved(container, character);
        }

        private static void ServerOnItemAddedOrRemoved(IItemsContainer container, ICharacter character)
        {
            if (character is null)
            {
                // this action is done by the server
                return;
            }

            // this action is done by the player so let's refresh the max energy level
            var vehicle = (IDynamicWorldObject)container.Owner;
            ((IProtoVehicle)vehicle.ProtoGameObject).ServerRefreshEnergyMax(vehicle);
        }
    }
}