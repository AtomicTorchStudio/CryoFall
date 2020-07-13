namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Decorations
{
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class ObjectDecorationRugBear : ProtoObjectDecorationFloor
    {
        public override string Description =>
            "Show that you are a real hunter and decorate your room with this luxurious rug.";

        public override string Name => "Bear rug";

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
            layout.Setup("##",
                         "##");
        }

        protected override void PrepareFloorDecorationConstructionConfig(
            ConstructionStageConfig build,
            ConstructionStageConfig repair)
        {
            build.StagesCount = 1;
            build.StageDurationSeconds = BuildDuration.Short;
            build.AddStageRequiredItem<ItemLeather>(count: 25);
            build.AddStageRequiredItem<ItemFur>(count: 25);
            build.AddStageRequiredItem<ItemThread>(count: 50);

            repair.StagesCount = 10;
            repair.StageDurationSeconds = BuildDuration.Short;
            repair.AddStageRequiredItem<ItemThread>(count: 5);
        }
    }
}