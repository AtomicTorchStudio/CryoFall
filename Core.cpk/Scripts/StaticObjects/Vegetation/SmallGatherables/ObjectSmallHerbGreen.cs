namespace AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation.SmallGatherables
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Medical;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Loot;
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class ObjectSmallHerbGreen : ProtoObjectSmallGatherableVegetation
    {
        public override string Name => "Green herb";

        protected override bool CanFlipSprite => false; // it's grass - flip is done by shader

        protected override TimeSpan TimeToMature => TimeSpan.Zero;

        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);
            renderer.DrawOrderOffsetY = 0.33;

            ClientGrassRenderingHelper.Setup(renderer,
                                             power: 0.1f,
                                             pivotY: 0.3f);
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
                .Add<ItemHerbGreen>(count: 1)
                .Add<ItemHerbGreen>(count: 1, probability: 1 / 3.0, condition: SkillForaging.ConditionAdditionalYield);
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeRectangle(offset: (0.35, 0.2), size: (0.3, 0.6), group: CollisionGroups.HitboxMelee)
                .AddShapeCircle(radius: 0.45, center: (0.5, 0.5), group: CollisionGroups.ClickArea);
            // no ranged hitbox
        }
    }
}