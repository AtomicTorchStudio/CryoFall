namespace AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation.SmallGatherables
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Food;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class ObjectSmallPineapple : ProtoObjectSmallGatherableVegetation
    {
        public override string Name => "Pineapple";

        protected override TimeSpan TimeToMature => TimeSpan.FromMinutes(30);

        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);
            renderer.DrawOrderOffsetY = 0.2;
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
                .Add<ItemPineapple>(count: 1)
                .Add<ItemFibers>(count: 3, countRandom: 2);
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeCircle(
                    radius: 0.17,
                    center: (0.5, 0.33))
                .AddShapeRectangle(
                    offset: (0.35, 0.2),
                    size: (0.3, 0.6),
                    group: CollisionGroups.HitboxMelee)
                .AddShapeRectangle(
                    offset: (0.35, 0.2),
                    size: (0.3, 0.6),
                    group: CollisionGroups.HitboxRanged)
                .AddShapeCircle(
                    radius: 0.4,
                    center: (0.5, 0.5),
                    group: CollisionGroups.ClickArea);
        }
    }
}