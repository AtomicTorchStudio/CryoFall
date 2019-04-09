namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Walls
{
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;

    public class ObjectWallStone : ProtoObjectWall
    {
        public override string Description =>
            "Significantly better than the wooden wall. Should stop most intruders from attempting to break in, but won't be much help against well-equipped raiders.";

        public override string Name => "Stone wall";

        public override ObjectSoundMaterial ObjectSoundMaterial => ObjectSoundMaterial.Stone;

        public override double ObstacleBlockDamageCoef => 1.0;

        public override double StructureExplosiveDefenseCoef => 0.25;

        public override float StructurePointsMax => 28000;

        protected override void PrepareConstructionConfig(
            ConstructionTileRequirements tileRequirements,
            ConstructionStageConfig build,
            ConstructionStageConfig repair,
            ConstructionUpgradeConfig upgrade,
            out ProtoStructureCategory category)
        {
            category = GetCategory<StructureCategoryBuildings>();

            build.StagesCount = 5;
            build.StageDurationSeconds = BuildDuration.Short;
            build.AddStageRequiredItem<ItemStone>(count: 4);
            build.AddStageRequiredItem<ItemClay>(count: 4);

            repair.StagesCount = 10;
            repair.StageDurationSeconds = BuildDuration.Short;
            repair.AddStageRequiredItem<ItemStone>(count: 1);
            repair.AddStageRequiredItem<ItemClay>(count: 1);
        }

        protected override void PrepareDefense(DefenseDescription defense)
        {
            defense.Set(ObjectDefensePresets.Tier2);
        }
    }
}