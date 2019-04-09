namespace AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation.Bushes
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Food;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Resources;

    public class ObjectBushPurple : ProtoObjectBush
    {
        public override bool IsAutoAddShadow => true;

        public override string Name => "Berry bush (purple)";

        protected override TimeSpan TimeToGiveHarvest => TimeSpan.FromHours(1);

        protected override TimeSpan TimeToMature => TimeSpan.FromHours(2);

        public override float CalculateShadowScale(VegetationClientState clientState)
        {
            return base.CalculateShadowScale(clientState) * 0.4f;
        }

        protected override ITextureResource PrepareDefaultTexture(Type thisType)
        {
            return new TextureAtlasResource(
                base.PrepareDefaultTexture(thisType),
                columns: 4,
                rows: 1);
        }

        protected override void PrepareGatheringDroplist(DropItemsList droplist)
        {
            droplist
                .Add<ItemBerriesViolet>(count: 1)
                .Add<ItemBerriesViolet>(count: 1,
                                        probability: 1 / 5.0,
                                        condition: SkillForaging.ConditionAdditionalYield);
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeCircle(
                    radius: 0.18,
                    center: (0.5, 0.25))
                .AddShapeRectangle(
                    size: (0.8, 0.7),
                    offset: (0.1, 0.1),
                    group: CollisionGroups.HitboxMelee)
                .AddShapeRectangle(
                    size: (0.9, 0.8),
                    offset: (0.05, 0.05),
                    group: CollisionGroups.HitboxRanged)
                .AddShapeCircle(
                    radius: 0.4,
                    center: (0.5, 0.5),
                    group: CollisionGroups.ClickArea);
        }
    }
}