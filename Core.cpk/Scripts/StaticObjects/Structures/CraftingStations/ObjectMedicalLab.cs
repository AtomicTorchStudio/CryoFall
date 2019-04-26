namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.CraftingStations
{
    using AtomicTorch.CBND.CoreMod.Items.Food;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class ObjectMedicalLab : ProtoObjectCraftStation
    {
        public override string Description => "Allows preparation of many different medical supplies and items.";

        public override string Name => "Medical laboratory";

        public override ObjectSoundMaterial ObjectSoundMaterial => ObjectSoundMaterial.Wood;

        public override double ObstacleBlockDamageCoef => 1.0;

        public override float StructurePointsMax => 300;

        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);
            renderer.DrawOrderOffsetY = 0.7;
        }

        protected override void CreateLayout(StaticObjectLayout layout)
        {
            layout.Setup("###");
        }

        protected override void PrepareConstructionConfig(
            ConstructionTileRequirements tileRequirements,
            ConstructionStageConfig build,
            ConstructionStageConfig repair,
            ConstructionUpgradeConfig upgrade,
            out ProtoStructureCategory category)
        {
            category = GetCategory<StructureCategoryIndustry>();

            build.StagesCount = 10;
            build.StageDurationSeconds = BuildDuration.Short;
            build.AddStageRequiredItem<ItemPlanks>(count: 20);
            build.AddStageRequiredItem<ItemIngotCopper>(count: 1);
            build.AddStageRequiredItem<ItemBottleWater>(count: 1);

            repair.StagesCount = 5;
            repair.StageDurationSeconds = BuildDuration.Short;
            repair.AddStageRequiredItem<ItemPlanks>(count: 20);
            repair.AddStageRequiredItem<ItemIngotCopper>(count: 1);
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            var hitboxMelee = CollisionGroups.HitboxMelee;
            var hitboxRanged = CollisionGroups.HitboxRanged;
            var clickArea = CollisionGroups.ClickArea;

            data.PhysicsBody
                // static collider
                .AddShapeRectangle((3, 0.8), offset: (0, 0))

                // melee hitbox
                .AddShapeRectangle((3, 0.8), offset: (0, 0.2), group: hitboxMelee)

                // ranged hitbox
                .AddShapeRectangle((3, 0.8), offset: (0, 0.2), group: hitboxRanged)

                // click area
                .AddShapeRectangle((3, 1.1), offset: (0, 0), group: clickArea);
        }
    }
}