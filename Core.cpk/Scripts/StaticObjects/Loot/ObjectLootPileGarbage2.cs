namespace AtomicTorch.CBND.CoreMod.StaticObjects.Loot
{
    using AtomicTorch.CBND.CoreMod.Items.Food;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ObjectLootPileGarbage2 : ProtoObjectLootContainer
    {
        public override double DurationGatheringSeconds => 1;

        public override string InteractionTooltipText => InteractionTooltipTexts.Examine;

        public override bool IsAutoTakeAll => true;

        public override string Name => GetProtoEntity<ObjectLootPileGarbage1>().Name;

        public override ObjectMaterial ObjectMaterial => ObjectMaterial.Stone;

        public override double ObstacleBlockDamageCoef => 0.5;

        // It's much easier to find and search these piles so they will provide much less skill experience.
        public override double SearchingSkillExperienceMultiplier => 0.2;

        public override float StructurePointsMax => 1000;

        protected override bool CanFlipSprite => true;

        public override Vector2D SharedGetObjectCenterWorldOffset(IWorldObject worldObject)
            => (0.5, 0.3);

        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);
            renderer.DrawOrderOffsetY = 0.5;
        }

        protected override void PrepareLootDroplist(DropItemsList droplist)
        {
            // common loot
            droplist.Add(nestedList:
                         new DropItemsList(outputs: 1, outputsRandom: 1)
                             // money
                             .Add<ItemCoinPenny>(count: 10, countRandom: 10, probability: 1 / 5.0)
                             // resources
                             .Add<ItemPlanks>(count: 25,  countRandom: 25, weight: 1 / 1.0)
                             .Add<ItemStone>(count: 25,   countRandom: 25, weight: 1 / 1.0)
                             .Add<ItemTwigs>(count: 15,   countRandom: 10, weight: 1 / 1.0)
                             .Add<ItemFibers>(count: 15,  countRandom: 10, weight: 1 / 1.0)
                             .Add<ItemCharcoal>(count: 5, countRandom: 5,  weight: 1 / 1.0)
                             .Add<ItemRot>(count: 5,      countRandom: 5,  weight: 1 / 1.0)
                             .Add<ItemCoal>(count: 5,     countRandom: 5,  weight: 1 / 5.0)
                             // misc
                             .Add<ItemGlassRaw>(count: 5,    countRandom: 15, weight: 1 / 5.0)
                             .Add<ItemBottleEmpty>(count: 1, countRandom: 2,  weight: 1 / 5.0)
                             .Add<ItemBottleWater>(count: 1, countRandom: 2,  weight: 1 / 5.0));

            // extra loot
            droplist.Add(condition: SkillSearching.ServerRollExtraLoot,
                         nestedList:
                         new DropItemsList(outputs: 1)
                             .Add<ItemPlanks>(count: 25,   countRandom: 25)
                             .Add<ItemStone>(count: 25,    countRandom: 25)
                             .Add<ItemFibers>(count: 25,   countRandom: 25)
                             .Add<ItemGlassRaw>(count: 10, countRandom: 10));
        }

        protected override ReadOnlySoundPreset<ObjectSound> PrepareSoundPresetObject()
        {
            return ObjectsSoundsPresets.ObjectGarbagePile;
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeCircle(radius: 0.32, center: (0.5, 0.35))
                .AddShapeCircle(radius: 0.32, center: (0.5, 0.4),  group: CollisionGroups.HitboxMelee)
                .AddShapeCircle(radius: 0.35, center: (0.5, 0.35), group: CollisionGroups.ClickArea);
        }
    }
}