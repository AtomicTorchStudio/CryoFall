namespace AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation.Plants
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Loot;
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using static Systems.Physics.CollisionGroups;

    public class ObjectPlantWheat : ProtoObjectPlant
    {
        public override string Name => "Wheat";

        public override byte NumberOfHarvests => 1;

        public override double ObstacleBlockDamageCoef => 0.5;

        public override float StructurePointsMax => 100;

        protected override bool CanFlipSprite => false;

        protected override TimeSpan TimeToGiveHarvest { get; } = TimeSpan.FromHours(2);

        protected override TimeSpan TimeToMature { get; } = TimeSpan.FromHours(8);

        protected override void ClientRefreshVegetationRendering(
            IStaticWorldObject worldObject,
            VegetationClientState clientState,
            VegetationPublicState publicState)
        {
            base.ClientRefreshVegetationRendering(worldObject, clientState, publicState);

            if (publicState.GrowthStage > 1
                && publicState.GrowthStage < this.GrowthStagesCount)
            {
                ClientGrassRenderingHelper.Setup(clientState.Renderer,
                                                 power: 0.1f,
                                                 pivotY: 0.2f);
            }
            else
            {
                // no grass swaying for the just planted and spoiled plant
                clientState.Renderer.RenderingMaterial = null;
            }
        }

        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);
            renderer.DrawOrderOffsetY = 0.25;
        }

        protected override ITextureResource PrepareDefaultTexture(Type thisType)
        {
            return new TextureAtlasResource(
                base.PrepareDefaultTexture(thisType),
                columns: 6,
                rows: 1);
        }

        protected override void PrepareGatheringDroplist(DropItemsList droplist)
        {
            droplist.Add<ItemWheatGrains>(count: 25, countRandom: 5);

            // additional yield
            droplist.Add<ItemWheatGrains>(count: 10, countRandom: 5, condition: ItemFertilizer.ConditionExtraYield);
            droplist.Add<ItemWheatGrains>(count: 10,
                                          countRandom: 5,
                                          condition: SkillFarming.ConditionExtraYield,
                                          probability: 0.05f);
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeRectangle(size: (0.9, 0.7), offset: (0.05, 0.15))
                .AddShapeRectangle(size: (0.9, 0.7),
                                   offset: (0.05, 0.15),
                                   group: HitboxMelee)
                .AddShapeRectangle(size: (0.9, 0.7),
                                   offset: (0.05, 0.15),
                                   group: HitboxRanged)
                .AddShapeCircle(radius: 0.45, center: (0.5, 0.5), group: ClickArea);
        }
    }
}