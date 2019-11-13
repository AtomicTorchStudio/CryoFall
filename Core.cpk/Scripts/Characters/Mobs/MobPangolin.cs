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

    public class MobPangolin : ProtoCharacterMob
    {
        public override bool AiIsRunAwayFromHeavyVehicles => true;

        public override float CharacterWorldHeight => 0.75f;

        public override double MobKillExperienceMultiplier => 0.1;

        public override string Name => "Pangolin";

        public override ObjectMaterial ObjectMaterial => ObjectMaterial.HardTissues;

        public override double StatDefaultHealthMax => 70;

        public override double StatMoveSpeed => 1.5;

        protected override void FillDefaultEffects(Effects effects)
        {
            base.FillDefaultEffects(effects);

            effects.AddValue(this, StatName.DefenseImpact, 0.6);
        }

        protected override void PrepareProtoCharacterMob(
            out ProtoCharacterSkeleton skeleton,
            ref double scale,
            DropItemsList lootDroplist)
        {
            skeleton = GetProtoEntity<SkeletonPangolin>();

            // primary loot
            lootDroplist
                .Add<ItemAnimalFat>(count: 1)
                .Add<ItemBones>(count: 1, countRandom: 1);

            // random loot
            lootDroplist
                .Add<ItemMeatRaw>(count: 1, probability: 1 / 3.0);

            // extra loot
            lootDroplist.Add(condition: SkillHunting.ServerRollExtraLoot,
                             nestedList: new DropItemsList(outputs: 1)
                                         .Add<ItemAnimalFat>(count: 1)
                                         .Add<ItemBones>(count: 1));
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

            ServerCharacterAiHelper.ProcessRetreatingAi(
                character,
                distanceRetreat: 7,
                out var movementDirection,
                out var rotationAngleRad);

            this.ServerSetMobInput(character, movementDirection, rotationAngleRad);
        }
    }
}