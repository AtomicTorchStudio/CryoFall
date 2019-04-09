namespace AtomicTorch.CBND.CoreMod.Systems.Crafting
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.State.NetSync;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class FuelBurningState : BaseNetObject
    {
        [NonSerialized]
        public double FuelUseTimeAccumulatedRemainder;

        public FuelBurningState(
            IStaticWorldObject worldObject,
            byte containerFuelSlotsCount)
        {
            var itemsService = Api.Server.Items;
            this.ContainerFuel = itemsService.CreateContainer(worldObject, containerFuelSlotsCount);
        }

        [SyncToClient]
        public IItemsContainer ContainerFuel { get; }

        [TempOnly]
        public ushort? ContainerFuelLastStateHash { get; set; }

        [SyncToClient]
        public IProtoItemFuelSolid CurrentFuelItemType { get; set; }

        [SyncToClient(
            deliveryMode: DeliveryMode.ReliableSequenced,
            networkDataType: typeof(float),
            maxUpdatesPerSecond: ScriptingConstants.NetworkDefaultMaxUpdatesPerSecond)]
        public double FuelUseTimeRemainsSeconds { get; set; }

        public void SetSlotsCount(
            byte containerFuelSlotsCount)
        {
            Api.Server.Items.SetSlotsCount(this.ContainerFuel, containerFuelSlotsCount);
        }
    }
}