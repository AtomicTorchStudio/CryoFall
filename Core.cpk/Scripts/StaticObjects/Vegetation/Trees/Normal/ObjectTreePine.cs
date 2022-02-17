namespace AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation.Trees
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Items.Seeds;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class ObjectTreePine : ProtoObjectTree
    {
        public override string Name => "Spruce tree";

        public override double TreeHeight => 2.2;

        protected override TimeSpan TimeToMature => TimeSpan.FromHours(2);

        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);
            renderer.DrawOrderOffsetY = 0.25;
        }

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
                .Add<ItemSaplingPine>(condition: SkillLumbering.ConditionGetSapplings,      count: 1, probability: 0.1)
                .Add<ItemSaplingPine>(condition: SkillLumbering.ConditionGetExtraSapplings, count: 1, probability: 0.1);
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeRectangle(size: (0.8, 0.4),  offset: (0.1, 0.1))
                .AddShapeRectangle(size: (0.8, 1),    offset: (0.1, 0.1), group: CollisionGroups.HitboxMelee)
                .AddShapeRectangle(size: (0.4, 0.35), offset: (0.3, 0.8), group: CollisionGroups.HitboxRanged);
        }
    }
}