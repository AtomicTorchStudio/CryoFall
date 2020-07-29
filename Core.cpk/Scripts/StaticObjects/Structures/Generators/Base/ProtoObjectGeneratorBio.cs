namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Generators
{
    using System;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.ItemContainers;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Systems.PowerGridSystem;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.PowerGrid;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.PowerGrid.Data;
    using AtomicTorch.CBND.GameApi.Data.World;

    public abstract class ProtoObjectGeneratorBio
        : ProtoObjectGenerator
            <ObjectGeneratorBioPrivateState,
                ObjectGeneratorPublicState,
                StaticObjectClientState>
    {
        public abstract byte ContainerInputSlotsCount { get; }

        public abstract ushort OrganicCapacity { get; }

        public abstract double OrganicDecreasePerSecondWhenActive { get; }

        public override double ServerUpdateIntervalSeconds => 0.5;

        public override void ServerOnDestroy(IStaticWorldObject gameObject)
        {
            base.ServerOnDestroy(gameObject);

            ObjectGroundItemsContainer.ServerTryDropOnGroundContainerContent(
                gameObject.OccupiedTile,
                GetPrivateState(gameObject).InputItemsCointainer);
        }

        protected override BaseUserControlWithWindow ClientOpenUI(ClientObjectData data)
        {
            return WindowGeneratorBio.Open(
                new ViewModelWindowGeneratorBio(
                    data.GameObject,
                    data.PrivateState));
        }

        protected override void ServerInitialize(ServerInitializeData data)
        {
            base.ServerInitialize(data);

            var privateState = data.PrivateState;

            // setup input container to allow only organic on input
            var itemsContainer = privateState.InputItemsCointainer;
            var itemsSlotsCount = this.ContainerInputSlotsCount;
            if (itemsContainer != null)
            {
                // container already created - update slots count
                Server.Items.SetSlotsCount(itemsContainer, slotsCount: itemsSlotsCount);
                return;
            }

            itemsContainer = Server.Items.CreateContainer<ItemsContainerOrganics>(
                owner: data.GameObject,
                slotsCount: itemsSlotsCount);

            privateState.InputItemsCointainer = itemsContainer;
        }

        protected override void ServerUpdate(ServerUpdateData data)
        {
            var privateState = data.PrivateState;
            var publicState = data.PublicState;

            if (publicState.IsActive)
            {
                privateState.OrganicAmount -= (float)(this.OrganicDecreasePerSecondWhenActive * data.DeltaTime);
            }

            this.ServerTryConsumeInputItem(privateState);

            publicState.IsActive = publicState.ElectricityProducerState == ElectricityProducerState.PowerOnActive
                                   && privateState.OrganicAmount > 0;
        }

        private void ServerTryConsumeInputItem(ObjectGeneratorBioPrivateState privateState)
        {
            var organicAmount = privateState.OrganicAmount;

            // try consume input item and add it's organic value into the mulchbox organic amount
            var inputItem = privateState.InputItemsCointainer.Items.FirstOrDefault();
            if (inputItem == null
                || !(inputItem.ProtoItem is IProtoItemOrganic protoItemOrganic))
            {
                return;
            }

            var count = inputItem.Count;
            while (count > 0)
            {
                if (organicAmount > 0
                    && organicAmount + protoItemOrganic.OrganicValue > this.OrganicCapacity)
                {
                    // don't add this item - it will lead to over capacity
                    break;
                }

                organicAmount += protoItemOrganic.OrganicValue;
                count--;
            }

            organicAmount = Math.Min(organicAmount, this.OrganicCapacity);
            privateState.OrganicAmount = organicAmount;

            if (count != inputItem.Count)
            {
                Server.Items.SetCount(inputItem, count);
            }
        }
    }
}