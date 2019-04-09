namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.TradingStations
{
    using AtomicTorch.CBND.CoreMod.ClientComponents.Rendering.Lighting;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ObjectTradingStationLarge : ProtoObjectTradingStation
    {
        public override string Description =>
            "Large automated trading station. Can be used to conveniently trade a wide array of items with other survivors. Can be configured to either sell or buy orders.";

        public override byte LotsCount => 6;

        public override string Name => "Large trading station";

        public override ObjectSoundMaterial ObjectSoundMaterial => ObjectSoundMaterial.Metal;

        public override double ObstacleBlockDamageCoef => 1;

        public override byte StockItemsContainerSlotsCount => 24;

        public override double StructureExplosiveDefenseCoef => 0.25;

        public override float StructurePointsMax => 35000;

        protected override BaseClientComponentLightSource ClientCreateLightSource(IClientSceneObject sceneObject)
        {
            return ClientLighting.CreateLightSourceSpot(
                sceneObject,
                color: LightColors.ElectricCold,
                size: (6, 12),
                positionOffset: (1, 0.6));
        }

        protected override void ClientInitialize(ClientInitializeData data)
        {
            base.ClientInitialize(data);

            data.ClientState.RendererTradingStationContent.PositionOffset
                = (data.ClientState.Renderer.PositionOffset.X + 0.59, 0.11);

            ClientFixTradingStationContentDrawOffset(data);
        }

        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);
            renderer.DrawOrderOffsetY = 0.2;
            renderer.PositionOffset = (0.13, 0);
        }

        protected override void CreateLayout(StaticObjectLayout layout)
        {
            layout.Setup("##");
        }

        protected override void PrepareConstructionConfig(
            ConstructionTileRequirements tileRequirements,
            ConstructionStageConfig build,
            ConstructionStageConfig repair,
            ConstructionUpgradeConfig upgrade,
            out ProtoStructureCategory category)
        {
            // TODO: set proper values here
            category = GetCategory<StructureCategoryOther>();

            build.StagesCount = 10;
            build.StageDurationSeconds = BuildDuration.Short;
            build.AddStageRequiredItem<ItemIngotSteel>(count: 10);
            build.AddStageRequiredItem<ItemGlassRaw>(count: 20);
            build.AddStageRequiredItem<ItemComponentsElectronic>(count: 1);

            repair.StagesCount = 10;
            repair.StageDurationSeconds = BuildDuration.Short;
            repair.AddStageRequiredItem<ItemIngotSteel>(count: 4);
        }

        protected override void PrepareDefense(DefenseDescription defense)
        {
            defense.Set(ObjectDefensePresets.Tier3);
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            Vector2D size = (1.8, 0.5);
            Vector2D offset = ((2 - size.X) / 2, 0.05);

            data.PhysicsBody
                .AddShapeRectangle(size,
                                   offset: offset,
                                   group: CollisionGroups.Default)
                .AddShapeRectangle(size,
                                   offset: offset,
                                   group: CollisionGroups.HitboxMelee)
                .AddShapeRectangle(size,
                                   offset: offset,
                                   group: CollisionGroups.HitboxRanged)
                .AddShapeRectangle(size + (0, 0.75),
                                   offset: offset,
                                   group: CollisionGroups.ClickArea);
        }
    }
}