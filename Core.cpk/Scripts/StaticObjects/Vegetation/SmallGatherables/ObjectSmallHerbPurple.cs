namespace AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation.SmallGatherables
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Medical;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class ObjectSmallHerbPurple : ProtoObjectSmallGatherableVegetation
    {
        public override string Name => "Purple herb";

        protected override TimeSpan TimeToMature => TimeSpan.FromMinutes(30);

        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);
            renderer.DrawOrderOffsetY = 0.33;
        }

        protected override ITextureResource PrepareDefaultTexture(Type thisType)
        {
            return new TextureAtlasResource(
                base.PrepareDefaultTexture(thisType),
                columns: 3,
                rows: 1);
        }

        protected override void PrepareGatheringDroplist(DropItemsList droplist)
        {
            droplist
                .Add<ItemHerbPurple>(count: 1)
                .Add<ItemHerbPurple>(count: 1, probability: 1 / 5.0, condition: SkillForaging.ConditionAdditionalYield);
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeCircle(
                    radius: 0.12,
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
                    radius: 0.45,
                    center: (0.5, 0.5),
                    group: CollisionGroups.ClickArea);
        }
    }
}