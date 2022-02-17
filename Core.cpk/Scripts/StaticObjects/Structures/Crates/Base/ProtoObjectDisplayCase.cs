namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Crates
{
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
    using AtomicTorch.GameEngine.Common.Primitives;

    public abstract class ProtoObjectDisplayCase
        <TPrivateState,
         TPublicState,
         TClientState>
        : ProtoObjectCrate
            <TPrivateState,
                TPublicState,
                TClientState>
        where TPrivateState : ObjectCratePrivateState, new()
        where TPublicState : ObjectDisplayCasePublicState, new()
        where TClientState : StaticObjectClientState, new()
    {
        public override bool IsSupportItemIcon => false;

        protected virtual Vector2D ShowcaseItemRendererOffset => (0, 1);

        protected virtual ClientComponentDisplayCaseSlotRenderer ClientCreateDisplayCaseSlotRenderer(
            IStaticWorldObject worldObject,
            IItemsContainer itemsContainer,
            byte slotIndex)
        {
            var component = worldObject.ClientSceneObject
                                       .AddComponent<ClientComponentDisplayCaseSlotRenderer>();
            component.Setup(itemsContainer, slotIndex);
            // setup default horizontal offset according to the item slot index
            var renderer = component.RendererShowcaseItem;
            renderer.PositionOffset = this.ShowcaseItemRendererOffset
                                      + (0.5 + slotIndex * 0.5, renderer.PositionOffset.Y);
            return component;
        }

        protected override void ClientInitialize(ClientInitializeData data)
        {
            var worldObject = data.GameObject;
            var itemsContainer = data.PublicState.ItemsContainer;

            using var tempComponents = Api.Shared.GetTempList<IClientComponent>();
            for (byte slotIndex = 0; slotIndex < itemsContainer.SlotsCount; slotIndex++)
            {
                var component = this.ClientCreateDisplayCaseSlotRenderer(worldObject, itemsContainer, slotIndex);
                tempComponents.Add(component);
            }

            // create default sprite renderer (top part) and other base stuff
            base.ClientInitialize(data);

            var primaryRenderer = data.ClientState.Renderer;
            foreach (var c in tempComponents.AsList())
            {
                var component = (ClientComponentDisplayCaseSlotRenderer)c;
                component.RendererShowcaseItem.PositionOffset += (0, primaryRenderer.PositionOffset.Y);
                component.RendererShowcaseItem.DrawOrderOffsetY +=
                    primaryRenderer.DrawOrderOffsetY - this.ShowcaseItemRendererOffset.Y;
                component.Refresh();
            }
        }

        protected override void ServerInitialize(ServerInitializeData data)
        {
            base.ServerInitialize(data);
            data.PublicState.ItemsContainer = data.PrivateState.ItemsContainer;
        }
    }

    public abstract class ProtoObjectDisplayCase
        : ProtoObjectDisplayCase<
            ObjectCratePrivateState,
            ObjectDisplayCasePublicState,
            StaticObjectClientState>
    {
    }
}