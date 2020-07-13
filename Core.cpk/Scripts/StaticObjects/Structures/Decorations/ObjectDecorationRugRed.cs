namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Decorations
{
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class ObjectDecorationRugRed : ProtoObjectDecorationFloor
    {
        public override string Description => "This fancy floor rug can make any room feel warmer and nicer.";

        public override string Name => "Floor rug";

        public override ObjectMaterial ObjectMaterial => ObjectMaterial.Vegetation;

        public override double ObstacleBlockDamageCoef => 1.0;

        public override float StructurePointsMax => 500;

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
            build.StagesCount = 1;
            build.StageDurationSeconds = BuildDuration.Short;
            build.AddStageRequiredItem<ItemThread>(count: 100);
            build.AddStageRequiredItem<ItemRope>(count: 10);
            build.AddStageRequiredItem<ItemGlue>(count: 10);

            repair.StagesCount = 10;
            repair.StageDurationSeconds = BuildDuration.Short;
            repair.AddStageRequiredItem<ItemThread>(count: 5);
        }
    }
}