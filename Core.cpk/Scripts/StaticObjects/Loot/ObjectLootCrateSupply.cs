﻿namespace AtomicTorch.CBND.CoreMod.StaticObjects.Loot
{
    using AtomicTorch.CBND.CoreMod.Items.Equipment;
    using AtomicTorch.CBND.CoreMod.Items.Fishing;
    using AtomicTorch.CBND.CoreMod.Items.Food;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Items.Seeds;
    using AtomicTorch.CBND.CoreMod.Items.Tools.Axes;
    using AtomicTorch.CBND.CoreMod.Items.Tools.Pickaxes;
    using AtomicTorch.CBND.CoreMod.Items.Tools.Toolboxes;
    using AtomicTorch.CBND.CoreMod.Items.Tools.WateringCans;
    using AtomicTorch.CBND.CoreMod.Rates;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class ObjectLootCrateSupply : ProtoObjectLootContainer
    {
        public override string Name => "Supply crate";

        public override ObjectMaterial ObjectMaterial => ObjectMaterial.Wood;

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
            droplist.Add(
                nestedList:
                new DropItemsList(outputs: 2, outputsRandom: 0)
                    // money
                    .Add<ItemCoinPenny>(count: 10, countRandom: 30, probability: 1 / 1.0)
                    .Add<ItemCoinPurse>(count: 1,  weight: 1 / 5.0)
                    // resources
                    .Add<ItemPlanks>(count: 20,     countRandom: 30, weight: 1 / 1.0)
                    .Add<ItemThread>(count: 10,     countRandom: 10, weight: 1 / 1.0)
                    .Add<ItemRope>(count: 2,        countRandom: 2,  weight: 1 / 5.0)
                    .Add<ItemPaper>(count: 10,      countRandom: 15, weight: 1 / 2.0)
                    .Add<ItemBottleEmpty>(count: 1, countRandom: 2,  weight: 1 / 5.0)
                    .Add<ItemGlassRaw>(count: 20,   countRandom: 10, weight: 1 / 2.0)
                    .Add<ItemCement>(count: 2,      countRandom: 8,  weight: 1 / 10.0)
                    .Add<ItemRubberRaw>(count: 5,   countRandom: 10, weight: 1 / 5.0)
                    .Add<ItemFirelog>(count: 2,     countRandom: 1,  weight: 1 / 10.0)
                    // components
                    .Add<ItemComponentsMechanical>(count: 10, countRandom: 10, weight: 1 / 5.0)
                    .Add<ItemComponentsElectronic>(count: 5,  countRandom: 5,  weight: 1 / 5.0)
                    // items
                    .Add<ItemCampFuel>(count: 2, countRandom: 3, weight: 1 / 5.0));

            // rare loot
            droplist.Add(
                    probability: 1 / 4.0,
                    nestedList:
                    new DropItemsList(outputs: 1)
                        .Add(weight: 1 / 2.0,
                             nestedList:
                             new DropItemsList(outputs: 1)
                                 // equipment
                                 .Add<ItemLeatherArmor>(count: 1,  weight: 1 / 3.0)
                                 .Add<ItemLeatherHelmet>(count: 1, weight: 1 / 3.0)
                                 // tools
                                 .Add<ItemAxeIron>(count: 1,            weight: 1 / 5.0)
                                 .Add<ItemPickaxeIron>(count: 1,        weight: 1 / 5.0)
                                 .Add<ItemToolboxT2>(count: 1,          weight: 1 / 5.0)
                                 .Add<ItemWateringCanCopper>(count: 1,  weight: 1 / 5.0)
                                 .Add<ItemWateringCanSteel>(count: 1,   weight: 1 / 10.0)
                                 .Add<ItemWateringCanPlastic>(count: 1, weight: 1 / 20.0)
                                 // fishing
                                 .Add<ItemFishingBaitInsect>(count: 1, weight: 1 / 20.0)
                                 .Add<ItemFishingBaitFish>(count: 1,   weight: 1 / 20.0)
                                 .Add<ItemFishingBaitMix>(count: 1,    weight: 1 / 20.0)
                                 // items
                                 .Add<ItemMRE>(count: 1, countRandom: 2, weight: 1 / 20.0)
                            )
                        .Add(weight: 1 / 1.0,
                             nestedList:
                             new DropItemsList(outputs: 2, outputsRandom: 1)
                                 // seeds - primary
                                 .Add<ItemSeedsCarrot>(count: 2,     countRandom: 1)
                                 .Add<ItemSeedsCucumber>(count: 2,   countRandom: 1)
                                 .Add<ItemSeedsTomato>(count: 2,     countRandom: 1)
                                 .Add<ItemSeedsBellPepper>(count: 2, countRandom: 1)
                                 .Add<ItemSeedsCorn>(count: 2,       countRandom: 1)
                                 .Add<ItemSeedsPotato>(count: 2,     countRandom: 1)
                                 // seeds - rare
                                 .Add<ItemSeedsChiliPepper>(count: 2, countRandom: 1)
                                 .Add<ItemSeedsMilkmelon>(count: 2,   countRandom: 1)
                                 .Add<ItemSeedsWheat>(count: 2,       countRandom: 1)
                                 .Add<ItemSeedsRice>(count: 2,        countRandom: 1)
                                 .Add<ItemSeedsSpices>(count: 2,      countRandom: 1)
                                 .Add<ItemSeedsTobacco>(count: 2,     countRandom: 1)
                                 // seeds - misc
                                 .Add<ItemSeedsFlowerOni>(count: 2,      countRandom: 1)
                                 .Add<ItemSeedsFlowerBlueSage>(count: 2, countRandom: 1)
                                 .Add<ItemSeedsFlowerYellow>(count: 2,   countRandom: 1)
                            )
                );

            // extra loot from skill
            droplist.Add(
                condition: SkillSearching.ServerRollExtraLoot,
                nestedList:
                new DropItemsList(outputs: 1)
                    .Add<ItemFirelog>(count: 1, countRandom: 1)
                    .Add<ItemGlue>(count: 1,    countRandom: 2)
                    .Add<ItemComponentsMechanical>(count: 2)
                    .Add<ItemComponentsElectronic>(count: 1));
        }

        protected override double ServerGetDropListRate()
        {
            return RateResourcesGatherCratesLoot.SharedValue;
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeRectangle(size: (0.8, 0.35), offset: (0.1, 0.65))
                .AddShapeRectangle(size: (0.8, 0.4),  offset: (0.1, 0.65), group: CollisionGroups.HitboxMelee)
                .AddShapeRectangle(size: (0.8, 0.6),  offset: (0.1, 0.65), group: CollisionGroups.ClickArea);
        }
    }
}