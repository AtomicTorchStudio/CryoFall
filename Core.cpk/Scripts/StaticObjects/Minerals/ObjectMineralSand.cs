namespace AtomicTorch.CBND.CoreMod.StaticObjects.Minerals
{
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class ObjectMineralSand : ProtoObjectMineral
    {
        // we don't want to see any decals under it
        public override StaticObjectKind Kind => StaticObjectKind.Structure;

        public override string Name => "Quartz sand deposit";

        public override ObjectSoundMaterial ObjectSoundMaterial => ObjectSoundMaterial.SolidGround;

        public override float StructurePointsMax => 250;

        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);
            renderer.SpritePivotPoint = (0.5, 0.5);
            renderer.PositionOffset = (0.5, 0.5);
            renderer.DrawOrderOffsetY = -0.15;
        }

        protected override void PrepareProtoMineral(MineralDropItemsConfig config)
        {
            // droplist for stage 1
            config.Stage1
                  .Add<ItemSand>(count: 5,       countRandom: 0)
                  .Add<ItemSand>(countRandom: 1, condition: SkillMining.ConditionAdditionalYield);

            // droplist for stages 2 and 3 - reuse droplist from stage 1
            config.Stage2.Add(config.Stage1);
            config.Stage3.Add(config.Stage1);

            // droplist for stage 4
            config.Stage4.Add<ItemSand>(count: 10, countRandom: 0)
                  .Add<ItemSand>(countRandom: 5, condition: SkillMining.ConditionAdditionalYield)
                  .Add<ItemGoldNugget
                      >(count: 1, countRandom: 3, probability: 1 / 100.0); // lower chance, since it's easy to mine

            // drop gemstones
            config.Stage4
                  .Add(condition: SkillMining.ConditionDropGemstones,
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
                .AddShapeRectangle(
                    size: (1, 0.3),
                    offset: (0.0, 0.35))
                .AddShapeRectangle(
                    size: (0.9, 0.5),
                    offset: (0.05, 0.3),
                    group: CollisionGroups.HitboxMelee)
                .AddShapeRectangle(
                    size: (1, 0.6),
                    offset: (0, 0.25),
                    group: CollisionGroups.HitboxRanged);
        }
    }
}