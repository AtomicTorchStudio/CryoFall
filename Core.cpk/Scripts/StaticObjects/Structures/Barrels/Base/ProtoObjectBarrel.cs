namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Barrels
{
    using AtomicTorch.CBND.CoreMod.ItemContainers;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Data;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public abstract class ProtoObjectBarrel
        <TPrivateState,
         TPublicState,
         TClientState>
        : ProtoObjectManufacturer
          <TPrivateState,
              TPublicState,
              TClientState>,
          IProtoObjectBarrel
        where TPrivateState : ProtoBarrelPrivateState, new()
        where TPublicState : ProtoBarrelPublicState, new()
        where TClientState : StaticObjectClientState, new()
    {
        // no fuel is used by this manufacturer
        public override byte ContainerFuelSlotsCount => 0;

        public override byte ContainerInputSlotsCount => 1;

        public override byte ContainerOutputSlotsCount => 1;

        public override string InteractionTooltipText => InteractionTooltipTexts.Open;

        public override bool IsAutoSelectRecipe => true;

        public override bool IsFuelProduceByproducts => false;

        public abstract ushort LiquidCapacity { get; }

        public ProtoBarrelPrivateState GetBarrelPrivateState(IStaticWorldObject objectManufacturer)
        {
            return GetPrivateState(objectManufacturer);
        }

        protected override void ClientInitialize(ClientInitializeData data)
        {
            base.ClientInitialize(data);

            var spriterRendererLiquidType = Client.Rendering.CreateSpriteRenderer(
                data.GameObject,
                TextureResource.NoTexture,
                positionOffset: (0.5, 0.475),
                spritePivotPoint: (0.5, 0.5));
            spriterRendererLiquidType.DrawOrderOffsetY = -0.475f;
            spriterRendererLiquidType.Scale = 2;

            var publicState = data.SyncPublicState;
            publicState.ClientSubscribe(
                _ => _.LiquidType,
                _ => UpdatePublicLiquidState(),
                data.ClientState);

            UpdatePublicLiquidState();

            // local method for updating public liquid state
            void UpdatePublicLiquidState()
            {
                var liquidType = publicState.LiquidType;
                spriterRendererLiquidType.IsEnabled = liquidType.HasValue;
                spriterRendererLiquidType.TextureResource = LiquidColorIconHelper.GetTexture(liquidType);
            }
        }

        protected override BaseUserControlWithWindow ClientOpenUI(ClientObjectData data)
        {
            return WindowBarrel.Open(
                new ViewModelWindowBarrel(
                    data.GameObject,
                    data.SyncPrivateState.ManufacturingState,
                    this.ManufacturingConfig));
        }

        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);
            renderer.DrawOrderOffsetY = 0.35;
        }

        protected override void ServerInitialize(ServerInitializeData data)
        {
            base.ServerInitialize(data);
            // setup input container to allow only liquids on input
            Server.Items.SetContainerType<ItemsContainerAnyLiquidContainer>(
                data.PrivateState.ManufacturingState.ContainerInput);
        }

        protected override void ServerUpdate(ServerUpdateData data)
        {
            base.ServerUpdate(data);
            data.PublicState.LiquidType = data.PrivateState.LiquidType;
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeRectangle((0.8, 0.5), offset: (0.1, 0))
                .AddShapeRectangle(
                    (0.7, 1),
                    offset: (0.15, 0),
                    group: CollisionGroups.ClickArea);
        }
    }

    public abstract class ProtoObjectBarrel
        : ProtoObjectBarrel
            <ProtoBarrelPrivateState,
                ProtoBarrelPublicState,
                StaticObjectClientState>
    {
    }
}