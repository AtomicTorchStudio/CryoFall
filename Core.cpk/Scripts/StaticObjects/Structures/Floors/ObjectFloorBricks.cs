namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Floors
{
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;

    public class ObjectFloorBricks : ProtoObjectFloor
    {
        public override string Description =>
            "Unusual stylistic choice, but looks nice nonetheless.";

        public override GroundSoundMaterial GroundSoundMaterial => GroundSoundMaterial.Stone;

        public override string Name => "Brick floor";

        public override ObjectMaterial ObjectMaterial => ObjectMaterial.Stone;

        public override float StructurePointsMax => 500;

        protected override void PrepareFloorConstructionConfig(
            ConstructionStageConfig build,
            ConstructionStageConfig repair)
        {
            build.StagesCount = 1;
            //build.StageDurationSeconds = BuildDuration.Short; <-- irrelevant here
            build.AddStageRequiredItem<ItemBricks>(count: 3);

            repair.StagesCount = 1;
            repair.StageDurationSeconds = BuildDuration.Short;
            repair.AddStageRequiredItem<ItemBricks>(count: 1);
        }
    }
}