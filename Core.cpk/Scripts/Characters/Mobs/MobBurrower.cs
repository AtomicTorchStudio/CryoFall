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

    public class MobBurrower : ProtoCharacterMob
    {
        public override bool AiIsRunAwayFromHeavyVehicles => true;

        public override float CharacterWorldHeight => 1f;

        public override double MobKillExperienceMultiplier => 1.5;

        public override string Name => "Burrower";

        public override ObjectMaterial ObjectMaterial => ObjectMaterial.SoftTissues;

        public override double StatDefaultHealthMax => 150;

        public override double StatMoveSpeed => 1.8;

        protected override void FillDefaultEffects(Effects effects)
        {
            base.FillDefaultEffects(effects);

            effects.AddValue(this, StatName.DefenseImpact,     0.4);
            effects.AddValue(this, StatName.DefenseKinetic,    0.4);
            effects.AddValue(this, StatName.DefenseChemical,   0.4);
            effects.AddValue(this, StatName.DefenseCold,       0.2);
            effects.AddValue(this, StatName.DefenseElectrical, 0.2);
        }

        protected override void PrepareProtoCharacterMob(
            out ProtoCharacterSkeleton skeleton,
            ref double scale,
            DropItemsList lootDroplist)
        {
            skeleton = GetProtoEntity<SkeletonBurrower>();

            // primary loot
            lootDroplist
                .Add<ItemMeatRaw>(count: 1)
                .Add<ItemSlime>(count: 1, countRandom: 1)
                .Add<ItemBones>(count: 2, countRandom: 1);

            // extra loot
            lootDroplist.Add(condition: SkillHunting.ServerRollExtraLoot,
                             nestedList: new DropItemsList(outputs: 1)
                                         .Add<ItemMeatRaw>(count: 1)
                                         .Add<ItemSlime>(count: 1)
                                         .Add<ItemBones>(count: 1));
        }

        protected override void ServerInitializeCharacterMob(ServerInitializeData data)
        {
            base.ServerInitializeCharacterMob(data);

            var weaponProto = GetProtoEntity<ItemWeaponGenericAnimalStrong>();
            data.PrivateState.WeaponState.SharedSetWeaponProtoOnly(weaponProto);
            data.PublicState.SharedSetCurrentWeaponProtoOnly(weaponProto);
        }

        protected override void ServerUpdateMob(ServerUpdateData data)
        {
            var character = data.GameObject;
            var currentStats = data.PublicState.CurrentStats;

            ServerCharacterAiHelper.ProcessAggressiveAi(
                character,
                isRetreating: false,
                isRetreatingForHeavyVehicles: this.AiIsRunAwayFromHeavyVehicles,
                distanceRetreat: 7,
                distanceEnemyTooClose: 1,
                distanceEnemyTooFar: 6,
                movementDirection: out var movementDirection,
                rotationAngleRad: out var rotationAngleRad);

            this.ServerSetMobInput(character, movementDirection, rotationAngleRad);
        }
    }
}