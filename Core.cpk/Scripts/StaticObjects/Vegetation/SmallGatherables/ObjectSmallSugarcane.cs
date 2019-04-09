namespace AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation.SmallGatherables
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Food;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Loot;
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class ObjectSmallSugarcane : ProtoObjectSmallGatherableVegetation
    {
        public override string Name => "Sugar cane";

        protected override bool CanFlipSprite => false;

        protected override TimeSpan TimeToMature => TimeSpan.FromMinutes(30);

        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);
            renderer.DrawOrderOffsetY = 0.33;

            ClientGrassRenderingHelper.Setup(renderer,
                                             power: 0.025f,
                                             pivotY: 0.05f);
        }

        protected override void PrepareGatheringDroplist(DropItemsList droplist)
        {
            droplist
                .Add<ItemSugar>(count: 2,  countRandom: 1)
                .Add<ItemFibers>(count: 3, countRandom: 2);

            // skill bonus
            droplist
                .Add<ItemSugar>(count: 1,  probability: 1 / 5.0, condition: SkillForaging.ConditionAdditionalYield)
                .Add<ItemFibers>(count: 1, condition: SkillForaging.ConditionAdditionalYield);
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeCircle(
                    radius: 0.3,
                    center: (0.5, 0.33))
                .AddShapeRectangle(
                    offset: (0.25, 0.2),
                    size: (0.5, 0.6),
                    group: CollisionGroups.HitboxMelee)
                .AddShapeRectangle(
                    offset: (0.25, 0.2),
                    size: (0.5, 0.6),
                    group: CollisionGroups.HitboxRanged)
                .AddShapeRectangle(
                    offset: (0.1, 0.1),
                    size: (0.8, 0.9),
                    group: CollisionGroups.ClickArea);
        }
    }
}