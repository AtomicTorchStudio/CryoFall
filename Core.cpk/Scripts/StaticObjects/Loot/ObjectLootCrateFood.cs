namespace AtomicTorch.CBND.CoreMod.StaticObjects.Loot
{
    using AtomicTorch.CBND.CoreMod.Items.Food;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Items.Medical;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class ObjectLootCrateFood : ProtoObjectLootContainer
    {
        public override string Name => "Food supplies crate";

        public override ObjectSoundMaterial ObjectSoundMaterial => ObjectSoundMaterial.Wood;

        public override double ObstacleBlockDamageCoef => 0.5;

        public override float StructurePointsMax => 1000;

        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);
            renderer.PositionOffset += (0, 0.5);
            renderer.DrawOrderOffsetY = 0.25;
        }

        protected override void PrepareLootDroplist(DropItemsList droplist)
        {
            // common loot
            droplist.Add(nestedList: new DropItemsList(outputs: 2, outputsRandom: 0)
                                     // drinks
                                     .Add<ItemBottleWater>(count: 2,      countRandom: 1, weight: 1 / 5.0)
                                     .Add<ItemBottleWaterStale>(count: 2, countRandom: 1, weight: 1 / 10.0)
                                     .Add<ItemBottleWaterSalty>(count: 2, countRandom: 1, weight: 1 / 10.0)
                                     .Add<ItemDrinkEnergy>(count: 1,      countRandom: 1, weight: 1 / 5.0)
                                     .Add<ItemDrinkHerbal>(count: 1,      countRandom: 1, weight: 1 / 5.0)
                                     .Add<ItemDrinkSoft>(count: 1,        countRandom: 1, weight: 1 / 5.0)
                                     // alcohol & cigars
                                     .Add<ItemBeer>(count: 1,         countRandom: 1, weight: 1 / 10.0)
                                     .Add<ItemWine>(count: 1,         countRandom: 1, weight: 1 / 10.0)
                                     .Add<ItemVodka>(count: 1,        countRandom: 1, weight: 1 / 10.0)
                                     .Add<ItemLiquor>(count: 1,       countRandom: 1, weight: 1 / 10.0)
                                     .Add<ItemTequila>(count: 1,      countRandom: 1, weight: 1 / 10.0)
                                     .Add<ItemTincture>(count: 1,     countRandom: 1, weight: 1 / 10.0)
                                     .Add<ItemCigarCheap>(count: 1,   countRandom: 1, weight: 1 / 10.0)
                                     .Add<ItemCigarNormal>(count: 1,  countRandom: 1, weight: 1 / 15.0)
                                     .Add<ItemCigarPremium>(count: 1, countRandom: 1, weight: 1 / 20.0)
                                     // food
                                     .Add<ItemMeatJerky>(count: 1,       countRandom: 1, weight: 1 / 10.0)
                                     .Add<ItemCannedBeans>(count: 1,     countRandom: 1, weight: 1 / 10.0)
                                     .Add<ItemCannedFish>(count: 1,      countRandom: 1, weight: 1 / 10.0)
                                     .Add<ItemCannedMeat>(count: 1,      countRandom: 1, weight: 1 / 10.0)
                                     .Add<ItemCannedMixedMeat>(count: 1, countRandom: 1, weight: 1 / 10.0)
                                     .Add<ItemCoffeeBeans>(count: 10,    countRandom: 5, weight: 1 / 10.0)
                                     // ingredients
                                     .Add<ItemWheatFlour>(count: 10,  countRandom: 5,  weight: 1 / 10.0)
                                     .Add<ItemWheatGrains>(count: 20, countRandom: 20, weight: 1 / 10.0)
                                     .Add<ItemRice>(count: 15,        countRandom: 10, weight: 1 / 10.0)
                                     .Add<ItemSugar>(count: 5,        countRandom: 5,  weight: 1 / 10.0)
                                     .Add<ItemSalt>(count: 15,        countRandom: 10, weight: 1 / 10.0));

            // extra loot from skill
            droplist.Add(condition: SkillSearching.ServerRollExtraLoot,
                         nestedList: new DropItemsList(outputs: 1)
                                     .Add<ItemBeer>(count: 1)
                                     .Add<ItemWine>(count: 1)
                                     .Add<ItemVodka>(count: 1)
                                     .Add<ItemWheatFlour>(count: 5,  countRandom: 5)
                                     .Add<ItemWheatGrains>(count: 5, countRandom: 10)
                                     .Add<ItemSugar>(count: 3,       countRandom: 3)
                                     .Add<ItemSalt>(count: 10,       countRandom: 5));
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeRectangle(size: (0.8, 0.35), offset: (0.1, 0.65))
                .AddShapeRectangle(size: (0.8, 0.4),  offset: (0.1, 0.65), group: CollisionGroups.HitboxMelee)
                .AddShapeRectangle(size: (0.6, 0.15), offset: (0.2, 1.3),  group: CollisionGroups.HitboxRanged)
                .AddShapeRectangle(size: (0.8, 0.6),  offset: (0.1, 0.65), group: CollisionGroups.ClickArea);
        }
    }
}