namespace AtomicTorch.CBND.CoreMod.Characters.Mobs
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.CharacterSkeletons;
    using AtomicTorch.CBND.CoreMod.Items.Ammo;
    using AtomicTorch.CBND.CoreMod.Items.Weapons.MobWeapons;
    using AtomicTorch.CBND.CoreMod.Rates;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Misc.Events;
    using AtomicTorch.CBND.CoreMod.Stats;
    using AtomicTorch.CBND.CoreMod.Systems.BossLootSystem;
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;
    using AtomicTorch.CBND.CoreMod.Systems.NewbieProtection;
    using AtomicTorch.CBND.CoreMod.Systems.ServerTimers;
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Helpers;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class MobBossPragmiumQueen
        : ProtoCharacterMob
          <MobBossPragmiumQueen.PrivateState,
              CharacterMobPublicState,
              CharacterMobClientState>,
          IProtoCharacterBoss
    {
        // The boss is regenerating fast only if it didn't receive a significant damage for a while.
        private const double BossHealthRegenerationPerSecondFast = 100;

        private const double BossHealthRegenerationPerSecondSlow = 40;

        private const int DeathSpawnLootObjectsDefaultCount = 16;

        private const double DeathSpawnLootObjectsRadius = 19;

        private const int DeathSpawnMinionsDefaultCount = 8;

        private const double DeathSpawnMinionsRadius = 17;

        private const double MinionSpawnNoObstaclesCircleRadius = 0.25;

        // The boss can move within the area in configured radius only.
        private const double MovementMaxRadius = 9;

        // Delay since the last damage before the HP regeneration will start.
        private const double RegenerationDelaySeconds = 6;

        private const double SpawnMinionsCheckDistance = 21;

        private const int SpawnMinionsPerPlayer = 1;

        private const int SpawnMinionsTotalNumberMax = 10;

        private const int SpawnMinionsTotalNumberMin = 3;

        private const double VictoryLearningPointsBonusPerLootObject = 15; // give bonus LP per loot pile

        // Determines how often the boss will attempt to use the nova attack (a time interval in seconds).
        private static readonly Interval<double> NovaAttackInterval
            = new(min: 14, max: 17);

        private static readonly Lazy<IProtoStaticWorldObject> ProtoLootObjectLazy
            = new(GetProtoEntity<ObjectPragmiumQueenRemains>);

        // Determines the type of the minion creatures to spawn when boss is dead.
        private static readonly Lazy<IProtoCharacterMob> ProtoMinionObjectDeathLazy
            = new(GetProtoEntity<MobPragmiumBeetleMinion>);

        // Determines the type of the minion creatures to spawn when boss is using the nova attack.
        private static readonly Lazy<IProtoCharacterMob> ProtoMinionObjectLazy
            = new(GetProtoEntity<MobPragmiumBeetleMinion>);

        private static readonly double ServerBossDifficultyCoef;

        private static readonly int ServerMaxLootWinners;

        private IReadOnlyList<AiWeaponPreset> weaponsListNovaAttack;

        private IReadOnlyList<AiWeaponPreset> weaponsListPrimary;

        static MobBossPragmiumQueen()
        {
            if (Api.IsClient)
            {
                return;
            }

            var requiredPlayersNumber = RateBossDifficultyPragmiumQueen.SharedValue;

            // coef range from 0.2 to 2.0
            ServerBossDifficultyCoef = requiredPlayersNumber / 5.0;

            ServerMaxLootWinners = (int)Math.Ceiling(requiredPlayersNumber * 2);
            ServerMaxLootWinners = Math.Max(ServerMaxLootWinners, 5); // ensure at least 5 winners
        }

        public override bool AiIsRunAwayFromHeavyVehicles => false;

        public override float CharacterWorldHeight => 3.15f;

        public override bool HasIncreasedScopeSize => true;

        // it's a boss and currently we don't have a way to account and add the kill to all participating players
        public override bool IsAvailableInCompletionist => false;

        public override bool IsBoss => true;

        // no experience for killing it, as it would just go to the player who dealt the final hit
        // instead, there is a different mechanic which provides XP to every participating player
        public override double MobKillExperienceMultiplier => 0;

        public override string Name => "Pragmium Queen";

        public override ObjectMaterial ObjectMaterial => ObjectMaterial.HardTissues;

        public override double StatDefaultHealthMax => 8500;

        // Please note: boss using different regeneration mechanism.
        public override double StatHealthRegenerationPerSecond => 0;

        public override double StatMoveSpeed => 2.25;

        protected override byte SoundEventsNetworkRadius => 40;

        public override void ServerOnDeath(ICharacter character)
        {
            this.ServerSendDeathSoundEvent(character);

            ServerTimersSystem.AddAction(
                delaySeconds: 3,
                () =>
                {
                    // explode
                    var bossPosition = character.Position;
                    var protoExplosion = Api.GetProtoEntity<ObjectPragmiumQueenDeathExplosion>();
                    Server.World.CreateStaticWorldObject(protoExplosion,
                                                         (bossPosition - protoExplosion.Layout.Center)
                                                         .ToVector2Ushort());

                    var privateState = GetPrivateState(character);
                    var damageTracker = privateState.DamageTracker;

                    // spawn loot and minions on death
                    ServerTimersSystem.AddAction(
                        delaySeconds: protoExplosion.ExplosionDelay.TotalSeconds
                                      + protoExplosion.ExplosionPreset.ServerDamageApplyDelay * 1.01,
                        () =>
                        {
                            try
                            {
                                ServerBossLootSystem.ServerCreateBossLoot(
                                    bossPosition: bossPosition.ToVector2Ushort(),
                                    protoCharacterBoss: this,
                                    damageTracker: damageTracker,
                                    bossDifficultyCoef: ServerBossDifficultyCoef,
                                    lootObjectProto: ProtoLootObjectLazy.Value,
                                    lootObjectsDefaultCount: DeathSpawnLootObjectsDefaultCount,
                                    lootObjectsRadius: DeathSpawnLootObjectsRadius,
                                    learningPointsBonusPerLootObject: VictoryLearningPointsBonusPerLootObject,
                                    maxLootWinners: ServerMaxLootWinners);
                            }
                            finally
                            {
                                ServerBossLootSystem.ServerSpawnBossMinionsOnDeath(
                                    epicenterPosition: bossPosition.ToVector2Ushort(),
                                    bossDifficultyCoef: ServerBossDifficultyCoef,
                                    minionProto: ProtoMinionObjectDeathLazy.Value,
                                    minionsDefaultCount: DeathSpawnMinionsDefaultCount,
                                    minionsRadius: DeathSpawnMinionsRadius);
                            }
                        });

                    // destroy the character object after the explosion
                    ServerTimersSystem.AddAction(
                        delaySeconds: protoExplosion.ExplosionDelay.TotalSeconds + 0.5,
                        () => Server.World.DestroyObject(character));
                });
        }

        public void ServerTrySpawnMinions(ICharacter characterBoss)
        {
            var privateState = GetPrivateState(characterBoss);
            ServerBossMinionHelper.ServerSpawnMinions(
                characterBoss,
                characterBossCenterPosition: characterBoss.Position + (0, 1.0),
                protoMinion: ProtoMinionObjectLazy.Value,
                minionsList: privateState.SpawnedMinionsList,
                spawnCheckDistanceSqr: SpawnMinionsCheckDistance * SpawnMinionsCheckDistance,
                bossDamageTracker: privateState.DamageTracker,
                minionsPerPlayer: SpawnMinionsPerPlayer,
                minionsTotalMin: SpawnMinionsTotalNumberMin,
                minionsTotalMax: SpawnMinionsTotalNumberMax,
                minionsSpawnPerIterationLimit: null,
                baseMinionsNumber: 1.0,
                spawnNoObstaclesCircleRadius: MinionSpawnNoObstaclesCircleRadius,
                spawnDistanceMin: 1.0,
                spawnDistanceMax: 1.5);
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
            if (NewbieProtectionSystem.SharedIsNewbie(byCharacter))
            {
                // don't allow attacking a boss while under newbie protection
                if (IsClient)
                {
                    NewbieProtectionSystem.ClientNotifyNewbieCannotPerformAction(this);
                }

                obstacleBlockDamageCoef = 0;
                damageApplied = 0;
                return false;
            }

            if (IsServer)
            {
                // apply the difficulty coefficient
                damagePostMultiplier /= ServerBossDifficultyCoef;

                if (weaponCache.ProtoExplosive is not null)
                {
                    // the boss is massive so it will take increased damage from explosives/grenades
                    if (weaponCache.ProtoExplosive is IAmmoCannonShell)
                    {
                        damagePostMultiplier *= 1.333; // the artillery shells are too OP already
                    }
                    else
                    {
                        damagePostMultiplier *= 2;
                    }
                }
            }

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
                // calculate the original damage (without the applied difficulty coefficient)
                var originalDamageApplied = damageApplied * ServerBossDifficultyCoef;
                privateState.DamageTracker.RegisterDamage(byCharacter,
                                                          targetCharacter,
                                                          originalDamageApplied);
                // record the last time the damage is dealt
                privateState.LastDamageTime = Server.Game.FrameTime;
            }

            return result;
        }

        protected override void FillDefaultEffects(Effects effects)
        {
            base.FillDefaultEffects(effects);

            effects.AddValue(this, StatName.DefenseImpact, 0.6)
                   .AddValue(this, StatName.DefenseKinetic,   0.95)
                   .AddValue(this, StatName.DefenseExplosion, 0.5)
                   .AddValue(this, StatName.DefenseHeat,      0.75)
                   .AddValue(this, StatName.DefenseCold,      0.7)
                   .AddValue(this, StatName.DefenseChemical,  0.7)
                   .AddValue(this, StatName.DefensePsi,       1.0)
                   .AddPercent(this, StatName.DazedIncreaseRateMultiplier, -100);
        }

        protected override void PrepareProtoCharacterMob(
            out ProtoCharacterSkeleton skeleton,
            ref double scale,
            DropItemsList lootDroplist)
        {
            skeleton = GetProtoEntity<SkeletonPragmiumQueen>();
        }

        protected override void ServerInitializeCharacterMob(ServerInitializeData data)
        {
            base.ServerInitializeCharacterMob(data);

            this.ServerSetSpawnState(data.GameObject, MobSpawnState.Spawning);

            if (data.IsFirstTimeInit)
            {
                data.PrivateState.HoldPosition = data.GameObject.TilePosition;
            }

            data.PrivateState.DamageTracker = new ServerBossDamageTracker();

            this.weaponsListPrimary = new AiWeaponPresetList()
                                      .Add(new AiWeaponPreset(GetProtoEntity<ItemWeaponMobPragmiumQueenMelee>()))
                                      .Add(new AiWeaponPreset(GetProtoEntity<ItemWeaponMobPragmiumQueenRanged>()))
                                      .ToReadReadOnly();

            this.weaponsListNovaAttack = new AiWeaponPresetList()
                                         .Add(new AiWeaponPreset(GetProtoEntity<ItemWeaponMobPragmiumQueenNova>()))
                                         .ToReadReadOnly();

            ServerMobWeaponHelper.TrySetWeapon(data.GameObject,
                                               this.weaponsListPrimary[0].ProtoWeapon,
                                               rebuildWeaponsCacheNow: false);
        }

        protected override void ServerUpdateMob(ServerUpdateData data)
        {
            var characterBoss = data.GameObject;
            var publicState = data.PublicState;

            if (publicState.IsDead)
            {
                return;
            }

            var privateState = data.PrivateState;
            var lastTargetCharacter = privateState.CurrentTargetCharacter;
            var deltaTime = data.DeltaTime;

            data.PrivateState.DamageTracker.Update(deltaTime);

            // Regenerate the health points fast on every frame
            // if there was no damage dealt to boss recently.
            // Please note: the difficulty coefficient doesn't apply there
            // as the boss HP doesn't change with difficulty - only damage
            // to it is modified by the difficulty coefficient.
            var isRegeneratingFast = Server.Game.FrameTime
                                     >= privateState.LastDamageTime + RegenerationDelaySeconds;

            var regenerationPerSecond
                = isRegeneratingFast
                      ? BossHealthRegenerationPerSecondFast
                      : BossHealthRegenerationPerSecondSlow;

            publicState.CurrentStats.ServerSetHealthCurrent(
                (float)(publicState.CurrentStats.HealthCurrent
                        + regenerationPerSecond * deltaTime));

            var weaponList = this.ServerSelectWeaponsList(privateState,
                                                          deltaTime,
                                                          out var isSwitchingToNovaAttack);

            ServerCharacterAiHelper.ProcessBossAi(
                characterBoss,
                weaponList,
                distanceEnemyTooClose: 7.5,
                distanceEnemyTooFar: 15.5,
                movementDirection: out var movementDirection,
                rotationAngleRad: out var rotationAngleRad);

            if (movementDirection != default
                && !ServerCanMoveInDirection(characterBoss.TilePosition.ToVector2D(),
                                             movementDirection,
                                             privateState.HoldPosition.ToVector2D()))
            {
                // cannot move in desired direction - too far from the position to hold
                movementDirection = default;
            }

            if (lastTargetCharacter is null
                && privateState.CurrentTargetCharacter is not null
                // is the last attack happened not too recently?
                && privateState.TimeToNextNovaAttack < NovaAttackInterval.Max - 8)
            {
                //Logger.Dev("Boss acquired target! Will use a nova attack in the next 2-4 seconds!");
                privateState.TimeToNextNovaAttack = RandomHelper.Next(2, 4);
            }

            if (isSwitchingToNovaAttack)
            {
                movementDirection = default;
                privateState.WeaponState.SharedSetInputIsFiring(false);
            }
            else if (privateState.WeaponState.IsFiring
                     && privateState.WeaponState.ProtoWeapon is ItemWeaponMobPragmiumQueenNova)
            {
                movementDirection = default;
            }

            this.ServerSetMobInput(characterBoss, movementDirection, rotationAngleRad);

            if (!privateState.WeaponState.IsFiring)
            {
                // detect and despawn minions that have moved too far away
                ServerBossMinionHelper.ServerProcessMinions(
                    characterBoss,
                    ProtoMinionObjectLazy.Value,
                    privateState.SpawnedMinionsList,
                    spawnedMinionsCount: out _,
                    despawnDistanceSqr: SpawnMinionsCheckDistance * SpawnMinionsCheckDistance);
            }
        }

        private static bool ServerCanMoveInDirection(
            in Vector2D currentPosition,
            in Vector2F movementDirection,
            in Vector2D holdPosition)
        {
            var deltaPos = currentPosition + movementDirection.Normalized - holdPosition;
            return deltaPos.LengthSquared <= MovementMaxRadius * MovementMaxRadius;
        }

        private IReadOnlyList<AiWeaponPreset> ServerSelectWeaponsList(
            PrivateState privateState,
            double deltaTime,
            out bool isSwitchingToNovaAttack)
        {
            var weaponList = this.weaponsListPrimary;
            isSwitchingToNovaAttack = false;

            privateState.TimeToNextNovaAttack -= deltaTime;
            if (privateState.TimeToNextNovaAttack > 0)
            {
                return weaponList;
            }

            if (privateState.CurrentTargetCharacter is null
                || privateState.WeaponState.CooldownSecondsRemains > 0
                || privateState.WeaponState.DamageApplyDelaySecondsRemains > 0)
            {
                // cannot switch to nova attack right now as the previous attack is ongoing
                // stop the previous attack and wait until it's finished
                isSwitchingToNovaAttack = true;
                privateState.TimeToNextNovaAttack = 0;
            }
            else
            {
                // time to start a nova attack
                privateState.TimeToNextNovaAttack = NovaAttackInterval.Min;
                privateState.TimeToNextNovaAttack += RandomHelper.NextDouble()
                                                     * (NovaAttackInterval.Max - NovaAttackInterval.Min);

                weaponList = this.weaponsListNovaAttack;
                //Logger.Dev("Time to start a nova attack!");
            }

            return weaponList;
        }

        public class PrivateState : CharacterMobPrivateState
        {
            [TempOnly]
            public ServerBossDamageTracker DamageTracker { get; set; }

            public Vector2Ushort HoldPosition { get; set; }

            [TempOnly]
            public double LastDamageTime { get; set; }

            [TempOnly]
            public List<ICharacter> SpawnedMinionsList { get; } = new();

            public double TimeToNextNovaAttack { get; set; }
        }
    }
}