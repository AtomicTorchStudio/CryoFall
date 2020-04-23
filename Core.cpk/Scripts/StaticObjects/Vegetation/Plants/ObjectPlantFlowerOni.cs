namespace AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation.Plants
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Farms;
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ObjectPlantFlowerOni : ProtoObjectPlant
    {
        public override string Name => "Oniflower";

        public override byte NumberOfHarvests => 1;

        public override double ObstacleBlockDamageCoef => 0.5;

        public override float StructurePointsMax => 25;

        protected override bool CanFlipSprite => true;

        protected override TimeSpan TimeToGiveHarvest { get; } = TimeSpan.FromHours(1);

        protected override TimeSpan TimeToMature { get; } = TimeSpan.FromHours(1);

        protected override TimeSpan TimeToHarvestSpoil => TimeSpan.FromDays(5);

        public override Vector2D SharedGetObjectCenterWorldOffset(IWorldObject worldObject)
            => (0.5, 0.2);

        protected override void ClientInitialize(ClientInitializeData data)
        {
            base.ClientInitialize(data);

            var protoFarm = CommonGetFarmObjectProto(data.GameObject.OccupiedTile);
            if (!(protoFarm is ObjectPlantPot))
            {
                data.ClientState.Renderer.DrawOrderOffsetY += 0.5;
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
            droplist.Add<ItemFlowerOni>(count: 1);

            // additional yield
            droplist.Add<ItemFlowerOni>(count: 1, condition: ItemFertilizer.ConditionExtraYield);
            droplist.Add<ItemFlowerOni>(count: 1, condition: SkillFarming.ConditionExtraYield, probability: 0.05f);
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