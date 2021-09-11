namespace AtomicTorch.CBND.CoreMod.Characters.Mobs
{
    using AtomicTorch.CBND.CoreMod.CharacterSkeletons;
    using AtomicTorch.CBND.CoreMod.Items.Weapons.MobWeapons;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Stats;
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;
    using AtomicTorch.CBND.CoreMod.Systems.ServerTimers;
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class MobPragmiumBeetleMinion
        : ProtoCharacterMob
            <MobPragmiumBeetleMinion.PrivateState,
                CharacterMobPublicState,
                CharacterMobClientState>
    {
        public override bool AiIsRunAwayFromHeavyVehicles => false;

        public override float CharacterWorldHeight => 0.8f;

        public override float CharacterWorldWeaponOffsetRanged => 0.2f;

        public override bool IsAvailableInCompletionist => false;

        public override double MobKillExperienceMultiplier => 1.5;

        public override string Name => Api.GetProtoEntity<MobPragmiumBeetle>().Name;

        public override ObjectMaterial ObjectMaterial => ObjectMaterial.Glass;

        public override double StatDefaultHealthMax => 120;

        public override double StatMoveSpeed => 2.9;

        public override void ServerOnDeath(ICharacter character)
        {
            this.ServerSendDeathSoundEvent(character);

            // remove by timer
            ServerTimersSystem.AddAction(
                delaySeconds: 5,
                () => this.ServerSetSpawnState(character, MobSpawnState.Despawning));
        }

        public override bool SharedOnDamage(
            WeaponFinalCache weaponCache,
            IWorldObject targetObject,
            double damagePreMultiplier,
            double damagePostMultiplier,
            out double obstacleBlockDamageCoef,
            out double damageApplied)
        {
            var byCharacter = weaponCache.Character;
            var result = base.SharedOnDamage(weaponCache,
                                             targetObject,
                                             damagePreMultiplier,
                                             damagePostMultiplier,
                                             out obstacleBlockDamageCoef,
                                             out damageApplied);

            if (IsServer
                && result
                && byCharacter is not null
                && !byCharacter.IsNpc)
            {
                // record the damage dealt by player
                var targetCharacter = (ICharacter)targetObject;
                var privateState = GetPrivateState(targetCharacter);
                privateState.DamageTracker?.RegisterDamage(byCharacter, targetCharacter, damageApplied);
            }

            return result;
        }

        protected override void FillDefaultEffects(Effects effects)
        {
            base.FillDefaultEffects(effects);

            effects.AddValue(this, StatName.DefenseImpact, 0.4)
                   .AddValue(this, StatName.DefenseKinetic,   0.4)
                   .AddValue(this, StatName.DefenseExplosion, 0.4)
                   .AddValue(this, StatName.DefenseHeat,      0.3) // lower, to make energy weapons more useful
                   .AddValue(this, StatName.DefenseChemical,  0.25)
                   .AddValue(this, StatName.DefenseCold,      0.2)
                   .AddValue(this, StatName.DefensePsi,       1.0)
                   .AddValue(this, StatName.DefenseRadiation, 1.0);
        }

        protected override void PrepareProtoCharacterMob(
            out ProtoCharacterSkeleton skeleton,
            ref double scale,
            DropItemsList lootDroplist)
        {
            skeleton = GetProtoEntity<SkeletonPragmiumBeetle>();

            // no loot
            lootDroplist.Clear();
        }

        protected override void ServerInitializeCharacterMob(ServerInitializeData data)
        {
            base.ServerInitializeCharacterMob(data);

            var weaponProto = GetProtoEntity<ItemWeaponMobPragmiumBeetleClaws>();
            data.PrivateState.WeaponState.SharedSetWeaponProtoOnly(weaponProto);
            data.PublicState.SharedSetCurrentWeaponProtoOnly(weaponProto);
        }

        protected override void ServerUpdateMob(ServerUpdateData data)
        {
            if (data.PublicState.IsDead)
            {
                return;
            }

            var character = data.GameObject;

            ServerCharacterAiHelper.ProcessAggressiveAi(
                character,
                targetCharacter: ServerCharacterAiHelper.GetClosestTargetPlayer(character),
                isRetreating: false,
                isRetreatingForHeavyVehicles: false,
                distanceRetreat: 0,
                distanceEnemyTooClose: 1,
                distanceEnemyTooFar: 19,
                movementDirection: out var movementDirection,
                rotationAngleRad: out var rotationAngleRad);

            this.ServerSetMobInput(character, movementDirection, rotationAngleRad);
        }

        public class PrivateState : CharacterMobPrivateState, ICharacterPrivateStateWithBossDamageTracker
        {
            [TempOnly]
            public ServerBossDamageTracker DamageTracker { get; set; }
        }
    }
}