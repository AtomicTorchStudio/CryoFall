namespace AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation.Plants
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Loot;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Farms;
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ObjectPlantFlowerBlueSage : ProtoObjectPlant
    {
        public override string Name => "Blue sage";

        public override byte NumberOfHarvests => 1;

        public override double ObstacleBlockDamageCoef => 0.5;

        public override float StructurePointsMax => 25;

        protected override bool CanFlipSprite => false;

        protected override TimeSpan TimeToGiveHarvest { get; } = TimeSpan.FromHours(1);

        protected override TimeSpan TimeToHarvestSpoil => TimeSpan.FromDays(5);

        protected override TimeSpan TimeToMature { get; } = TimeSpan.FromHours(3);

        public override Vector2D SharedGetObjectCenterWorldOffset(IWorldObject worldObject)
        {
            return (0.5, 0.2);
        }

        protected override void ClientInitialize(ClientInitializeData data)
        {
            base.ClientInitialize(data);

            var protoFarm = CommonGetFarmObjectProto(data.GameObject.OccupiedTile);
            if (!(protoFarm is ObjectPlantPot))
            {
                data.ClientState.Renderer.DrawOrderOffsetY += 0.5;
            }
        }

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
                                                 pivotY: 0.5f);
            }
            else
            {
                // no grass swaying for the just planted and spoiled sprites
                clientState.Renderer.RenderingMaterial = null;
            }
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
            droplist.Add<ItemFlowerBlueSage>(count: 2);

            // additional yield
            droplist.Add<ItemFlowerBlueSage>(count: 1, condition: ItemFertilizer.ConditionExtraYield);
            droplist.Add<ItemFlowerBlueSage>(count: 1, condition: SkillFarming.ConditionExtraYield, probability: 0.05f);
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