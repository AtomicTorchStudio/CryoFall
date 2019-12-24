namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.CraftingStations
{
    using AtomicTorch.CBND.CoreMod.Systems.InteractionChecker;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Crafting;

    public abstract class ProtoObjectCraftStation
        <TPrivateState,
         TPublicState,
         TClientState>
        : ProtoObjectStructure
          <TPrivateState,
              TPublicState,
              TClientState>,
          IProtoObjectCraftStation
        where TPrivateState : StructurePrivateState, new()
        where TPublicState : StaticObjectPublicState, new()
        where TClientState : StaticObjectClientState, new()
    {
        public override double ServerUpdateIntervalSeconds => double.MaxValue;

        protected override void ClientInteractStart(ClientObjectData data)
        {
            var worldObject = data.GameObject;
            var character = Client.Characters.CurrentPlayerCharacter;

            var menuWindow = WindowCraftingStation.Open(this);
            ClientCurrentInteractionMenu.RegisterMenuWindow(menuWindow);

            InteractionCheckerSystem.SharedRegister(
                character,
                worldObject,
                finishAction: _ => menuWindow.CloseWindow());

            ClientInteractionUISystem.Register(
                worldObject,
                menuWindow,
                onMenuClosedByClient:
                () => InteractionCheckerSystem.SharedUnregister(
                    character,
                    worldObject,
                    isAbort: false));

            ClientCurrentInteractionMenu.Open();
        }
    }

    public abstract class ProtoObjectCraftStation
        : ProtoObjectCraftStation
            <StructurePrivateState,
                StaticObjectPublicState,
                StaticObjectClientState>
    {
    }
}