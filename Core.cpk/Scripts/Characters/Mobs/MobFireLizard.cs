namespace AtomicTorch.CBND.CoreMod.Characters.Mobs
{
    using AtomicTorch.CBND.CoreMod.CharacterSkeletons;
    using AtomicTorch.CBND.CoreMod.Items.Food;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Items.Weapons.MobWeapons;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Stats;
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;

    public class MobFireLizard : ProtoCharacterMob
    {
        public override bool AiIsRunAwayFromHeavyVehicles => false;

        public override float CharacterWorldHeight => 1.3f;

        public override float CharacterWorldWeaponOffsetRanged => 0.4f;

        public override double MobKillExperienceMultiplier => 2.0;

        public override string Name => "Fire lizard";

        public override ObjectSoundMaterial ObjectSoundMaterial => ObjectSoundMaterial.HardTissues;

        public override double StatDefaultHealthMax => 300;

        public override double StatMoveSpeed => 1.65;

        protected override void FillDefaultEffects(Effects effects)
        {
            base.FillDefaultEffects(effects);

            effects.AddValue(this, StatName.DefenseImpact,  0.4);
            effects.AddValue(this, StatName.DefenseKinetic, 0.2);
            effects.AddValue(this, StatName.DefenseHeat,    0.4);
        }

        protected override void PrepareProtoCharacterMob(
            out ProtoCharacterSkeleton skeleton,
            ref double scale,
            DropItemsList lootDroplist)
        {
            skeleton = GetProtoEntity<SkeletonFireLizard>();

            // primary loot
            lootDroplist
                .Add<ItemMeatRaw>(count: 1)
                .Add<ItemLeather>(count: 3, countRandom: 1)
                .Add<ItemBones>(count: 2,   probability: 1 / 3.0);

            // extra loot
            lootDroplist.Add(condition: SkillHunting.ServerRollExtraLoot,
                             nestedList: new DropItemsList(outputs: 2)
                                         .Add<ItemMeatRaw>(count: 1)
                                         .Add<ItemLeather>(count: 1, countRandom: 1)
                                         .Add<ItemBones>(count: 1,   weight: 1 / 2.0));
        }

        protected override void ServerInitializeCharacterMob(ServerInitializeData data)
        {
            base.ServerInitializeCharacterMob(data);
            var weaponProto = GetProtoEntity<ItemWeaponLizardFangs>();
            data.PrivateState.WeaponState.SharedSetWeaponProtoOnly(weaponProto);
            data.PublicState.SharedSetCurrentWeaponProtoOnly(weaponProto);
        }

        protected override void ServerUpdateMob(ServerUpdateData data)
        {
            var character = data.GameObject;
            var currentStats = data.PublicState.CurrentStats;

            ServerCharacterAiHelper.ProcessAggressiveAi(
                character,
                isRetreating: currentStats.HealthCurrent < currentStats.HealthMax / 4,
                isRetreatingForHeavyVehicles: this.AiIsRunAwayFromHeavyVehicles,
                distanceRetreat: 10,
                distanceEnemyTooClose: 1,
                distanceEnemyTooFar: 8,
                movementDirection: out var movementDirection,
                rotationAngleRad: out var rotationAngleRad);

            this.ServerSetMobInput(character, movementDirection, rotationAngleRad);
        }
    }
}