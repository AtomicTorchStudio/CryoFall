namespace AtomicTorch.CBND.CoreMod.StaticObjects.Minerals
{
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;

    public class ObjectMineralCopper : ProtoObjectMineral
    {
        public override string Name => "Copper deposit";

        public override ObjectMaterial ObjectMaterial => ObjectMaterial.Stone;

        public override float StructurePointsMax => 1000;

        protected override void PrepareProtoMineral(MineralDropItemsConfig config)
        {
            // droplist for stage 1
            config.Stage1
                  .Add<ItemStone>(count: 1,           countRandom: 0)
                  .Add<ItemOreCopper>(count: 5,       countRandom: 0)
                  .Add<ItemOreCopper>(countRandom: 1, condition: SkillMining.ConditionAdditionalYield);

            // droplist for stages 2 and 3 - reuse droplist from stage 1
            config.Stage2.Add(config.Stage1);
            config.Stage3.Add(config.Stage1);

            // droplist for stage 4
            config.Stage4
                  .Add<ItemStone>(count: 2,           countRandom: 0)
                  .Add<ItemOreCopper>(count: 10,      countRandom: 0)
                  .Add<ItemOreCopper>(countRandom: 5, condition: SkillMining.ConditionAdditionalYield)
                  .Add<ItemGoldNugget>(count: 1,      countRandom: 3, probability: 1 / 50.0);

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
    }
}