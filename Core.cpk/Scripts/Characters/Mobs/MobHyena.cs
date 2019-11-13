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

    public class MobHyena : ProtoCharacterMob
    {
        public override bool AiIsRunAwayFromHeavyVehicles => true;

        public override float CharacterWorldHeight => 1.1f;

        public override double MobKillExperienceMultiplier => 1.0;

        public override string Name => "Hyena";

        public override ObjectMaterial ObjectMaterial => ObjectMaterial.SoftTissues;

        public override double StatDefaultHealthMax => 120;

        public override double StatMoveSpeed => 2.1;

        protected override void FillDefaultEffects(Effects effects)
        {
            base.FillDefaultEffects(effects);

            effects.AddValue(this, StatName.DefenseImpact, 0.4);
        }

        protected override void PrepareProtoCharacterMob(
            out ProtoCharacterSkeleton skeleton,
            ref double scale,
            DropItemsList lootDroplist)
        {
            skeleton = GetProtoEntity<SkeletonHyena>();

            // primary loot
            lootDroplist
                .Add<ItemMeatRaw>(count: 1)
                .Add<ItemFur>(count: 1)
                .Add<ItemBones>(count: 1, countRandom: 1);

            // random loot
            lootDroplist
                .Add<ItemAnimalFat>(count: 1, probability: 1 / 3.0);

            // extra loot
            lootDroplist.Add(condition: SkillHunting.ServerRollExtraLoot,
                             nestedList: new DropItemsList(outputs: 1)
                                         .Add<ItemMeatRaw>(count: 1)
                                         .Add<ItemFur>(count: 1)
                                         .Add<ItemBones>(count: 1)
                                         .Add<ItemAnimalFat>(count: 1));
        }

        protected override void ServerInitializeCharacterMob(ServerInitializeData data)
        {
            base.ServerInitializeCharacterMob(data);

            var weaponProto = GetProtoEntity<ItemWeaponGenericAnimalMedium>();
            data.PrivateState.WeaponState.SharedSetWeaponProtoOnly(weaponProto);
            data.PublicState.SharedSetCurrentWeaponProtoOnly(weaponProto);
        }

        protected override void ServerUpdateMob(ServerUpdateData data)
        {
            var character = data.GameObject;
            var currentStats = data.PublicState.CurrentStats;

            ServerCharacterAiHelper.ProcessAggressiveAi(
                character,
                isRetreating: currentStats.HealthCurrent < currentStats.HealthMax / 3,
                isRetreatingForHeavyVehicles: this.AiIsRunAwayFromHeavyVehicles,
                distanceRetreat: 8,
                distanceEnemyTooClose: 1,
                distanceEnemyTooFar: 8,
                movementDirection: out var movementDirection,
                rotationAngleRad: out var rotationAngleRad);

            this.ServerSetMobInput(character, movementDirection, rotationAngleRad);
        }
    }
}