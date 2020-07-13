namespace AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation.Trees
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Items.Seeds;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Resources;

    public class ObjectTreeBirch : ProtoObjectTree
    {
        public override string Name => "Birch tree";

        public override double TreeHeight => 2;

        protected override TimeSpan TimeToMature => TimeSpan.FromHours(2);

        protected override ITextureResource PrepareDefaultTexture(Type thisType)
        {
            return new TextureAtlasResource(
                base.PrepareDefaultTexture(thisType),
                columns: 3,
                rows: 1);
        }

        protected override void PrepareDroplistOnDestroy(DropItemsList droplist)
        {
            // primary drop
            droplist
                .Add<ItemLogs>(count: 5)
                .Add<ItemTwigs>(count: 2, countRandom: 2);

            // saplings drop (requires skill)
            droplist
                .Add<ItemSaplingBirch>(condition: SkillLumbering.ConditionGetSapplings, count: 1, probability: 0.1)
                .Add<ItemSaplingBirch>(condition: SkillLumbering.ConditionGetExtraSapplings,
                                       count: 1,
                                       probability: 0.1);
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeCircle(radius: 0.25, center: (0.5, 0.47))
                .AddShapeRectangle(size: (0.75, 1),   offset: (0.125, 0.22), group: CollisionGroups.HitboxMelee)
                .AddShapeRectangle(size: (0.4, 0.35), offset: (0.3, 0.95),   group: CollisionGroups.HitboxRanged);
        }
    }
}