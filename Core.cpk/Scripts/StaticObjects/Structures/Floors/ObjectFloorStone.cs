namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Floors
{
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;

    public class ObjectFloorStone : ProtoObjectFloor
    {
        public override string Description => "Gives you that nice castle vibe. Perfect match for the stone walls.";

        public override GroundSoundMaterial GroundSoundMaterial => GroundSoundMaterial.Stone;

        public override string Name => "Stone floor";

        public override ObjectSoundMaterial ObjectSoundMaterial => ObjectSoundMaterial.Stone;

        public override float StructurePointsMax => 150;

        protected override void PrepareFloorConstructionConfig(
            ConstructionStageConfig build,
            ConstructionStageConfig repair)
        {
            build.StagesCount = 1;
            //build.StageDurationSeconds = BuildDuration.Short; <-- irrelevant here
            build.AddStageRequiredItem<ItemStone>(count: 10);

            repair.StagesCount = 1;
            repair.StageDurationSeconds = BuildDuration.Short;
            repair.AddStageRequiredItem<ItemStone>(count: 5);
        }
    }
}