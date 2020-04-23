namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Fridges
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Crates;
    using AtomicTorch.CBND.CoreMod.Systems.PowerGridSystem;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public abstract class ProtoObjectFridgeElectrical
        <TPrivateState,
         TPublicState,
         TClientState>
        : ProtoObjectFridge
          <TPrivateState,
              TPublicState,
              TClientState>,
          IProtoObjectElectricityConsumer
        where TPrivateState : ObjectCratePrivateState, new()
        where TPublicState : ElectricalFridgePublicState, new()
        where TClientState : StaticObjectClientState, new()
    {
        public abstract double ElectricityConsumptionPerSecondWhenActive { get; }

        public override double ServerGetCurrentFreshnessDurationMultiplier(IStaticWorldObject worldObject)
        {
            var publicState = GetPublicState(worldObject);
            if (publicState.ElectricityConsumerState
                != ElectricityConsumerState.PowerOn)
            {
                // no power supplied so no freshness increase
                return 1;
            }

            return this.FreshnessDurationMultiplier;
        }

        protected override void ClientInitialize(ClientInitializeData data)
        {
            base.ClientInitialize(data);

            PowerGridSystem.ClientInitializeConsumerOrProducer(data.GameObject);

            // create sound emitter
            var soundEmitter = Client.Audio.CreateSoundEmitter(
                data.GameObject,
                new SoundResource("Objects/Structures/ObjectFridge/Active"),
                isLooped: true,
                volume: 0.35f,
                radius: 1f);
            soundEmitter.CustomMaxDistance = 3.5f;
            this.ClientSetupSoundEmitter(soundEmitter);

            var publicState = data.PublicState;
            publicState.ClientSubscribe(_ => _.ElectricityConsumerState,
                                        _ => RefreshSoundEmitterState(),
                                        data.ClientState);

            RefreshSoundEmitterState();

            void RefreshSoundEmitterState()
            {
                soundEmitter.IsEnabled = publicState.ElectricityConsumerState
                                         == ElectricityConsumerState.PowerOn;
            }
        }

        protected virtual void ClientSetupSoundEmitter(IComponentSoundEmitter soundEmitter)
        {
        }
    }

    public abstract class ProtoObjectFridgeElectrical
        : ProtoObjectFridgeElectrical<
            ObjectCratePrivateState,
            ElectricalFridgePublicState,
            StaticObjectClientState>
    {
    }
}