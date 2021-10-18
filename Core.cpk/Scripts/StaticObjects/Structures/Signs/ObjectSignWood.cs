namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Signs
{
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ObjectSignWood : ProtoObjectSign
    {
        public override string Description =>
            "Convenient wooden sign to write anything you need to remember or let other people know about.";

        public override string Name => "Wooden sign";

        public override ObjectMaterial ObjectMaterial => ObjectMaterial.Wood;

        public override double ObstacleBlockDamageCoef => 0.5;

        public override float StructurePointsMax => 250;

        public override Vector2D SharedGetObjectCenterWorldOffset(IWorldObject worldObject)
        {
            return (0.5, 0.725);
        }

        protected override void ClientInitialize(ClientInitializeData data)
        {
            base.ClientInitialize(data);
            data.ClientState.RendererSignContent.PositionOffset = (0.1, 0.71);
            ClientFixSignDrawOffset(data);
        }

        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);
            renderer.PositionOffset = (0.5, 0.2);
            renderer.DrawOrderOffsetY = 0.15;
        }

        protected override void PrepareConstructionConfig(
            ConstructionStageConfig build,
            ConstructionStageConfig repair,
            ConstructionUpgradeConfig upgrade,
            out ProtoStructureCategory category)
        {
            category = GetCategory<StructureCategoryDecorations>();

            build.StagesCount = 5;
            build.StageDurationSeconds = BuildDuration.Medium;
            build.AddStageRequiredItem<ItemPlanks>(count: 5);

            repair.StagesCount = 5;
            repair.StageDurationSeconds = BuildDuration.Medium;
            repair.AddStageRequiredItem<ItemPlanks>(count: 3);
        }
    }
}