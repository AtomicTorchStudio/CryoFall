﻿namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Crates
{
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class ObjectCrateWood : ProtoObjectCrate
    {
        public override string Description =>
            "This is the most simple crate for storage of one's possessions.";

        public override bool HasOwnersList => false;

        public override bool IsSupportItemIcon => false;

        public override byte ItemsSlotsCount => 16;

        public override string Name => "Wooden crate";

        public override ObjectMaterial ObjectMaterial => ObjectMaterial.Wood;

        public override double ObstacleBlockDamageCoef => 0.5;

        public override float StructurePointsMax => 400;

        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);
            renderer.PositionOffset += (0, 0.35);
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

            build.StagesCount = 1;
            build.StageDurationSeconds = BuildDuration.Short;
            build.AddStageRequiredItem<ItemPlanks>(count: 25);

            repair.StagesCount = 10;
            repair.StageDurationSeconds = BuildDuration.Short;
            repair.AddStageRequiredItem<ItemPlanks>(count: 1);
        }

        protected override void PrepareDefense(DefenseDescription defense)
        {
            defense.Set(ObjectDefensePresets.Tier3);
        }
    }
}