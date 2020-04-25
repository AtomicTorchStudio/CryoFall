namespace AtomicTorch.CBND.CoreMod.StaticObjects.Loot
{
    using AtomicTorch.CBND.CoreMod.Items.Devices;
    using AtomicTorch.CBND.CoreMod.Items.Equipment;
    using AtomicTorch.CBND.CoreMod.Items.Equipment.ApartSuit;
    using AtomicTorch.CBND.CoreMod.Items.Equipment.Hazmat;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.CoreMod.Technologies;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class ObjectLootCrateHightech : ProtoObjectLootContainer
    {
        public override string Name => "High-tech crate";

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
            DropItemConditionDelegate T3Specialized = ServerTechTimeGateHelper.IsAvailableT3Specialized;
            DropItemConditionDelegate T4Specialized = ServerTechTimeGateHelper.IsAvailableT4Specialized;

            // common loot
            droplist.Add(nestedList: new DropItemsList(outputs: 1, outputsRandom: 1)
                                     // resources
                                     .Add<ItemToxin>(count: 3,        countRandom: 3,  weight: 1)
                                     .Add<ItemAcidSulfuric>(count: 2, countRandom: 3,  weight: 1)
                                     .Add<ItemAcidNitric>(count: 2,   countRandom: 3,  weight: 1)
                                     .Add<ItemAramidFiber>(count: 5,  countRandom: 5,  weight: 1 / 2.0)
                                     .Add<ItemPlastic>(count: 3,      countRandom: 3,  weight: 1)
                                     .Add<ItemIngotLithium>(count: 2, countRandom: 3,  weight: 1 / 2.0)
                                     .Add<ItemOreLithium>(count: 10,  countRandom: 20, weight: 1 / 2.0));

            // rare loot
            droplist.Add(nestedList: new DropItemsList(outputs: 1)
                                     // components
                                     .Add<ItemComponentsMechanical>(count: 5, countRandom: 10, weight: 1)
                                     .Add<ItemComponentsElectronic>(count: 5, countRandom: 10, weight: 1)
                                     .Add<ItemComponentsOptical>(count: 5,    countRandom: 10, weight: 1 / 5.0)
                                     .Add<ItemComponentsHighTech>(count: 2,   countRandom: 3,  weight: 1 / 10.0)
                                     // items
                                     .Add<ItemBatteryDisposable>(count: 1, countRandom: 2, weight: 1)
                                     .Add<ItemBatteryHeavyDuty>(count: 1,  countRandom: 2, weight: 1 / 10.0)
                                     .Add<ItemPowerCell>(count: 1,         countRandom: 1, weight: 1 / 10.0)
                                     // equipment
                                     .Add<ItemRespirator>(count: 1, weight: 1 / 25.0)
                                     // ensure both hazmat and A.P.A.R.T. are available only after reaching T3 timegate
                                     // otherwise it would be too easy to loot radtowns by acquiring these early
                                     // it's intentional that A.P.A.R.T. (T4) is available starting from T3 here
                                     .Add<ItemHazmatSuit>(count: 1, weight: 1 / 50.0,  condition: T3Specialized)
                                     .Add<ItemApartSuit>(count: 1,  weight: 1 / 100.0, condition: T3Specialized)
                                     // devices
                                     .Add<ItemPowerBankStandard>(count: 1, weight: 1 / 50.0)
                                     .Add<ItemPowerBankLarge>(count: 1,    weight: 1 / 100.0));

            // extra loot from skill
            droplist.Add(condition: SkillSearching.ServerRollExtraLoot,
                         nestedList: new DropItemsList(outputs: 1)
                                     .Add<ItemComponentsMechanical>(count: 1, countRandom: 2)
                                     .Add<ItemComponentsElectronic>(count: 1, countRandom: 2)
                                     .Add<ItemBatteryDisposable>(count: 1,    countRandom: 1));
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeRectangle(size: (0.8, 0.45), offset: (0.1, 0.55))
                .AddShapeRectangle(size: (0.8, 0.5),  offset: (0.1, 0.55), group: CollisionGroups.HitboxMelee)
                .AddShapeRectangle(size: (0.6, 0.15), offset: (0.2, 1.3),  group: CollisionGroups.HitboxRanged)
                .AddShapeRectangle(size: (0.8, 0.8),  offset: (0.1, 0.55), group: CollisionGroups.ClickArea);
        }
    }
}