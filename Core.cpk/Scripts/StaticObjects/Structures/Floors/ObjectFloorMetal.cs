namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Floors
{
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;

    public class ObjectFloorMetal : ProtoObjectFloor
    {
        public override string Description => "Durable and slick. Good choice for a military bunker.";

        public override GroundSoundMaterial GroundSoundMaterial => GroundSoundMaterial.Metal;

        public override string Name => "Metal floor";

        public override ObjectMaterial ObjectMaterial => ObjectMaterial.Metal;

        public override float StructurePointsMax => 1000;

        protected override void PrepareFloorConstructionConfig(
            ConstructionStageConfig build,
            ConstructionStageConfig repair)
        {
            build.StagesCount = 1;
            //build.StageDurationSeconds = BuildDuration.Short; <-- irrelevant here
            build.AddStageRequiredItem<ItemIngotSteel>(count: 5);

            repair.StagesCount = 1;
            repair.StageDurationSeconds = BuildDuration.Short;
            repair.AddStageRequiredItem<ItemIngotSteel>(count: 2);
        }
    }
}