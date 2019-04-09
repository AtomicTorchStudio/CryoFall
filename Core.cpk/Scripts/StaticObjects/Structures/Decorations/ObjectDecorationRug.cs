namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Decorations
{
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    internal class ObjectDecorationRug : ProtoObjectDecorationFloor
    {
        public override string Description => "This fancy floor rug can make any room feel warmer and nicer.";

        public override string Name => "Floor rug";

        public override ObjectSoundMaterial ObjectSoundMaterial => ObjectSoundMaterial.Vegetation;

        public override double ObstacleBlockDamageCoef => 1.0;

        public override float StructurePointsMax => 200;

        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);
            renderer.DrawOrderOffsetY = 2;
        }

        protected override void CreateLayout(StaticObjectLayout layout)
        {
            layout.Setup("###",
                         "###");
        }

        protected override void PrepareFloorDecorationConstructionConfig(
            ConstructionStageConfig build,
            ConstructionStageConfig repair)
        {
            build.StagesCount = 10;
            build.StageDurationSeconds = BuildDuration.Short;
            build.AddStageRequiredItem<ItemThread>(count: 10);
            build.AddStageRequiredItem<ItemRope>(count: 1);
            build.AddStageRequiredItem<ItemGlue>(count: 1);

            repair.StagesCount = 10;
            repair.StageDurationSeconds = BuildDuration.Short;
            repair.AddStageRequiredItem<ItemThread>(count: 5);
        }
    }
}