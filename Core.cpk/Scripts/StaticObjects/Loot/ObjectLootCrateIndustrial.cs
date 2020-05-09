namespace AtomicTorch.CBND.CoreMod.StaticObjects.Loot
{
    using AtomicTorch.CBND.CoreMod.Items.Equipment;
    using AtomicTorch.CBND.CoreMod.Items.Explosives.Bombs;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Items.Tools;
    using AtomicTorch.CBND.CoreMod.Items.Tools.Axes;
    using AtomicTorch.CBND.CoreMod.Items.Tools.Crowbars;
    using AtomicTorch.CBND.CoreMod.Items.Tools.Pickaxes;
    using AtomicTorch.CBND.CoreMod.Items.Tools.Toolboxes;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class ObjectLootCrateIndustrial : ProtoObjectLootContainer
    {
        public override string Name => "Industrial crate";

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
            droplist.Add(nestedList: new DropItemsList(outputs: 2, outputsRandom: 0)
                                     // money
                                     .Add<ItemCoinPenny>(count: 10, countRandom: 30, weight: 1 / 1.0)
                                     // resources
                                     .Add<ItemPlanks>(count: 30,     countRandom: 50,  weight: 1 / 1.0)
                                     .Add<ItemIngotCopper>(count: 5, countRandom: 15,  weight: 1 / 1.0)
                                     .Add<ItemIngotIron>(count: 5,   countRandom: 15,  weight: 1 / 1.0)
                                     .Add<ItemIngotSteel>(count: 5,  countRandom: 10,  weight: 1 / 4.0)
                                     .Add<ItemClay>(count: 30,       countRandom: 20,  weight: 1 / 4.0)
                                     .Add<ItemCoal>(count: 5,        countRandom: 15,  weight: 1 / 4.0)
                                     .Add<ItemOreCopper>(count: 25,  countRandom: 75,  weight: 1 / 10.0)
                                     .Add<ItemOreIron>(count: 25,    countRandom: 75,  weight: 1 / 10.0)
                                     .Add<ItemSand>(count: 50,       countRandom: 100, weight: 1 / 10.0)
                                     .Add<ItemStone>(count: 50,      countRandom: 100, weight: 1 / 10.0)
                                     .Add<ItemCement>(count: 10,     countRandom: 40,  weight: 1 / 10.0)
                                     // items
                                     .Add<ItemGlue>(count: 2,             countRandom: 3,  weight: 1 / 10.0)
                                     .Add<ItemFertilizer>(count: 2,       countRandom: 3,  weight: 1 / 10.0)
                                     .Add<ItemRubberRaw>(count: 10,       countRandom: 20, weight: 1 / 10.0)
                                     .Add<ItemRubberVulcanized>(count: 3, countRandom: 2,  weight: 1 / 10.0));

            // rare loot
            droplist.Add(probability: 1 / 3.0,
                         nestedList: new DropItemsList(outputs: 1)
                                     // components
                                     .Add<ItemComponentsMechanical>(count: 5,          countRandom: 5, weight: 1 / 1.0)
                                     .Add<ItemComponentsElectronic>(count: 5,          countRandom: 5, weight: 1 / 1.0)
                                     .Add<ItemComponentsOptical>(count: 5,             countRandom: 5, weight: 1 / 10.0)
                                     .Add<ItemComponentsIndustrialChemicals>(count: 5, countRandom: 5, weight: 1 / 10.0)
                                     // items
                                     .Add<ItemFirelog>(count: 2,           countRandom: 3, weight: 1 / 1.0)
                                     .Add<ItemBatteryDisposable>(count: 2, countRandom: 3, weight: 1 / 10.0)
                                     .Add<ItemBombMining>(count: 2,        countRandom: 3, weight: 1 / 25.0)
                                     // tools
                                     .Add<ItemAxeIron>(count: 1,      weight: 1 / 25.0)
                                     .Add<ItemPickaxeIron>(count: 1,  weight: 1 / 25.0)
                                     .Add<ItemToolboxT2>(count: 1,    weight: 1 / 25.0)
                                     .Add<ItemAxeSteel>(count: 1,     weight: 1 / 50.0)
                                     .Add<ItemPickaxeSteel>(count: 1, weight: 1 / 50.0)
                                     .Add<ItemToolboxT3>(count: 1,    weight: 1 / 50.0)
                                     .Add<ItemCrowbar>(count: 1,      weight: 1 / 25.0)
                                     // equipment
                                     .Add<ItemHelmetSafety>(count: 1, weight: 1 / 15.0));

            // extra loot from skill
            droplist.Add(condition: SkillSearching.ServerRollExtraLoot,
                         nestedList: new DropItemsList(outputs: 1)
                                     .Add<ItemComponentsMechanical>(count: 1, countRandom: 1)
                                     .Add<ItemComponentsElectronic>(count: 1, countRandom: 1)
                                     .Add<ItemBatteryDisposable>(count: 1,    countRandom: 1));
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