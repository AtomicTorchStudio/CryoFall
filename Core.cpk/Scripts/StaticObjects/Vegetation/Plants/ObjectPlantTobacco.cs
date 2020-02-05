namespace AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation.Plants
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Loot;
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class ObjectPlantTobacco : ProtoObjectPlant
    {
        public override string Name => "Tobacco";

        public override byte NumberOfHarvests => 1;

        public override double ObstacleBlockDamageCoef => 0.5;

        public override float StructurePointsMax => 100;

        protected override TimeSpan TimeToGiveHarvest { get; } = TimeSpan.FromHours(5);

        protected override TimeSpan TimeToMature { get; } = TimeSpan.FromHours(3);

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
                                                 power: 0.05f,
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
            renderer.DrawOrderOffsetY = 0.6;
        }

        protected override ITextureResource PrepareDefaultTexture(Type thisType)
        {
            return new TextureAtlasResource(
                base.PrepareDefaultTexture(thisType),
                columns: 5,
                rows: 1);
        }

        protected override void PrepareGatheringDroplist(DropItemsList droplist)
        {
            droplist.Add<ItemTobaccoRaw>(count: 5);

            // additional yield
            droplist.Add<ItemTobaccoRaw>(count: 2, condition: ItemFertilizer.ConditionExtraYield);
            droplist.Add<ItemTobaccoRaw>(count: 2, condition: SkillFarming.ConditionExtraYield, probability: 0.05f);
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeCircle(radius: 0.28, center: (0.5, 0.55))
                .AddShapeCircle(radius: 0.35, center: (0.5, 0.55), group: CollisionGroups.HitboxMelee)
                .AddShapeCircle(radius: 0.3,  center: (0.5, 0.55), group: CollisionGroups.HitboxRanged)
                .AddShapeCircle(radius: 0.45, center: (0.5, 0.5),  group: CollisionGroups.ClickArea);
        }
    }
}