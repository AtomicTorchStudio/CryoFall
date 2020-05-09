namespace AtomicTorch.CBND.CoreMod.Characters.Mobs
{
    using AtomicTorch.CBND.CoreMod.CharacterSkeletons;
    using AtomicTorch.CBND.CoreMod.Items.Food;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Items.Weapons.MobWeapons;
    using AtomicTorch.CBND.CoreMod.Objects;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Stats;
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;
    using AtomicTorch.CBND.GameApi.Data.World;

    public class MobScorpion : ProtoCharacterMob, IProtoObjectPsiSource
    {
        public override bool AiIsRunAwayFromHeavyVehicles => false;

        public override float CharacterWorldHeight => 1.0f;

        public override double MobKillExperienceMultiplier => 4.0;

        public override string Name => "Scorpion";

        public override ObjectMaterial ObjectMaterial => ObjectMaterial.HardTissues;

        public double PsiIntensity => 0.5;

        public double PsiRadiusMax => 6;

        public double PsiRadiusMin => 3;

        public override double StatDefaultHealthMax => 240; // not very high hp because it has crazy armor

        public override double StatMoveSpeed => 2.25;

        public bool ServerIsPsiSourceActive(IWorldObject worldObject) => true;

        protected override void FillDefaultEffects(Effects effects)
        {
            base.FillDefaultEffects(effects);

            effects.AddValue(this, StatName.DefenseImpact, 0.6)
                   .AddValue(this, StatName.DefenseKinetic,    0.6)
                   .AddValue(this, StatName.DefenseHeat,       0.4)
                   .AddValue(this, StatName.DefenseCold,       0.0)
                   .AddValue(this, StatName.DefenseChemical,   0.8)
                   .AddValue(this, StatName.DefenseElectrical, 0.4)
                   .AddValue(this, StatName.DefensePsi,        1.0);
        }

        protected override void PrepareProtoCharacterMob(
            out ProtoCharacterSkeleton skeleton,
            ref double scale,
            DropItemsList lootDroplist)
        {
            skeleton = GetProtoEntity<SkeletonScorpion>();

            // primary loot
            lootDroplist
                .Add<ItemInsectMeatRaw>(count: 1, countRandom: 1)
                .Add<ItemAnimalFat>(count: 3,     countRandom: 2)
                .Add<ItemToxin>(count: 2,         countRandom: 2)
                .Add<ItemSlime>(count: 2,         countRandom: 2);

            // extra loot
            lootDroplist.Add(condition: SkillHunting.ServerRollExtraLoot,
                             nestedList: new DropItemsList(outputs: 2)
                                         .Add<ItemInsectMeatRaw>(count: 1, countRandom: 1)
                                         .Add<ItemAnimalFat>(count: 1,     countRandom: 1)
                                         .Add<ItemToxin>(count: 1,         countRandom: 1)
                                         .Add<ItemSlime>(count: 1,         countRandom: 1));
        }

        protected override void ServerInitializeCharacterMob(ServerInitializeData data)
        {
            base.ServerInitializeCharacterMob(data);

            var weaponProto = GetProtoEntity<ItemWeaponScorpionClaws>();
            data.PrivateState.WeaponState.SharedSetWeaponProtoOnly(weaponProto);
            data.PublicState.SharedSetCurrentWeaponProtoOnly(weaponProto);
        }

        protected override void ServerUpdateMob(ServerUpdateData data)
        {
            var character = data.GameObject;

            ServerCharacterAiHelper.ProcessAggressiveAi(
                character,
                isRetreating: false,
                isRetreatingForHeavyVehicles: this.AiIsRunAwayFromHeavyVehicles,
                distanceRetreat: 0,
                distanceEnemyTooClose: 1,
                distanceEnemyTooFar: 10,
                movementDirection: out var movementDirection,
                rotationAngleRad: out var rotationAngleRad);

            this.ServerSetMobInput(character, movementDirection, rotationAngleRad);
        }
    }
}