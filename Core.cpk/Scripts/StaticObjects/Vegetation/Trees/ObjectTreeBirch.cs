namespace AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation.Trees
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Items.Seeds;
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Resources;

    public class ObjectTreeBirch : ProtoObjectTree
    {
        public override string Name => "Birch tree";

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
            droplist.Add<ItemLogs>(count: 4);

            // saplings
            droplist.Add<ItemSaplingBirch>(count: 1, probability: 0.2);

            // bonus drop
            droplist.Add<ItemTwigs>(count: 1, countRandom: 2, probability: 0.25);
            droplist.Add<ItemLeaf>(count: 2, countRandom: 3, probability: 0.5);
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            const double offsetY = 0.12;
            data.PhysicsBody
                .AddShapeCircle(
                    radius: 0.25,
                    center: (0.5, offsetY + 0.35))
                .AddShapeRectangle(
                    size: (0.75, 1),
                    offset: (0.125, offsetY + 0.1),
                    group: CollisionGroups.HitboxMelee)
                .AddShapeRectangle(
                    size: (0.6, 1.1),
                    offset: (0.2, offsetY + 0.1),
                    group: CollisionGroups.HitboxRanged);
        }
    }
}