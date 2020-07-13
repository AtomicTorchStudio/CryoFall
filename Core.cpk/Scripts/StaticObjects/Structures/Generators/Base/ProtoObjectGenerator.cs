namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Generators
{
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers;
    using AtomicTorch.CBND.CoreMod.Systems.PowerGridSystem;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.World;

    public abstract class ProtoObjectGenerator
        <TPrivateState,
         TPublicState,
         TClientState>
        : ProtoObjectStructure
          <TPrivateState,
              TPublicState,
              TClientState>,
          IProtoObjectElectricityProducer,
          IInteractableProtoWorldObject
        where TPrivateState : ObjectGeneratorPrivateState, new()
        where TPublicState : StaticObjectPublicState, IObjectPublicStateWithActiveFlag,
        IObjectElectricityProducerPublicState, new()
        where TClientState : StaticObjectClientState, new()
    {
        public abstract ElectricityThresholdsPreset DefaultGenerationElectricityThresholds { get; }

        public int GenerationOrder { get; set; }

        public virtual bool IsAutoEnterPrivateScopeOnInteraction => true;

        public override bool IsRelocatable => true;

        public override double ServerUpdateIntervalSeconds => 0.2;

        public virtual void ServerOnClientInteract(ICharacter who, IWorldObject worldObject)
        {
        }

        public virtual void ServerOnMenuClosed(ICharacter who, IWorldObject worldObject)
        {
        }

        public abstract void SharedGetElectricityProduction(
            IStaticWorldObject worldObject,
            out double currentProduction,
            out double maxProduction);

        BaseUserControlWithWindow IInteractableProtoWorldObject.ClientOpenUI(IWorldObject worldObject)
        {
            return this.ClientOpenUI(new ClientObjectData((IStaticWorldObject)worldObject));
        }

        IObjectElectricityStructurePrivateState IProtoObjectElectricityProducer.GetPrivateState(
            IStaticWorldObject worldObject)
        {
            return GetPrivateState(worldObject);
        }

        IObjectElectricityProducerPublicState IProtoObjectElectricityProducer.GetPublicState(
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

        protected abstract BaseUserControlWithWindow ClientOpenUI(ClientObjectData data);

        protected override ReadOnlySoundPreset<ObjectSound> PrepareSoundPresetObject()
        {
            return base.PrepareSoundPresetObject().Clone()
                       .Replace(ObjectSound.Active, "Objects/Structures/" + this.GetType().Name + "/Active");
        }
    }

    public abstract class ProtoObjectGenerator
        : ProtoObjectGenerator
            <ObjectGeneratorPrivateState,
                ObjectGeneratorPublicState,
                StaticObjectClientState>
    {
    }
}