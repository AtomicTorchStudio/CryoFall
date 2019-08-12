namespace AtomicTorch.CBND.CoreMod.StaticObjects.Loot
{
    using AtomicTorch.CBND.CoreMod.Items.Food;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Items.Seeds;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class ObjectLootPileGarbageLarge : ProtoObjectLootContainer
    {
        public override double DurationGatheringSeconds => 1;

        public override string InteractionTooltipText => InteractionTooltipTexts.Examine;

        public override string Name => "Large garbage pile";

        public override ObjectSoundMaterial ObjectSoundMaterial => ObjectSoundMaterial.Stone;

        public override double ObstacleBlockDamageCoef => 0.5;

        public override float StructurePointsMax => 1000;

        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);
            renderer.PositionOffset = (0, 0);
            renderer.SpritePivotPoint = (0, 0);
            renderer.DrawOrderOffsetY = 1.35;
        }

        protected override void CreateLayout(StaticObjectLayout layout)
        {
            layout.Setup("##",
                         "##");
        }

        protected override void PrepareLootDroplist(DropItemsList droplist)
        {
            // common loot
            droplist.Add(nestedList: new DropItemsList(outputs: 1, outputsRandom: 1)
                                     // money
                                     .Add<ItemCoinPenny>(count: 10, countRandom: 10, probability: 1 / 3.0)
                                     // resources
                                     .Add<ItemPlanks>(count: 25,    countRandom: 25, weight: 1 / 1.0)
                                     .Add<ItemStone>(count: 25,     countRandom: 25, weight: 1 / 1.0)
                                     .Add<ItemTwigs>(count: 15,     countRandom: 10, weight: 1 / 1.0)
                                     .Add<ItemFibers>(count: 15,    countRandom: 10, weight: 1 / 1.0)
                                     .Add<ItemTreebark>(count: 3,   countRandom: 3,  weight: 1 / 1.0)
                                     .Add<ItemCharcoal>(count: 5,   countRandom: 5,  weight: 1 / 1.0)
                                     .Add<ItemFoodRotten>(count: 5, countRandom: 5,  weight: 1 / 1.0)
                                     .Add<ItemCoal>(count: 5,       countRandom: 5,  weight: 1 / 5.0)
                                     // misc
                                     .Add<ItemGlassRaw>(count: 5,    countRandom: 15, weight: 1 / 5.0)
                                     .Add<ItemBottleEmpty>(count: 1, countRandom: 2,  weight: 1 / 5.0)
                                     .Add<ItemBottleWater>(count: 1, countRandom: 2,  weight: 1 / 5.0));

            //seeds
            droplist.Add(probability: 1 / 10.0,
                         nestedList: new DropItemsList(outputs: 1, outputsRandom: 1)
                                     .Add<ItemSeedsCarrot>(count: 1,         countRandom: 2)
                                     .Add<ItemSeedsCucumber>(count: 1,       countRandom: 2)
                                     .Add<ItemSeedsTomato>(count: 1,         countRandom: 2)
                                     .Add<ItemSeedsBellPepper>(count: 1,     countRandom: 2)
                                     .Add<ItemSeedsFlowerOni>(count: 1,      countRandom: 2)
                                     .Add<ItemSeedsFlowerBlueSage>(count: 1, countRandom: 2));

            // extra loot
            droplist.Add(condition: SkillSearching.ServerRollExtraLoot,
                         nestedList: new DropItemsList(outputs: 1)
                                     .Add<ItemPlanks>(count: 25,   countRandom: 25)
                                     .Add<ItemStone>(count: 25,    countRandom: 25)
                                     .Add<ItemFibers>(count: 25,   countRandom: 25)
                                     .Add<ItemGlassRaw>(count: 10, countRandom: 10));
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeCircle(radius: 0.6, center: (1, 0.9))
                .AddShapeCircle(radius: 0.8, center: (1, 1), group: CollisionGroups.HitboxMelee)
                .AddShapeRectangle(size: (1.4, 0.2), offset: (0.3, 0.95), group: CollisionGroups.HitboxRanged)
                .AddShapeCircle(radius: 0.85, center: (1, 1), group: CollisionGroups.ClickArea);
        }
    }
}