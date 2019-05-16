namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Beds
{
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class ObjectBedroll : ProtoObjectBed
    {
        public override string Description =>
            "Place bedroll in your base to serve as a respawn point, allowing you to respawn in a specified location after death.";

        public override string Name => "Bedroll";

        public override ObjectSoundMaterial ObjectSoundMaterial => ObjectSoundMaterial.Wood;

        public override double ObstacleBlockDamageCoef => 0.3;

        public override double RespawnCooldownDurationSeconds => 4 * 60;

        public override float StructurePointsMax => 500;

        protected override void ClientInitialize(ClientInitializeData data)
        {
            base.ClientInitialize(data);
            data.ClientState.Renderer.DrawOrder = DrawOrder.FloorCharredGround + 1;
        }

        protected override void CreateLayout(StaticObjectLayout layout)
        {
            layout.Setup("#",
                         "#");
        }

        protected override void PrepareConstructionConfig(
            ConstructionTileRequirements tileRequirements,
            ConstructionStageConfig build,
            ConstructionStageConfig repair,
            ConstructionUpgradeConfig upgrade,
            out ProtoStructureCategory category)
        {
            category = GetCategory<StructureCategoryOther>();

            build.StagesCount = 1;
            build.StageDurationSeconds = BuildDuration.Short;
            build.AddStageRequiredItem<ItemTwigs>(count: 20);
            build.AddStageRequiredItem<ItemFibers>(count: 50);

            repair.StagesCount = 5;
            repair.StageDurationSeconds = BuildDuration.Short;
            repair.AddStageRequiredItem<ItemTwigs>(count: 2);
            repair.AddStageRequiredItem<ItemFibers>(count: 5);
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            // no physics - can walk over

            data.PhysicsBody.AddShapeRectangle((1, 2), group: CollisionGroups.ClickArea);
        }
    }
}