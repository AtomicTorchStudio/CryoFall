﻿namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Walls
{
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;

    public class ObjectWallBrick : ProtoObjectWall
    {
        public override string Description =>
            "Solid brick wall reinforced with metal. This will keep your valuables in and intruders out. Can even withstand explosives to some degree.";

        public override string Name => "Brick wall";

        public override ObjectMaterial ObjectMaterial => ObjectMaterial.Stone;

        public override double ObstacleBlockDamageCoef => 1.0;

        public override double StructureExplosiveDefenseCoef => 0.5;

        public override float StructurePointsMax => 35000;

        protected override void PrepareConstructionConfig(
            ConstructionStageConfig build,
            ConstructionStageConfig repair,
            ConstructionUpgradeConfig upgrade,
            out ProtoStructureCategory category)
        {
            category = GetCategory<StructureCategoryBuildings>();

            build.StagesCount = 3;
            build.StageDurationSeconds = BuildDuration.Medium;
            build.AddStageRequiredItem<ItemBricks>(count: 5);
            build.AddStageRequiredItem<ItemIngotCopper>(count: 1);
            build.AddStageRequiredItem<ItemCement>(count: 1);

            repair.StagesCount = 3;
            repair.StageDurationSeconds = BuildDuration.Short;
            repair.AddStageRequiredItem<ItemBricks>(count: 3);
        }

        protected override void PrepareDefense(DefenseDescription defense)
        {
            defense.Set(ObjectDefensePresets.Tier5);
        }
    }
}