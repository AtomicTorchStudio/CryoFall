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

    public class ObjectTradingStationSmall : ProtoObjectTradingStation
    {
        public override string Description =>
            "Automated trading station. Can be used to conveniently trade items with other survivors. Can be configured to either sell or buy orders.";

        public override byte LotsCount => 2;

        public override string Name => "Small trading station";

        public override ObjectSoundMaterial ObjectSoundMaterial => ObjectSoundMaterial.Metal;

        public override double ObstacleBlockDamageCoef => 1;

        public override byte StockItemsContainerSlotsCount => 16;

        public override double StructureExplosiveDefenseCoef => 0.25;

        public override float StructurePointsMax => 20000;

        protected override BaseClientComponentLightSource ClientCreateLightSource(IClientSceneObject sceneObject)
        {
            return ClientLighting.CreateLightSourceSpot(
                sceneObject,
                color: LightColors.ElectricCold,
                size: (4, 8),
                positionOffset: (0.5, 0.6));
        }

        protected override void ClientInitialize(ClientInitializeData data)
        {
            base.ClientInitialize(data);

            data.ClientState
                .RendererTradingStationContent
                .PositionOffset
                = (0.565, 0.11);

            ClientFixTradingStationContentDrawOffset(data);
        }

        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);
            renderer.DrawOrderOffsetY = 0.2;
        }

        protected override void CreateLayout(StaticObjectLayout layout)
        {
            layout.Setup("#");
        }

        protected override void PrepareConstructionConfig(
            ConstructionTileRequirements tileRequirements,
            ConstructionStageConfig build,
            ConstructionStageConfig repair,
            ConstructionUpgradeConfig upgrade,
            out ProtoStructureCategory category)
        {
            category = GetCategory<StructureCategoryOther>();

            build.StagesCount = 10;
            build.StageDurationSeconds = BuildDuration.Short;
            build.AddStageRequiredItem<ItemIngotSteel>(count: 5);
            build.AddStageRequiredItem<ItemGlassRaw>(count: 10);
            build.AddStageRequiredItem<ItemComponentsElectronic>(count: 1);

            repair.StagesCount = 10;
            repair.StageDurationSeconds = BuildDuration.Short;
            repair.AddStageRequiredItem<ItemIngotSteel>(count: 2);
        }

        protected override void PrepareDefense(DefenseDescription defense)
        {
            defense.Set(ObjectDefensePresets.Tier3);
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            Vector2D size = (0.9, 0.5);
            Vector2D offset = ((1 - size.X) / 2, 0.05);

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