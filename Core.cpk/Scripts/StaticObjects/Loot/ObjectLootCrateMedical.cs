namespace AtomicTorch.CBND.CoreMod.StaticObjects.Loot
{
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Items.Medical;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.CoreMod.Technologies;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class ObjectLootCrateMedical : ProtoObjectLootContainer
    {
        public override string Name => "Medical crate";

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
            droplist.Add(nestedList: new DropItemsList(outputs: 2, outputsRandom: 0)
                                     // resources
                                     .Add<ItemComponentsPharmaceutical>(count: 10, countRandom: 10, weight: 1)
                                     // basic medical
                                     .Add<ItemBandage>(count: 1,      weight: 1 / 4.0)
                                     .Add<ItemRemedyHerbal>(count: 1, weight: 1 / 4.0)
                                     .Add<ItemPainkiller>(count: 1,   weight: 1 / 10.0)
                                     .Add<ItemAntiNausea>(count: 1,   weight: 1 / 10.0)
                                     .Add<ItemHemostatic>(count: 1,   weight: 1 / 25.0)
                                     .Add<ItemSplint>(count: 1,       weight: 1 / 25.0)
                                     // boost
                                     .Add<ItemStrengthBoostSmall>(count: 1, weight: 1 / 5.0)
                                     .Add<ItemStrengthBoostBig>(count: 1,   weight: 1 / 10.0)
                                     // anti effect
                                     .Add<ItemAntiRadiation>(count: 1,            weight: 1 / 15.0)
                                     .Add<ItemAntiRadiationPreExposure>(count: 1, weight: 1 / 40.0)
                                     .Add<ItemAntiToxin>(count: 1,                weight: 1 / 25.0)
                                     .Add<ItemAntiToxinPreExposure>(count: 1,     weight: 1 / 60.0)
                                     .Add<ItemPsiPreExposure>(count: 1,           weight: 1 / 60.0)
                                     .Add<ItemHeatPreExposure>(count: 1,          weight: 1 / 60.0)
                                     // top tier
                                     .Add<ItemMedkit>(count: 1,         weight: 1 / 40.0)
                                     .Add<ItemStimpack>(count: 1,       weight: 1 / 40.0, condition: T4Specialized)
                                     .Add<ItemPeredozin>(count: 1,      weight: 1 / 50.0, condition: T4Specialized)
                                     .Add<ItemNeuralEnhancer>(count: 1, weight: 1 / 150.0));

            // extra loot from skill
            droplist.Add(condition: SkillSearching.ServerRollExtraLoot,
                         nestedList: new DropItemsList(outputs: 1)
                                     .Add<ItemComponentsPharmaceutical>(count: 2, countRandom: 3)
                                     .Add<ItemStrengthBoostSmall>(count: 1)
                                     .Add<ItemStrengthBoostBig>(count: 1)
                                     .Add<ItemRemedyHerbal>(count: 1)
                                     .Add<ItemBandage>(count: 1));
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