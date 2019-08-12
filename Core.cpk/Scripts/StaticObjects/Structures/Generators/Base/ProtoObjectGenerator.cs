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
          IInteractableProtoStaticWorldObject
        where TPrivateState : StructurePrivateState, new()
        where TPublicState : StaticObjectPublicState, IObjectPublicStateWithActiveFlag, new()
        where TClientState : StaticObjectClientState, new()
    {
        public int GenerationOrder { get; set; }

        public virtual bool IsAutoEnterPrivateScopeOnInteraction => true;

        public override double ServerUpdateIntervalSeconds => 0.2;

        public virtual void ServerOnClientInteract(ICharacter who, IStaticWorldObject worldObject)
        {
        }

        public virtual void ServerOnMenuClosed(ICharacter who, IStaticWorldObject worldObject)
        {
        }

        protected override ReadOnlySoundPreset<ObjectSound> PrepareSoundPresetObject()
        {
            return base.PrepareSoundPresetObject().Clone()
                       .Replace(ObjectSound.Active, "Objects/Structures/" + this.GetType().Name + "/Active");
        }

        public abstract void SharedGetElectricityProduction(
            IStaticWorldObject worldObject,
            out double currentProduction,
            out double maxProduction);

        BaseUserControlWithWindow IInteractableProtoStaticWorldObject.ClientOpenUI(IStaticWorldObject worldObject)
        {
            return this.ClientOpenUI(new ClientObjectData(worldObject));
        }

        protected override void ClientInitialize(ClientInitializeData data)
        {
            base.ClientInitialize(data);
            PowerGridSystem.ClientInitializeConsumerOrProducer(data.GameObject);
        }

        protected override void ClientInteractStart(ClientObjectData data)
        {
            InteractableStaticWorldObjectHelper.ClientStartInteract(data.GameObject);
        }

        protected abstract BaseUserControlWithWindow ClientOpenUI(ClientObjectData data);
    }

    public abstract class ProtoObjectGenerator
        : ProtoObjectGenerator
            <StructurePrivateState,
                ObjectGeneratorPublicState,
                StaticObjectClientState>
    {
    }
}