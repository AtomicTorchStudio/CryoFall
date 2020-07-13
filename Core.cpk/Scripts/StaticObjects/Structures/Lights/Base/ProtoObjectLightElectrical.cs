namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Lights
{
    using AtomicTorch.CBND.CoreMod.ItemContainers;
    using AtomicTorch.CBND.CoreMod.Systems.PowerGridSystem;
    using AtomicTorch.CBND.GameApi.Data.World;

    public abstract class ProtoObjectLightElectrical
        <TPrivateState,
         TPublicState,
         TClientState>
        : ProtoObjectLight
          <TPrivateState,
              TPublicState,
              TClientState>,
          IProtoObjectElectricityConsumerWithCustomRate
        where TPrivateState : ObjectLightWithElectricityPrivateState, new()
        where TPublicState : ObjectLightWithElectricityPublicState, new()
        where TClientState : ObjectLightClientState, new()
    {
        public override byte ContainerInputSlotsCount => 0;

        public virtual ElectricityThresholdsPreset DefaultConsumerElectricityThresholds
            => new ElectricityThresholdsPreset(startupPercent: 20,
                                               shutdownPercent: 10);

        public abstract double ElectricityConsumptionPerSecondWhenActive { get; }

        public double SharedGetCurrentElectricityConsumptionRate(IStaticWorldObject worldObject)
        {
            return GetPublicState(worldObject).IsLightActive
                       ? 1
                       : 0;
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

        protected sealed override void PrepareFuelConfig(
            out double fuelCapacity,
            out double fuelAmountInitial,
            out double fuelUsePerSecond,
            out IFuelItemsContainer fuelContainerPrototype)
        {
            // don't use any fuel as this building operates on the electricity
            fuelCapacity = 0;
            fuelAmountInitial = 0;
            fuelUsePerSecond = 0;
            fuelContainerPrototype = GetProtoEntity<ItemsContainerFuelElectricity>();
        }

        protected override bool ServerCheckCanLight(IStaticWorldObject lightObject, double fuelAmount)
        {
            return GetPublicState(lightObject).ElectricityConsumerState
                   == ElectricityConsumerState.PowerOnActive;
        }

        protected override void ServerTryConsumeFuelItem(TPrivateState privateState)
        {
            // no fuel consumption
        }
    }

    public abstract class ProtoObjectLightElectrical
        : ProtoObjectLightElectrical
            <ObjectLightWithElectricityPrivateState,
                ObjectLightWithElectricityPublicState,
                ObjectLightClientState>
    {
    }
}