namespace AtomicTorch.CBND.CoreMod.Characters.Mobs
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.CharacterSkeletons;
    using AtomicTorch.CBND.CoreMod.Items.Ammo;
    using AtomicTorch.CBND.CoreMod.Items.Weapons.MobWeapons;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Misc.Events;
    using AtomicTorch.CBND.CoreMod.Stats;
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

    public class MobPragmiumQueen
        : ProtoCharacterMob
            <MobPragmiumQueen.PrivateState,
                CharacterMobPublicState,
                CharacterMobClientState>
    {
        private const int DeathSpawnLootObjectsDefaultCount = 16;

        private const double DeathSpawnLootObjectsRadius = 19;

        private const int DeathSpawnMinionsDefaultCount = 8;

        private const double DeathSpawnMinionsRadius = 17;

        // The boss is regenerating only if it didn't receive any serious damage for a while.
        private const double HealthRegenerationPerSecond = 40;

        // The boss can move within the area in configured radius only.
        private const double MovementMaxRadius = 9;

        // Delay since the last damage before the HP regeneration will start.
        private const double RegenerationDelaySeconds = 6;

        private const double SpawnMinionsCheckDistance = 21;

        private const int SpawnMinionsPerPlayer = 1;

        // Max distance to provide victory LP bonus (if player beyond this distance the bonus will be not provided).
        private const double VictoryLearningPointsBonusMaxDistance = 17;

        private const double VictoryLearningPointsBonusToEachAlivePlayer = 50;

        private static readonly int MaxLootWinners;

        // Determines how often the boss will attempt to use the nova attack (a time interval in seconds).
        private static readonly Interval<double> NovaAttackInterval
            = new Interval<double>(min: 15, max: 21);

        private static readonly Lazy<IProtoStaticWorldObject> ProtoLootObjectLazy
            = new Lazy<IProtoStaticWorldObject>(
                GetProtoEntity<ObjectPragmiumQueenRemains>);

        // Determines the type of the minion creatures to spawn when boss is dead.
        private static readonly Lazy<IProtoCharacterMob> ProtoMinionObjectDeathLazy
            = new Lazy<IProtoCharacterMob>(
                GetProtoEntity<MobPragmiumBeetleMinion>);

        // Determines the type of the minion creatures to spawn when boss is using the nova attack.
        private static readonly Lazy<IProtoCharacterMob> ProtoMinionObjectLazy
            = new Lazy<IProtoCharacterMob>(
                GetProtoEntity<MobPragmiumBeetleMinion>);

        private static readonly double ServerBossDifficultyCoef;

        private IReadOnlyList<AiWeaponPreset> weaponsListNovaAttack;

        private IReadOnlyList<AiWeaponPreset> weaponsListPrimary;

        static MobPragmiumQueen()
        {
            var key = "BossDifficultyPragmiumQueen";
            var defaultValue = 5.0; // by default the boss is balanced for 5 players
            var description =
                @"Difficulty of the Pragmium Queen boss (and the amount of loot/reward).
                  The number is corresponding to the number of players necessary to kill the boss
                  with a reasonable challenge
                  (with mechs or without mechs but in T4 armor, machineguns, with Stimpacks).                  
                  You can setup this rate to make boss possible to kill
                  by a single player (set 1 or 1.5 for extra challenge and reward)
                  or any other number of players up to 10.
                  It's affecting the number of loot piles you get when the boss is killed.
                  The value range is from 1 to 10 (inclusive).";

            var requiredPlayersNumber = ServerRates.Get(key, defaultValue: defaultValue, description);
            {
                var clampedValue = MathHelper.Clamp(requiredPlayersNumber, 1, 10);
                if (clampedValue != requiredPlayersNumber)
                {
                    clampedValue = defaultValue;
                    ServerRates.Reset(key, defaultValue, description);
                }

                requiredPlayersNumber = clampedValue;
            }

            // coef range from 0.2 to 2.0
            ServerBossDifficultyCoef = requiredPlayersNumber / 5.0;

            MaxLootWinners = (int)Math.Ceiling(requiredPlayersNumber * 2);
            MaxLootWinners = Math.Max(MaxLootWinners, 5); // ensure at least 5 winners
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

        public override double StatDefaultHealthMax => 6500;

        // boss using different regeneration mechanism (much faster)
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
                    var bossPosition = character.Position;
                    using var tempListPlayersNearby = Api.Shared.GetTempList<ICharacter>();
                    Server.World.GetScopedByPlayers(character, tempListPlayersNearby);

                    foreach (var player in tempListPlayersNearby.AsList())
                    {
                        if (player.Position.DistanceSquaredTo(bossPosition)
                            <= VictoryLearningPointsBonusMaxDistance * VictoryLearningPointsBonusMaxDistance)
                        {
                            player.SharedGetTechnologies()
                                  .ServerAddLearningPoints(VictoryLearningPointsBonusToEachAlivePlayer,
                                                           allowModifyingByStat: false);
                        }
                    }

                    // explode
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
                                    epicenterPosition: bossPosition.ToVector2Ushort(),
                                    protoCharacterBoss: this,
                                    damageTracker: damageTracker,
                                    bossDifficultyCoef: ServerBossDifficultyCoef,
                                    lootObjectProto: ProtoLootObjectLazy.Value,
                                    lootObjectsDefaultCount: DeathSpawnLootObjectsDefaultCount,
                                    lootObjectsRadius: DeathSpawnLootObjectsRadius,
                                    maxLootWinners: MaxLootWinners);
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
            var bossDamageTracker = GetPrivateState(characterBoss).DamageTracker;
            var bossPosition = characterBoss.Position + (0, 1.0);

            // calculate how many minions required
            var minionsRequired = 1;
            using var tempListCharacters = Api.Shared.GetTempList<ICharacter>();
            Server.World.GetScopedByPlayers(characterBoss, tempListCharacters);

            foreach (var player in tempListCharacters.AsList())
            {
                if (player.Position.DistanceSquaredTo(bossPosition)
                    <= SpawnMinionsCheckDistance * SpawnMinionsCheckDistance)
                {
                    minionsRequired += SpawnMinionsPerPlayer;
                }
            }

            if (minionsRequired < 3)
            {
                // ensure there are at least 3 minions
                minionsRequired = 3;
            }

            // calculate how many minions present
            tempListCharacters.Clear();
            Server.World.GetScopedByCharacters(characterBoss, tempListCharacters.AsList(), onlyPlayerCharacters: false);

            var minionsHave = 0;
            var protoMobMinion = ProtoMinionObjectLazy.Value;
            foreach (var otherCharacter in tempListCharacters.AsList())
            {
                if (otherCharacter.IsNpc
                    && otherCharacter.ProtoGameObject == protoMobMinion
                    && otherCharacter.Position.DistanceSquaredTo(bossPosition)
                    <= SpawnMinionsCheckDistance * SpawnMinionsCheckDistance
                    && !otherCharacter.GetPublicState<CharacterMobPublicState>().IsDead)
                {
                    minionsHave++;
                }
            }

            //Logger.Dev($"Minions required: {minionsRequired} minions have: {minionsHave}");
            minionsRequired -= minionsHave;
            if (minionsRequired <= 0)
            {
                return;
            }

            // spawn minions
            var attemptsRemains = 300;

            const double spawnDistanceMin = 2.0;
            const double spawnDistanceMax = 2.5;
            while (minionsRequired > 0)
            {
                attemptsRemains--;
                if (attemptsRemains <= 0)
                {
                    // attempts exceeded
                    return;
                }

                var spawnDistance = spawnDistanceMin
                                    + RandomHelper.NextDouble() * (spawnDistanceMax - spawnDistanceMin);
                var angle = RandomHelper.NextDouble() * MathConstants.DoublePI;
                var spawnPosition = new Vector2Ushort(
                    (ushort)(bossPosition.X + spawnDistance * Math.Cos(angle)),
                    (ushort)(bossPosition.Y + spawnDistance * Math.Sin(angle)));

                if (ServerTrySpawnMinion(spawnPosition))
                {
                    // spawned successfully!
                    minionsRequired--;
                }
            }

            bool ServerTrySpawnMinion(Vector2Ushort spawnPosition)
            {
                var worldPosition = spawnPosition.ToVector2D();
                var spawnedCharacter = Server.Characters.SpawnCharacter(protoMobMinion, worldPosition);
                if (spawnedCharacter is null)
                {
                    return false;
                }

                // write this boss' damage tracker into the minion character
                // so any damage dealt to it will be counted in the winners ranking
                var privateState = spawnedCharacter.GetPrivateState<ICharacterPrivateStateWithBossDamageTracker>();
                privateState.DamageTracker = bossDamageTracker;
                return true;
            }
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
                var privateState = GetPrivateState((ICharacter)targetObject);

                // record the damage dealt by player
                privateState.DamageTracker.RegisterDamage(byCharacter, damageApplied);

                if (damageApplied > 1 / ServerBossDifficultyCoef)
                {
                    // record the last time a significant damage is dealt
                    privateState.LastDamageTime = Server.Game.FrameTime;
                }
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
                   .AddValue(this, StatName.DefenseChemical,  1.0)
                   .AddValue(this, StatName.DefensePsi,       1.0);
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

            if (data.IsFirstTimeInit)
            {
                data.PrivateState.HoldPosition = data.GameObject.TilePosition;
            }

            data.PrivateState.DamageTracker = new ServerBossDamageTracker();

            this.weaponsListPrimary = new AiWeaponPresetList()
                                      .Add(new AiWeaponPreset(GetProtoEntity<ItemWeaponPragmiumQueenMelee>()))
                                      .Add(new AiWeaponPreset(GetProtoEntity<ItemWeaponPragmiumQueenRanged>()))
                                      .ToReadReadOnly();

            this.weaponsListNovaAttack = new AiWeaponPresetList()
                                         .Add(new AiWeaponPreset(GetProtoEntity<ItemWeaponPragmiumQueenNova>()))
                                         .ToReadReadOnly();

            ServerMobWeaponHelper.TrySetWeapon(data.GameObject,
                                               this.weaponsListPrimary[0].ProtoWeapon,
                                               rebuildWeaponsCacheNow: false);
        }

        protected override void ServerUpdateMob(ServerUpdateData data)
        {
            var character = data.GameObject;
            var publicState = data.PublicState;

            if (publicState.IsDead)
            {
                return;
            }

            var privateState = data.PrivateState;
            var lastTargetCharacter = privateState.CurrentTargetCharacter;
            var deltaTime = data.DeltaTime;

            // Regenerate the health points a bit on every frame
            // if there was no damage dealt to boss recently.
            // Please note: the difficulty coefficient doesn't apply there
            // as the boss HP doesn't change with difficulty - only damage
            // to it is modified by the difficulty coefficient.
            if (Server.Game.FrameTime
                >= privateState.LastDamageTime + RegenerationDelaySeconds)
            {
                publicState.CurrentStats.ServerSetHealthCurrent(
                    (float)(publicState.CurrentStats.HealthCurrent
                            + HealthRegenerationPerSecond * deltaTime));
            }

            var weaponList = this.ServerSelectWeaponsList(privateState,
                                                          deltaTime,
                                                          out var isSwitchingToNovaAttack);

            ServerCharacterAiHelper.ProcessBossAi(
                character,
                weaponList,
                distanceEnemyTooClose: 7.5,
                distanceEnemyTooFar: 15.5,
                movementDirection: out var movementDirection,
                rotationAngleRad: out var rotationAngleRad);

            if (movementDirection != default
                && !ServerCanMoveInDirection(character.TilePosition.ToVector2D(),
                                             movementDirection,
                                             privateState.HoldPosition.ToVector2D()))
            {
                // cannot move in desired direction - too far from the position to hold
                movementDirection = default;
            }

            this.ServerSetMobInput(character, movementDirection, rotationAngleRad);

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
                privateState.WeaponState.SharedSetInputIsFiring(false);
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

            public double TimeToNextNovaAttack { get; set; }
        }
    }
}