namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Signs
{
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ObjectSignDigital : ProtoObjectSignPicture
    {
        public override string Description =>
            "Allows you to display an image for the world to see. Uses solar panels on the back, removing the need for an external power source.";

        public override string Name => "Digital sign";

        public override ObjectMaterial ObjectMaterial => ObjectMaterial.Metal;

        public override double ObstacleBlockDamageCoef => 0.5;

        public override float StructurePointsMax => 1200;

        public override Vector2D SharedGetObjectCenterWorldOffset(IWorldObject worldObject)
            => (0.5, 0.725);

        protected override void ClientInitialize(ClientInitializeData data)
        {
            base.ClientInitialize(data);

            var signRenderer = data.ClientState.RendererSignContent;
            signRenderer.PositionOffset = data.ClientState.Renderer.PositionOffset
                                          + (0, 0.72);
            signRenderer.SpritePivotPoint = (0.5, 0.5);

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
            category = GetCategory<StructureCategoryOther>();

            build.StagesCount = 5;
            build.StageDurationSeconds = BuildDuration.Short;
            build.AddStageRequiredItem<ItemIngotCopper>(count: 1);
            build.AddStageRequiredItem<ItemPlastic>(count: 1);

            repair.StagesCount = 5;
            repair.StageDurationSeconds = BuildDuration.Short;
            repair.AddStageRequiredItem<ItemIngotCopper>(count: 1);
            repair.AddStageRequiredItem<ItemPlastic>(count: 1);
        }
    }
}