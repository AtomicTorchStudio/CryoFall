namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Misc
{
    using System;
    using AtomicTorch.CBND.CoreMod.ItemContainers;
    using AtomicTorch.CBND.CoreMod.Items;
    using AtomicTorch.CBND.CoreMod.Items.Devices;
    using AtomicTorch.CBND.CoreMod.Systems.PowerGridSystem;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.PowerGrid;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.PowerGrid.Data;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;

    public abstract class ProtoObjectRechargingStation
        : ProtoObjectStructure
          <ProtoObjectRechargingStation.PrivateState,
              StaticObjectElectricityConsumerPublicState,
              StaticObjectClientState>,
          IInteractableProtoWorldObject,
          IProtoObjectElectricityConsumerWithCustomRate
    {
        public abstract byte ContainerInputSlotsCount { get; }

        public virtual ElectricityThresholdsPreset DefaultConsumerElectricityThresholds
            => new(startupPercent: 20,
                   shutdownPercent: 10);

        public double ElectricityConsumptionPerSecondWhenActive { get; private set; }

        public bool IsAutoEnterPrivateScopeOnInteraction => true;

        public override bool IsRelocatable => true;

        /// <summary>
        /// Recharging speed in EU/s (per slot).
        /// </summary>
        public abstract double RechargePerSecondPerSlot { get; }

        public override double ServerUpdateIntervalSeconds => 5;

        public override void ServerOnDestroy(IStaticWorldObject gameObject)
        {
            base.ServerOnDestroy(gameObject);

            var privateState = GetPrivateState(gameObject);
            ObjectGroundItemsContainer.ServerTryDropOnGroundContainerContent(
                gameObject.OccupiedTile,
                privateState.ItemsContainer);
        }

        public virtual double SharedGetCurrentElectricityConsumptionRate(IStaticWorldObject worldObject)
        {
            var count = 0;
            var items = GetPrivateState(worldObject).ItemsContainer.Items;
            foreach (var item in items)
            {
                if (SharedCanRechargePowerBank(item)
                    || SharedCanRechargeItemWithFuel(item))
                {
                    count++;
                }
            }

            var consumptionRate = count / (double)this.ContainerInputSlotsCount;
            return consumptionRate;
        }

        BaseUserControlWithWindow IInteractableProtoWorldObject.ClientOpenUI(IWorldObject worldObject)
        {
            var staticWorldObject = (IStaticWorldObject)worldObject;
            var privateState = GetPrivateState(staticWorldObject);
            return this.ClientOpenUI(staticWorldObject, privateState);
        }

        void IInteractableProtoWorldObject.ServerOnClientInteract(ICharacter who, IWorldObject worldObject)
        {
        }

        void IInteractableProtoWorldObject.ServerOnMenuClosed(ICharacter who, IWorldObject worldObject)
        {
        }

        IObjectElectricityStructurePrivateState IProtoObjectElectricityConsumer.GetPrivateState(
            IStaticWorldObject worldObject)
        {
            return GetPrivateState(worldObject);
        }

        IObjectElectricityConsumerPublicState IProtoObjectElectricityConsumer.GetPublicState(
            IStaticWorldObject worldObject)
        {
            return GetPublicState(worldObject);
        }

        protected override void ClientInitialize(ClientInitializeData data)
        {
            base.ClientInitialize(data);
            PowerGridSystem.ClientInitializeConsumerOrProducer(data.GameObject);
        }

        protected override void ClientInteractStart(ClientObjectData data)
        {
            InteractableWorldObjectHelper.ClientStartInteract(data.GameObject);
        }

        protected virtual BaseUserControlWithWindow ClientOpenUI(
            IStaticWorldObject worldObject,
            PrivateState privateState)
        {
            return WindowRechargingStation.Open(
                new ViewModelWindowRechargingStation(privateState));
        }

        protected virtual void PrepareProtoObjectRechargingStation()
        {
        }

        protected sealed override void PrepareProtoStaticWorldObject()
        {
            base.PrepareProtoStaticWorldObject();

            this.ElectricityConsumptionPerSecondWhenActive =
                this.RechargePerSecondPerSlot * this.ContainerInputSlotsCount;

            this.PrepareProtoObjectRechargingStation();
        }

        protected override void ServerInitialize(ServerInitializeData data)
        {
            base.ServerInitialize(data);

            var privateState = data.PrivateState;

            // setup input container to allow only power banks on input
            var itemsContainer = privateState.ItemsContainer;
            var itemsSlotsCount = this.ContainerInputSlotsCount;
            if (itemsContainer is not null)
            {
                // container already created - update slots count
                Server.Items.SetSlotsCount(itemsContainer, slotsCount: itemsSlotsCount);
                return;
            }

            itemsContainer = Server.Items.CreateContainer<ItemsContainerPowerBanks>(
                owner: data.GameObject,
                slotsCount: itemsSlotsCount);

            privateState.ItemsContainer = itemsContainer;
        }

        protected override void ServerUpdate(ServerUpdateData data)
        {
            base.ServerUpdate(data);

            if (data.PublicState.ElectricityConsumerState != ElectricityConsumerState.PowerOnActive)
            {
                return;
            }

            var items = GetPrivateState(data.GameObject).ItemsContainer.Items;
            var deltaTime = data.DeltaTime;
            foreach (var item in items)
            {
                this.ServerTryRechargePowerBank(item, deltaTime);
                this.ServerTryRechargeItemWithFuel(item, deltaTime);
            }
        }

        private static bool SharedCanRechargeItemWithFuel(IItem item)
        {
            if (!(item.ProtoItem is IProtoItemWithFuel protoItemWithFuel))
            {
                return false;
            }

            var fuelConfig = protoItemWithFuel.ItemFuelConfig;
            if (!fuelConfig.IsElectricity)
            {
                return false;
            }

            var capacity = fuelConfig.FuelCapacity;
            if (capacity <= 0)
            {
                return false;
            }

            var itemPrivateState = item.GetPrivateState<ItemWithFuelPrivateState>();
            var charge = itemPrivateState.FuelAmount;
            return charge < capacity;
        }

        private static bool SharedCanRechargePowerBank(IItem item)
        {
            if (!(item.ProtoItem is IProtoItemPowerBank protoPowerBank))
            {
                return false;
            }

            var privateState = item.GetPrivateState<ItemPowerBankPrivateState>();
            return privateState.EnergyCharge < protoPowerBank.EnergyCapacity;
        }

        private void ServerTryRechargeItemWithFuel(IItem item, double deltaTime)
        {
            if (!(item.ProtoItem is IProtoItemWithFuel protoItemWithFuel))
            {
                return;
            }

            var fuelConfig = protoItemWithFuel.ItemFuelConfig;
            if (!fuelConfig.IsElectricity)
            {
                return;
            }

            var capacity = fuelConfig.FuelCapacity;
            if (capacity <= 0)
            {
                return;
            }

            var privateState = item.GetPrivateState<ItemWithFuelPrivateState>();
            var charge = privateState.FuelAmount;
            if (charge >= capacity)
            {
                return;
            }

            // recharge this item
            var deltaCharge = deltaTime * this.RechargePerSecondPerSlot;
            fuelConfig.SharedOnRefilled(item,
                                        newFuelAmount: privateState.FuelAmount + deltaCharge,
                                        serverNotifyClients: true);
        }

        private void ServerTryRechargePowerBank(IItem item, double deltaTime)
        {
            if (!(item.ProtoItem is IProtoItemPowerBank protoPowerBank))
            {
                return;
            }

            var itemPrivateState = item.GetPrivateState<ItemPowerBankPrivateState>();
            var capacity = protoPowerBank.EnergyCapacity;
            var charge = itemPrivateState.EnergyCharge;
            if (charge >= capacity)
            {
                return;
            }

            // recharge this power bank
            var deltaCharge = deltaTime * this.RechargePerSecondPerSlot;
            charge += deltaCharge;
            charge = Math.Min(charge, capacity);

            itemPrivateState.EnergyCharge = charge;
        }

        public class PrivateState : StructurePrivateState, IObjectElectricityStructurePrivateState
        {
            [SyncToClient]
            public ElectricityThresholdsPreset ElectricityThresholds { get; set; }

            [SyncToClient]
            public IItemsContainer ItemsContainer { get; set; }

            [SyncToClient]
            [TempOnly]
            public byte PowerGridChargePercent { get; set; }
        }
    }
}