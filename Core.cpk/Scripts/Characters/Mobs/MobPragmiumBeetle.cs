namespace AtomicTorch.CBND.CoreMod.Characters.Mobs
{
    using AtomicTorch.CBND.CoreMod.CharacterSkeletons;
    using AtomicTorch.CBND.CoreMod.Items.Food;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Items.Weapons.MobWeapons;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Stats;
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;

    public class MobPragmiumBeetle : ProtoCharacterMob
    {
        public override float CharacterWorldHeight => 0.8f;

        public override float CharacterWorldWeaponOffsetRanged => 0.2f;

        public override double MobKillExperienceMultiplier => 1.5;

        public override string Name => "Pragmium beetle";

        public override ObjectSoundMaterial ObjectSoundMaterial => ObjectSoundMaterial.Glass;

        public override double StatDefaultHealthMax => 120;

        public override double StatMoveSpeed => 1.65;

        protected override void FillDefaultEffects(Effects effects)
        {
            base.FillDefaultEffects(effects);

            effects.AddValue(this, StatName.DefenseImpact,     0.4);
            effects.AddValue(this, StatName.DefenseKinetic,    0.4);
            effects.AddValue(this, StatName.DefenseHeat,       0.6);
            effects.AddValue(this, StatName.DefenseChemical,   1.0);
            effects.AddValue(this, StatName.DefenseCold,       0.2);
            effects.AddValue(this, StatName.DefenseElectrical, 0.4);
            effects.AddValue(this, StatName.DefensePsi,        1.0);
            effects.AddValue(this, StatName.DefenseRadiation,  1.0);
        }

        protected override void PrepareProtoCharacterMob(
            out ProtoCharacterSkeleton skeleton,
            ref double scale,
            DropItemsList lootDroplist)
        {
            skeleton = GetProtoEntity<SkeletonPragmiumBeetle>();

            // primary loot
            lootDroplist
                .Add<ItemInsectMeatRaw>(count: 1, countRandom: 1)
                .Add<ItemOrePragmium>(count: 1);
        }

        protected override void ServerInitializeCharacterMob(ServerInitializeData data)
        {
            base.ServerInitializeCharacterMob(data);

            var weaponProto = GetProtoEntity<ItemWeaponGenericAnimalStrong>();
            data.PrivateState.WeaponState.SharedSetWeaponProtoOnly(weaponProto);
            data.PublicState.SetCurrentWeaponProtoOnly(weaponProto);
        }

        protected override void ServerUpdateMob(ServerUpdateData data)
        {
            var character = data.GameObject;

            ServerCharacterAiHelper.ProcessAggressiveAi(
                character,
                isRetreating: false,
                distanceRetreat: 7,
                distanceEnemyTooClose: 1,
                distanceEnemyTooFar: 6,
                out var movementDirection,
                out var rotationAngleRad);

            this.ServerSetMobInput(character, movementDirection, rotationAngleRad);
        }
    }
}