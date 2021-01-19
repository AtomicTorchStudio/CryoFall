namespace AtomicTorch.CBND.CoreMod.Characters.Mobs
{
    using AtomicTorch.CBND.CoreMod.CharacterSkeletons;
    using AtomicTorch.CBND.CoreMod.Items.Food;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Objects;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Stats;
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;
    using AtomicTorch.CBND.GameApi.Data.World;

    public class MobPsiGrove : ProtoCharacterMob, IProtoObjectPsiSource
    {
        public override bool AiIsRunAwayFromHeavyVehicles => false;

        public override float CharacterWorldHeight => 1.5f;

        public override double MobKillExperienceMultiplier => 1.0;

        public override string Name => "Psi grove";

        public override ObjectMaterial ObjectMaterial => ObjectMaterial.SoftTissues;

        public double PsiIntensity => 0.75;

        public double PsiRadiusMax => 6;

        public double PsiRadiusMin => 4;

        public override double ServerUpdateIntervalSeconds => 1; // rare updates as it's a static mob without attacks

        public override double StatDefaultHealthMax => 200;

        public override double StatMoveSpeed => 0;

        public bool ServerIsPsiSourceActive(IWorldObject worldObject)
        {
            return true;
        }

        protected override void FillDefaultEffects(Effects effects)
        {
            base.FillDefaultEffects(effects);

            effects
                .AddValue(this, StatName.DefenseKinetic,   1.0)
                .AddValue(this, StatName.DefenseHeat,      1.0)
                .AddValue(this, StatName.DefenseCold,      0.5)
                .AddValue(this, StatName.DefenseExplosion, 0.5);
        }

        protected override void PrepareProtoCharacterMob(
            out ProtoCharacterSkeleton skeleton,
            ref double scale,
            DropItemsList lootDroplist)
        {
            skeleton = GetProtoEntity<SkeletonPsiGrove>();

            // primary loot
            lootDroplist
                .Add<ItemSlime>(count: 2,       countRandom: 1)
                .Add<ItemTwigs>(count: 4,       countRandom: 2)
                .Add<ItemSugar>(count: 1,       countRandom: 1)
                .Add<ItemOrePragmium>(count: 1, probability: 0.3);

            // extra loot
            lootDroplist.Add(condition: SkillHunting.ServerRollExtraLoot,
                             nestedList: new DropItemsList(outputs: 1)
                                         .Add<ItemSlime>(count: 1)
                                         .Add<ItemTwigs>(count: 2)
                );
        }
    }
}