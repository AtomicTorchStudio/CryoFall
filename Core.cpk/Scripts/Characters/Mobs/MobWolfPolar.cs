namespace AtomicTorch.CBND.CoreMod.Characters.Mobs
{
    using AtomicTorch.CBND.CoreMod.CharacterSkeletons;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.Stats;
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;

    public class MobWolfPolar : MobWolf
    {
        public override bool AiIsRunAwayFromHeavyVehicles => false;

        public override string Name => "Polar wolf";

        public override double StatDefaultHealthMax => 160;

        public override double StatMoveSpeed => 2.1;

        protected override void FillDefaultEffects(Effects effects)
        {
            base.FillDefaultEffects(effects);

            effects.AddValue(this, StatName.DefenseImpact, 0.4);
            effects.AddValue(this, StatName.DefenseCold, 0.3);
        }

        protected override void PrepareProtoCharacterMob(
            out ProtoCharacterSkeleton skeleton,
            ref double scale,
            DropItemsList lootDroplist)
        {
            skeleton = GetProtoEntity<SkeletonWolfPolar>();

            // primary loot
            lootDroplist
                .Add<ItemFur>(count: 1,       countRandom: 2)
                .Add<ItemBones>(count: 1,     countRandom: 2)
                .Add<ItemAnimalFat>(count: 1, probability: 1 / 3.0);

            // extra loot
            lootDroplist.Add(condition: SkillHunting.ServerRollExtraLoot,
                             nestedList: new DropItemsList(outputs: 2)
                                         .Add<ItemFur>(count: 1)
                                         .Add<ItemBones>(count: 1)
                                         .Add<ItemAnimalFat>(count: 1, weight: 1 / 2.0));
        }
    }
}