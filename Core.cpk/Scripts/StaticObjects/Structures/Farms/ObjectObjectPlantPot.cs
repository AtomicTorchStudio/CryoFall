namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Farms
{
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation.Plants;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ObjectPlantPot : ProtoObjectPlantPot
    {
        public override string Description =>
            "Can be used to grow flowers and decorate your house or collect flowers for other purposes. Can be constructed anywhere.";

        public override bool IsDrawingPlantShadow => false;

        public override string Name => "Plant pot";

        public override ObjectSoundMaterial ObjectSoundMaterial => ObjectSoundMaterial.Stone;

        public override Vector2D PlacedPlantPositionOffset { get; } = (0, 0.15);

        public override float StructurePointsMax => 150;

        public override void ServerOnDestroy(IStaticWorldObject gameObject)
        {
            // destroy all the plants over this plant pot
            foreach (var tile in gameObject.OccupiedTiles)
            {
                foreach (var staticWorldObject in Api.Shared.WrapInTempList(tile.StaticObjects))
                {
                    if (staticWorldObject.ProtoStaticWorldObject is IProtoObjectPlant)
                    {
                        Server.World.DestroyObject(staticWorldObject);
                    }
                }
            }

            base.ServerOnDestroy(gameObject);
        }

        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);
            renderer.DrawOrderOffsetY = 0.33;
        }

        protected override void PrepareFarmConstructionConfig(
            ConstructionTileRequirements tileRequirements,
            ConstructionStageConfig build,
            ConstructionStageConfig repair)
        {
            build.StagesCount = 1;
            build.StageDurationSeconds = BuildDuration.Short;
            build.AddStageRequiredItem<ItemStone>(count: 25);
            build.AddStageRequiredItem<ItemClay>(count: 25);
            build.AddStageRequiredItem<ItemSand>(count: 25);

            repair.StagesCount = 10;
            repair.StageDurationSeconds = BuildDuration.Short;
            repair.AddStageRequiredItem<ItemStone>(count: 2);
            repair.AddStageRequiredItem<ItemClay>(count: 2);
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeCircle(
                    radius: 0.33,
                    center: (0.5, 0.4))
                .AddShapeRectangle(
                    size: (0.8, 0.7),
                    offset: (0.1, 0.1),
                    group: CollisionGroups.HitboxMelee)
                .AddShapeRectangle(
                    size: (0.9, 0.8),
                    offset: (0.05, 0.05),
                    group: CollisionGroups.HitboxRanged);
        }
    }
}