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

    public class MobBear : ProtoCharacterMob
    {
        public override float CharacterWorldHeight => 1.3f;

        public override double MobKillExperienceMultiplier => 1.0;

        public override string Name => "Bear";

        public override ObjectSoundMaterial ObjectSoundMaterial => ObjectSoundMaterial.SoftTissues;

        public override double StatDefaultHealthMax => 200;

        public override double StatMoveSpeed => 1;

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
            skeleton = GetProtoEntity<SkeletonBear>();

            // primary loot
            lootDroplist
                .Add<ItemMeatRaw>(count: 2)
                .Add<ItemFur>(count: 2,       countRandom: 1)
                .Add<ItemAnimalFat>(count: 2, countRandom: 1)
                .Add<ItemBones>(count: 2,     countRandom: 1);

            // chance for extra loot (2 bonus items)
            lootDroplist.Add(condition: SkillHunting.ServerRollExtraLoot,
                             nestedList: new DropItemsList(outputs: 2)
                                         .Add<ItemMeatRaw>(count: 1)
                                         .Add<ItemFur>(count: 1)
                                         .Add<ItemAnimalFat>(count: 1)
                                         .Add<ItemBones>(count: 1));
        }

        protected override void ServerInitializeCharacterMob(ServerInitializeData data)
        {
            base.ServerInitializeCharacterMob(data);

            var weaponProto = GetProtoEntity<ItemWeaponGenericAnimalStrong>();
            data.PrivateState.WeaponState.SetWeaponProtoOnly(weaponProto);
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
                distanceEnemyTooFar: 6,
                out var movementDirection,
                out var rotationAngleRad);

            this.ServerSetMobInput(character, movementDirection, rotationAngleRad);
        }
    }
}