namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.PowerStorage
{
    using AtomicTorch.CBND.CoreMod.Systems.PowerGridSystem;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.PowerGrid;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.PowerGrid.Data;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.World;

    public abstract class ProtoObjectPowerStorage
        <TPrivateState,
         TPublicState,
         TClientState>
        : ProtoObjectStructure
          <TPrivateState,
              TPublicState,
              TClientState>,
          IProtoObjectElectricityStorage
        where TPrivateState : ObjectPowerGridPrivateState, new()
        where TPublicState : StaticObjectPublicState, new()
        where TClientState : StaticObjectClientState, new()
    {
        public abstract double ElectricityCapacity { get; }

        public bool IsAutoEnterPrivateScopeOnInteraction => true;

        public BaseUserControlWithWindow ClientOpenUI(IStaticWorldObject worldObject)
        {
            var privateState = GetPrivateState(worldObject);
            return WindowPowerStorage.Open(
                new ViewModelWindowPowerStorage(privateState));
        }

        void IInteractableProtoStaticWorldObject.ServerOnClientInteract(ICharacter who, IStaticWorldObject worldObject)
        {
        }

        void IInteractableProtoStaticWorldObject.ServerOnMenuClosed(ICharacter who, IStaticWorldObject worldObject)
        {
        }

        protected override void ClientInteractStart(ClientObjectData data)
        {
            InteractableStaticWorldObjectHelper.ClientStartInteract(data.GameObject);
        }
    }

    public abstract class ProtoObjectPowerStorage
        : ProtoObjectPowerStorage
            <ObjectPowerGridPrivateState,
                StaticObjectPublicState,
                StaticObjectClientState>
    {
    }
}