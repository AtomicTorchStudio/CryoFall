namespace AtomicTorch.CBND.CoreMod.Characters.Mobs
{
    using AtomicTorch.CBND.CoreMod.CharacterSkeletons;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Items.Weapons.MobWeapons;
    using AtomicTorch.CBND.CoreMod.Objects;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Stats;
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;

    public class MobScorpion : ProtoCharacterMob, IProtoObjectPsiSource
    {
        public override float CharacterWorldHeight => 1.25f;

        public override double MobKillExperienceMultiplier => 2.0;

        public override string Name => "Scorpion";

        public override ObjectSoundMaterial ObjectSoundMaterial => ObjectSoundMaterial.HardTissues;

        public double PsiIntensity => 0.5;

        public double PsiRadiusMax => 6;

        public double PsiRadiusMin => 3;

        public override double StatDefaultHealthMax => 200; // not very high hp because it has crazy armor

        public override double StatMoveSpeed => 1.5;

        protected override void FillDefaultEffects(Effects effects)
        {
            base.FillDefaultEffects(effects);

            effects.AddValue(this, StatName.DefenseImpact,     0.6);
            effects.AddValue(this, StatName.DefenseKinetic,    0.6);
            effects.AddValue(this, StatName.DefenseHeat,       0.4);
            effects.AddValue(this, StatName.DefenseCold,       0.4);
            effects.AddValue(this, StatName.DefenseChemical,   0.8);
            effects.AddValue(this, StatName.DefenseElectrical, 0.4);
        }

        protected override void PrepareProtoCharacterMob(
            out ProtoCharacterSkeleton skeleton,
            ref double scale,
            DropItemsList lootDroplist)
        {
            skeleton = GetProtoEntity<SkeletonScorpion>();

            // primary loot
            lootDroplist
                .Add<ItemBones>(count: 4,     countRandom: 2)
                .Add<ItemAnimalFat>(count: 4, countRandom: 2)
                .Add<ItemToxin>(count: 2,     countRandom: 2);

            // extra loot
            lootDroplist.Add(condition: SkillHunting.ServerRollExtraLoot,
                             nestedList: new DropItemsList(outputs: 2)
                                         .Add<ItemBones>(count: 1,     countRandom: 1)
                                         .Add<ItemAnimalFat>(count: 1, countRandom: 1)
                                         .Add<ItemToxin>(count: 1,     countRandom: 1));
        }

        protected override void ServerInitializeCharacterMob(ServerInitializeData data)
        {
            base.ServerInitializeCharacterMob(data);

            var weaponProto = GetProtoEntity<ItemWeaponScorpionClaws>();
            data.PrivateState.WeaponState.SharedSetWeaponProtoOnly(weaponProto);
            data.PublicState.SetCurrentWeaponProtoOnly(weaponProto);
        }

        protected override void ServerUpdateMob(ServerUpdateData data)
        {
            var character = data.GameObject;

            ServerCharacterAiHelper.ProcessAggressiveAi(
                character,
                isRetreating: false,
                distanceRetreat: 0,
                distanceEnemyTooClose: 1,
                distanceEnemyTooFar: 10,
                out var movementDirection,
                out var rotationAngleRad);

            this.ServerSetMobInput(character, movementDirection, rotationAngleRad);
        }
    }
}