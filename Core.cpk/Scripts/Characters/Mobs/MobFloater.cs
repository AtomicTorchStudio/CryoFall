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

    public class MobFloater : ProtoCharacterMob
    {
        public override bool AiIsRunAwayFromHeavyVehicles => false;

        public override double BiomaterialValueMultiplier => 2.0;

        public override float CharacterWorldHeight => 2.4f;

        public override float CharacterWorldWeaponOffsetRanged => 1.1f;

        public override double MobKillExperienceMultiplier => 5.0;

        public override string Name => "Floater";

        public override ObjectMaterial ObjectMaterial => ObjectMaterial.SoftTissues;

        public override double PhysicsBodyAccelerationCoef => 1;

        public override double PhysicsBodyFriction => 0.5;

        public override double StatDefaultHealthMax => 300;

        public override double StatMoveSpeed => 2.5;

        protected override void FillDefaultEffects(Effects effects)
        {
            base.FillDefaultEffects(effects);

            // easy to kill by normal means, but have very tough protection against certain types of damage
            effects.AddValue(this, StatName.DefenseImpact, 1.0)
                   .AddValue(this, StatName.DefenseKinetic,   0.2)
                   .AddValue(this, StatName.DefenseHeat,      0.1)
                   .AddValue(this, StatName.DefenseChemical,  1.0)
                   .AddValue(this, StatName.DefensePsi,       1.0)
                   .AddValue(this, StatName.DefenseRadiation, 1.0);
        }

        protected override void PrepareProtoCharacterMob(
            out ProtoCharacterSkeleton skeleton,
            ref double scale,
            DropItemsList lootDroplist)
        {
            skeleton = GetProtoEntity<SkeletonFloater>();

            // primary loot
            lootDroplist
                .Add<ItemInsectMeatRaw>(count: 1, countRandom: 1)
                .Add<ItemSlime>(count: 10,        countRandom: 10)
                .Add<ItemToxin>(count: 5,         countRandom: 5)
                .Add<ItemSalt>(count: 5,          countRandom: 5)
                .Add<ItemRubberRaw>(count: 8,     countRandom: 3);

            // extra loot
            lootDroplist.Add(condition: SkillHunting.ServerRollExtraLoot,
                             nestedList: new DropItemsList(outputs: 2)
                                         .Add<ItemInsectMeatRaw>(count: 1)
                                         .Add<ItemSlime>(count: 5)
                                         .Add<ItemToxin>(count: 2)
                                         .Add<ItemSalt>(count: 2));
        }

        protected override void ServerInitializeCharacterMob(ServerInitializeData data)
        {
            base.ServerInitializeCharacterMob(data);

            var weaponProto = GetProtoEntity<ItemWeaponMobFloaterNova>();
            data.PrivateState.WeaponState.SharedSetWeaponProtoOnly(weaponProto);
            data.PublicState.SharedSetCurrentWeaponProtoOnly(weaponProto);
        }

        protected override void ServerUpdateMob(ServerUpdateData data)
        {
            var character = data.GameObject;

            ServerCharacterAiHelper.ProcessAggressiveAi(
                character,
                targetCharacter: ServerCharacterAiHelper.GetClosestTargetPlayer(character),
                isRetreating: false,
                isRetreatingForHeavyVehicles: this.AiIsRunAwayFromHeavyVehicles,
                distanceRetreat: 0,
                distanceEnemyTooClose: 1,
                distanceEnemyTooFar: 15,
                movementDirection: out var movementDirection,
                rotationAngleRad: out var rotationAngleRad);

            this.ServerSetMobInput(character, movementDirection, rotationAngleRad);
        }
    }
}