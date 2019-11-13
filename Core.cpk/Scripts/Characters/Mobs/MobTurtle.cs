namespace AtomicTorch.CBND.CoreMod.Characters.Mobs
{
    using AtomicTorch.CBND.CoreMod.CharacterSkeletons;
    using AtomicTorch.CBND.CoreMod.Items.Food;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Items.Weapons.MobWeapons;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;

    public class MobTurtle : ProtoCharacterMob
    {
        public override bool AiIsRunAwayFromHeavyVehicles => true;

        public override float CharacterWorldHeight => 0.7f;

        public override double MobKillExperienceMultiplier => 0.1;

        public override string Name => "Turtle";

        public override ObjectMaterial ObjectMaterial => ObjectMaterial.HardTissues;

        public override double ServerUpdateIntervalSeconds => 0.5;

        public override double StatDefaultHealthMax => 75;

        public override double StatMoveSpeed => 0.3;

        protected override void PrepareProtoCharacterMob(
            out ProtoCharacterSkeleton skeleton,
            ref double scale,
            DropItemsList lootDroplist)
        {
            skeleton = GetProtoEntity<SkeletonTurtle>();

            // primary loot
            lootDroplist.Add<ItemAnimalFat>(count: 2, countRandom: 1);

            // random loot
            lootDroplist.Add(probability: 1 / 3.0,
                             nestedList: new DropItemsList(outputs: 1)
                                         .Add<ItemMeatRaw>(count: 1, weight: 1 / 3.0)
                                         .Add<ItemEggsRaw>(count: 1, weight: 1 / 7.0));

            // extra loot
            lootDroplist.Add(probability: 1 / 2.0,
                             condition: SkillHunting.ServerRollExtraLoot,
                             nestedList: new DropItemsList(outputs: 1)
                                         .Add<ItemAnimalFat>(count: 1)
                                         .Add<ItemMeatRaw>(count: 1)
                                         .Add<ItemEggsRaw>(count: 1));
        }

        protected override void ServerInitializeCharacterMob(ServerInitializeData data)
        {
            base.ServerInitializeCharacterMob(data);

            var weaponProto = GetProtoEntity<ItemWeaponGenericAnimalWeak>();
            data.PrivateState.WeaponState.SharedSetWeaponProtoOnly(weaponProto);
            data.PublicState.SharedSetCurrentWeaponProtoOnly(weaponProto);
        }

        protected override void ServerUpdateMob(ServerUpdateData data)
        {
            var character = data.GameObject;

            ServerCharacterAiHelper.ProcessRetreatingAi(
                character,
                distanceRetreat: 5,
                out var movementDirection,
                out var rotationAngleRad);

            this.ServerSetMobInput(character, movementDirection, rotationAngleRad);
        }
    }
}