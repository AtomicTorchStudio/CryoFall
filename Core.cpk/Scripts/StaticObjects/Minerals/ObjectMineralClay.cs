namespace AtomicTorch.CBND.CoreMod.StaticObjects.Minerals
{
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Data.World;

    public class ObjectMineralClay : ProtoObjectMineral
    {
        // we don't want to see any decals under it
        public override StaticObjectKind Kind => StaticObjectKind.Structure;

        public override string Name => "Clay";

        public override ObjectMaterial ObjectMaterial => ObjectMaterial.SolidGround;

        public override float StructurePointsMax => 250;

        protected override void PrepareProtoMineral(MineralDropItemsConfig config)
        {
            // droplist for stage 1
            config.Stage1
                  .Add<ItemClay>(count: 5,       countRandom: 0)
                  .Add<ItemClay>(countRandom: 1, condition: SkillProspecting.ConditionAdditionalYield);

            // droplist for stages 2 and 3 - reuse droplist from stage 1
            config.Stage2.Add(config.Stage1);
            config.Stage3.Add(config.Stage1);

            // droplist for stage 4
            config.Stage4
                  .Add<ItemClay>(count: 10,      countRandom: 0)
                  .Add<ItemClay>(countRandom: 5, condition: SkillProspecting.ConditionAdditionalYield)
                  .Add<ItemGoldNugget
                      >(count: 1, countRandom: 3, probability: 1 / 100.0); // lower chance, since it's easy to mine

            // drop gemstones
            config.Stage4
                  .Add(condition: SkillProspecting.ConditionDropGemstones,
                       probability: 1 / 1000.0,
                       nestedList: new DropItemsList(outputs: 1)
                                   .Add<ItemGemDiamond>()
                                   .Add<ItemGemEmerald>()
                                   .Add<ItemGemRuby>()
                                   .Add<ItemGemSapphire>()
                                   .Add<ItemGemTopaz>()
                                   .Add<ItemGemTourmaline>());
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeRectangle(size: (1, 0.3),    offset: (0.0, 0.35))
                .AddShapeRectangle(size: (0.9, 0.5),  offset: (0.05, 0.3), group: CollisionGroups.HitboxMelee)
                .AddShapeRectangle(size: (0.8, 0.15), offset: (0.1, 0.85), group: CollisionGroups.HitboxRanged)
                .AddShapeLineSegment(point1: (0.5, 0.2), point2: (0.5, 0.85), group: CollisionGroups.HitboxRanged);
        }
    }
}