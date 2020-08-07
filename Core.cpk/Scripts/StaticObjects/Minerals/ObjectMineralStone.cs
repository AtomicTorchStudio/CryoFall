namespace AtomicTorch.CBND.CoreMod.StaticObjects.Minerals
{
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;

    public class ObjectMineralStone : ProtoObjectMineral
    {
        public override string Name => "Stone";

        public override ObjectMaterial ObjectMaterial => ObjectMaterial.Stone;

        public override float StructurePointsMax => 500;

        public override byte TextureVariantsCount => 2;

        protected override void PrepareProtoMineral(MineralDropItemsConfig config)
        {
            // droplist for stage 1
            config.Stage1
                  .Add<ItemStone>(count: 5)
                  .Add<ItemStone>(countRandom: 1, condition: SkillProspecting.ConditionAdditionalYield);

            // droplist for stages 2 and 3 - reuse droplist from stage 1
            config.Stage2.Add(config.Stage1);
            config.Stage3.Add(config.Stage1);

            // droplist for stage 4
            config.Stage4
                  .Add<ItemStone>(count: 10)
                  .Add<ItemStone>(countRandom: 5, condition: SkillProspecting.ConditionAdditionalYield)
                  // extra stuff
                  .Add<ItemCoal>(count: 2, countRandom: 2, probability: 1 / 10.0)
                  .Add<ItemSalt>(count: 3, countRandom: 3, probability: 1 / 10.0)
                  .Add(preset: ItemDroplistPresets.GoldNuggets);

            // drop gemstones
            config.Stage4
                  .Add(condition: SkillProspecting.ConditionDropGemstones,
                       preset: ItemDroplistPresets.Gemstones);
        }
    }
}