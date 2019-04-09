namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Floors
{
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;

    public class ObjectFloorConcrete : ProtoObjectFloor
    {
        public override string Description => "Simple and utilitarian. Ideal for a factory floor or military base.";

        public override GroundSoundMaterial GroundSoundMaterial => GroundSoundMaterial.Stone;

        public override string Name => "Concrete floor";

        public override ObjectSoundMaterial ObjectSoundMaterial => ObjectSoundMaterial.Stone;

        public override float StructurePointsMax => 200;

        protected override void PrepareFloorConstructionConfig(
            ConstructionStageConfig build,
            ConstructionStageConfig repair)
        {
            build.StagesCount = 1;
            //build.StageDurationSeconds = BuildDuration.Short; <-- irrelevant here
            build.AddStageRequiredItem<ItemCement>(count: 5);

            repair.StagesCount = 1;
            repair.StageDurationSeconds = BuildDuration.Short;
            repair.AddStageRequiredItem<ItemCement>(count: 1);
        }
    }
}