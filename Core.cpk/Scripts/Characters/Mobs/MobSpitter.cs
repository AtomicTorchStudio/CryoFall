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

    public class MobSpitter : ProtoCharacterMob
    {
        public override bool AiIsRunAwayFromHeavyVehicles => false;

        public override float CharacterWorldHeight => 2.0f;

        public override double CorpseInteractionAreaScale => 1.1;

        public override double MobKillExperienceMultiplier => 2.0;

        public override string Name => "Spitter";

        public override ObjectMaterial ObjectMaterial => ObjectMaterial.HardTissues;

        public override double StatDefaultHealthMax => 120;

        public override double StatMoveSpeed => 0;

        protected override void FillDefaultEffects(Effects effects)
        {
            base.FillDefaultEffects(effects);

            effects
                .AddValue(this, StatName.DefenseKinetic,  0.3)
                .AddValue(this, StatName.DefenseHeat,     0.3)
                .AddValue(this, StatName.DefenseChemical, 1.0);
        }

        protected override void PrepareProtoCharacterMob(
            out ProtoCharacterSkeleton skeleton,
            ref double scale,
            DropItemsList lootDroplist)
        {
            skeleton = GetProtoEntity<SkeletonSpitter>();

            // primary loot
            lootDroplist
                .Add<ItemInsectMeatRaw>(count: 2, countRandom: 1)
                .Add<ItemToxin>(count: 1,         countRandom: 1)
                .Add<ItemSlime>(count: 2,         countRandom: 1);

            // extra loot
            lootDroplist.Add(condition: SkillHunting.ServerRollExtraLoot,
                             nestedList: new DropItemsList(outputs: 1)
                                         .Add<ItemInsectMeatRaw>(count: 1)
                                         .Add<ItemSlime>(count: 1));
        }

        protected override void ServerInitializeCharacterMob(ServerInitializeData data)
        {
            base.ServerInitializeCharacterMob(data);

            var weaponProto = GetProtoEntity<ItemWeaponMobSpitterPoison>();
            data.PrivateState.WeaponState.SharedSetWeaponProtoOnly(weaponProto);
            data.PublicState.SharedSetCurrentWeaponProtoOnly(weaponProto);
        }

        protected override void ServerUpdateMob(ServerUpdateData data)
        {
            var character = data.GameObject;

            ServerCharacterAiHelper.ProcessAggressiveAi(
                character,
                targetCharacter: ServerRangedAiHelper.GetClosestTargetPlayer(character, data.PrivateState),
                isRetreating: false,
                isRetreatingForHeavyVehicles: false,
                distanceRetreat: 0,
                distanceEnemyTooClose: 1,
                distanceEnemyTooFar: 8,
                movementDirection: out var movementDirection,
                rotationAngleRad: out var rotationAngleRad,
                attackFarOnlyIfAgro: true);

            // it's a static mob
            movementDirection = default;
            this.ServerSetMobInput(character, movementDirection, rotationAngleRad);
        }
    }
}