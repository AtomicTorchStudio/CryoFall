namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers
{
    using System;
    using AtomicTorch.CBND.CoreMod.ItemContainers;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Manufacturers;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Manufacturers.Data;
    using AtomicTorch.CBND.GameApi.Data.World;

    public abstract class ProtoObjectMulchbox
        <TPrivateState,
         TPublicState,
         TClientState>
        : ProtoObjectManufacturer
          <TPrivateState,
              TPublicState,
              TClientState>,
          IProtoObjectMulchbox
        where TPrivateState : ObjectMulchboxPrivateState, new()
        where TPublicState : ObjectMulchboxPublicState, new()
        where TClientState : StaticObjectClientState, new()
    {
        // no fuel is used by this manufacturer
        public override byte ContainerFuelSlotsCount => 0;

        public override byte ContainerInputSlotsCount => 1;

        public override byte ContainerOutputSlotsCount => 1;

        public override bool IsAutoSelectRecipe => true;

        public override bool IsFuelProduceByproducts => false;

        public abstract ushort OrganicCapacity { get; }

        public override double ServerUpdateIntervalSeconds => 0.25;

        public override double ServerUpdateRareIntervalSeconds => 2;

        public ObjectMulchboxPrivateState GetMulchboxPrivateState(IStaticWorldObject objectMulchbox)
        {
            return GetPrivateState(objectMulchbox);
        }

        protected override BaseUserControlWithWindow ClientOpenUI(ClientObjectData data)
        {
            return WindowMulchbox.Open(
                new ViewModelWindowMulchbox(
                    data.GameObject,
                    data.PrivateState,
                    this.ManufacturingConfig));
        }

        protected override void ServerInitialize(ServerInitializeData data)
        {
            base.ServerInitialize(data);
            // setup input container to allow only organic on input
            Server.Items.SetContainerType<ItemsContainerOrganics>(
                data.PrivateState.ManufacturingState.ContainerInput);
        }

        protected override void ServerUpdate(ServerUpdateData data)
        {
            this.ServerTryConsumeInputItem(data.PrivateState);
            base.ServerUpdate(data);
        }

        private void ServerTryConsumeInputItem(TPrivateState privateState)
        {
            var organicAmount = privateState.OrganicAmount;

            // try consume input item and add it's organic value into the mulchbox organic amount
            var inputItem = privateState.ManufacturingState.ContainerInput.GetItemAtSlot(0);
            if (inputItem is null
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

    public abstract class ProtoObjectMulchbox
        : ProtoObjectMulchbox
            <ObjectMulchboxPrivateState,
                ObjectMulchboxPublicState,
                StaticObjectClientState>
    {
    }
}