namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Crates
{
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class ObjectCrateIron : ProtoObjectCrate
    {
        public override string Description => "Affords more space than a simple wooden crate and is harder to destroy.";

        public override bool HasOwnersList => false;

        public override byte ItemsSlotsCount => 24;

        public override string Name => "Iron crate";

        public override ObjectMaterial ObjectMaterial => ObjectMaterial.Wood;

        public override double ObstacleBlockDamageCoef => 0.5;

        public override double StructureExplosiveDefenseCoef => 0.25;

        public override float StructurePointsMax => 3500;

        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);
            renderer.PositionOffset += (0, 0.4);
            renderer.DrawOrderOffsetY = 0.2;
        }

        protected override void PrepareConstructionConfig(
            ConstructionTileRequirements tileRequirements,
            ConstructionStageConfig build,
            ConstructionStageConfig repair,
            ConstructionUpgradeConfig upgrade,
            out ProtoStructureCategory category)
        {
            category = GetCategory<StructureCategoryStorage>();

            build.StagesCount = 5;
            build.StageDurationSeconds = BuildDuration.Short;
            build.AddStageRequiredItem<ItemPlanks>(count: 4);
            build.AddStageRequiredItem<ItemIngotIron>(count: 1);

            repair.StagesCount = 5;
            repair.StageDurationSeconds = BuildDuration.Short;
            repair.AddStageRequiredItem<ItemPlanks>(count: 2);
            repair.AddStageRequiredItem<ItemIngotIron>(count: 1);
        }

        protected override void PrepareDefense(DefenseDescription defense)
        {
            defense.Set(ObjectDefensePresets.Tier3);
        }
    }
}