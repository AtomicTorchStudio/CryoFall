﻿namespace AtomicTorch.CBND.CoreMod.StaticObjects.Minerals
{
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;

    public class ObjectMineralSaltpeter : ProtoObjectMineral
    {
        public override string Name => "Saltpeter deposit";

        public override ObjectMaterial ObjectMaterial => ObjectMaterial.Stone;

        public override float StructurePointsMax => 1000;

        protected override void PrepareProtoMineral(MineralDropItemsConfig config)
        {
            // droplist for stage 1
            config.Stage1
                  .Add<ItemStone>(count: 1)
                  .Add<ItemPotassiumNitrate>(count: 5)
                  .Add<ItemPotassiumNitrate>(countRandom: 1, condition: SkillProspecting.ConditionAdditionalYield);

            // droplist for stages 2 and 3 - reuse droplist from stage 1
            config.Stage2.Add(config.Stage1);
            config.Stage3.Add(config.Stage1);

            // droplist for stage 4
            config.Stage4
                  .Add<ItemStone>(count: 2)
                  .Add<ItemPotassiumNitrate>(count: 10)
                  .Add<ItemPotassiumNitrate>(countRandom: 5, condition: SkillProspecting.ConditionAdditionalYield)
                  .Add(preset: ItemDroplistPresets.GoldNuggets);

            // drop gemstones
            config.Stage4
                  .Add(condition: SkillProspecting.ConditionDropGemstones,
                       preset: ItemDroplistPresets.Gemstones);
        }
    }
}