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

        // available by default
        public override bool IsAutoUnlocked => true;

        public override string Name => "Bedroll";

        public override ObjectMaterial ObjectMaterial => ObjectMaterial.SolidGround;

        public override double ObstacleBlockDamageCoef => 0.3;

        public override double RespawnCooldownDurationSeconds => 4 * 60;

        public override float StructurePointsMax => 500;

        protected override void ClientInitialize(ClientInitializeData data)
        {
            var worldObject = data.GameObject;
            var clientState = data.ClientState;

            this.ClientAddAutoStructurePointsBar(data);

            clientState.Renderer = Client.Rendering.CreateSpriteRenderer(
                worldObject,
                this.DefaultTexture,
                drawOrder: DrawOrder.FloorCharredGround + 1);

            this.ClientSetupRenderer(clientState.Renderer);
        }

        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);
            renderer.PositionOffset += (0, 0.25);
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
            tileRequirements.Clear()
                            .Add(ConstructionTileRequirements.DefaultForPlayerStructuresOwnedOrFreeLand);

            category = GetCategory<StructureCategoryOther>();

            build.StagesCount = 1;
            build.StageDurationSeconds = BuildDuration.Short;
            build.AddStageRequiredItem<ItemTwigs>(count: 20);
            build.AddStageRequiredItem<ItemFibers>(count: 20);

            repair.StagesCount = 5;
            repair.StageDurationSeconds = BuildDuration.Short;
            repair.AddStageRequiredItem<ItemTwigs>(count: 2);
            repair.AddStageRequiredItem<ItemFibers>(count: 2);
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            // no static collider - can walk over
            // but has melee weapon collider - could be destroyed with a melee weapon
            data.PhysicsBody
                .AddShapeRectangle(size: (0.3, 1),   offset: (0.35, 0.5), group: CollisionGroups.HitboxMelee)
                .AddShapeRectangle(size: (0.8, 1.5), offset: (0.1, 0.3),  group: CollisionGroups.ClickArea);
        }
    }
}