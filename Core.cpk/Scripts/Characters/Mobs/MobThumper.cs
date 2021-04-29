namespace AtomicTorch.CBND.CoreMod.Characters.Mobs
{
    using System;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.CharacterSkeletons;
    using AtomicTorch.CBND.CoreMod.Helpers;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Items.Weapons;
    using AtomicTorch.CBND.CoreMod.Items.Weapons.MobWeapons;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Stats;
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.CoreMod.Vehicles;
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Physics;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.GameEngine.Common.DataStructures;
    using AtomicTorch.GameEngine.Common.Helpers;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class MobThumper
        : ProtoCharacterMob
            <MobThumper.PrivateState,
                MobThumper.PublicState,
                CharacterMobClientState>
    {
        /// <summary>
        /// Max distance to target - if it's further than that, a rush attack will be performed.
        /// </summary>
        private const double AttackRushMinDistance = 3.0;

        /// <summary>
        /// Desired distance for normal attack (when not in rush mode).
        /// The creature will approach to this distance (if distance to target is shorter than AttackRushMinDistance).
        /// </summary>
        private const double MinDistanceToTarget = 1.75;

        private const double MobPhysicsBoundingBoxSize = 1.0;

        private const double RushSpeedMultiplier = 1.0;

        /// <summary>
        /// If cannot perform a rush attack due to obstacles, the character will rush in a different direction.
        /// This multiplier limits the distance of rush run in this case.
        /// </summary>
        private const double RushWrongDirectionRangeMultiplier = 0.75;

        private const double StateDurationRushAttack = 1.5 / RushSpeedMultiplier;

        private const double StateDurationRushAttackCooldown = 1.0;

        private const double StateDurationRushAttackPreparation = 1.0;

        private const double StatMoveSpeedNormal = 2.0;

        private const double StatMoveSpeedRushAttackState = 7.0 * RushSpeedMultiplier;

        private const double TargetPredictionInterpolationRate = 20;

        /// <summary>
        /// If AI detects that there are obstacles in the way of the attack,
        /// it will try to find a better angle by applying an offset angle (with positive and negative signs, randomly).
        /// </summary>
        private static readonly float[] RushAttackFallbackOffsetAngles = { 30, 45, 60, 90, 120, 165, 180 };

        private static readonly SoundResource RushPreparationSoundResource
            = new("Skeletons/Thumper/Character/RushPreparation.ogg");

        private ItemWeaponMobThumperNormalAttack weaponProtoNormalAttack;

        private ItemWeaponMobThumperRushAttack weaponProtoRushAttack;

        [NotPersistent]
        [RemoteEnum]
        public enum ThumperAiState : byte
        {
            Idle = 0,

            RushAttackPreparation = 10,

            RushAttack = 20,

            RushAttackCooldown = 30
        }

        public override bool AiIsRunAwayFromHeavyVehicles => false;

        public override float CharacterWorldHeight => 1.95f * SkeletonThumper.Scale;

        public override double CorpseInteractionAreaScale => 1.7;

        public override double MobKillExperienceMultiplier => 3.0;

        public override string Name => "Thumper";

        public override ObjectMaterial ObjectMaterial => ObjectMaterial.HardTissues;

        public override double PhysicsBodyAccelerationCoef => 20;

        public override double PhysicsBodyFriction => 25;

        // high update interval is necessary to ensure a better rush run direction's prediction accuracy
        public override double ServerUpdateIntervalSeconds => 0.05;

        public override double StatDefaultHealthMax => 400;

        public override double StatMoveSpeed => StatMoveSpeedNormal;

        protected virtual double DistanceEnemyTooFar => 14;

        protected virtual double DistanceEnemyTooFarWhenAggro => 36;

        protected IReadOnlyStatsDictionary ProtoCharacterEffectsRushAttack { get; private set; }

        protected override void ClientInitialize(ClientInitializeData data)
        {
            base.ClientInitialize(data);

            var character = data.GameObject;
            var clientState = data.ClientState;

            // listen to the AI state change and play the rush attack sound when necessarily
            data.PublicState.ClientSubscribe(
                _ => _.AiState,
                aiState =>
                {
                    if (aiState != ThumperAiState.RushAttackPreparation
                        || clientState.CurrentProtoSkeleton is null)
                    {
                        return;
                    }

                    var emitter = Client.Audio.PlayOneShot(RushPreparationSoundResource,
                                                           character);
                    var (soundPresetCharacter, _) = clientState.CurrentProtoSkeleton.GetSoundPresets(character);
                    soundPresetCharacter.ApplyCustomDistance(emitter);
                },
                clientState);
        }

        protected override void FillDefaultEffects(Effects effects)
        {
            base.FillDefaultEffects(effects);

            effects.AddValue(this, StatName.DefenseImpact, 0.4)
                   .AddValue(this, StatName.DefenseKinetic,   0.8)
                   .AddValue(this, StatName.DefenseExplosion, 0.3)
                   .AddValue(this, StatName.DefenseHeat,      0.4)
                   .AddValue(this, StatName.DefenseCold,      0.0)
                   .AddValue(this, StatName.DefenseChemical,  0.5)
                   .AddPercent(this, StatName.DazedIncreaseRateMultiplier, -100);
        }

        protected override void PrepareProtoCharacterMob(
            out ProtoCharacterSkeleton skeleton,
            ref double scale,
            DropItemsList lootDroplist)
        {
            var protoCharacterEffectsRushAttack = new Effects();
            protoCharacterEffectsRushAttack.Merge(this.ProtoCharacterDefaultEffects);
            protoCharacterEffectsRushAttack.AddValue(
                this,
                StatName.MoveSpeed,
                (StatMoveSpeedRushAttackState * GameplayConstants.CharacterMoveSpeedMultiplier) - this.StatMoveSpeed);
            this.ProtoCharacterEffectsRushAttack = protoCharacterEffectsRushAttack.ToReadOnly();

            skeleton = GetProtoEntity<SkeletonThumper>();

            // primary loot
            lootDroplist
                .Add<ItemFuelSack>(count: 5, countRandom: 5)
                .Add<ItemCoal>(count: 2,     countRandom: 2)
                .Add<ItemKeinite>(count: 4,  countRandom: 2);

            // extra loot
            lootDroplist.Add(condition: SkillHunting.ServerRollExtraLoot,
                             nestedList: new DropItemsList(outputs: 1)
                                 .Add<ItemCoal>(count: 2));
        }

        protected override void ServerForceBuildFinalStatsCache(
            PrivateState privateState,
            PublicState publicState)
        {
            SharedCharacterStatsHelper.RefreshCharacterFinalStatsCache(
                protoEffects: publicState.AiState == ThumperAiState.RushAttack
                              && publicState.ServerStateTimeRemains > 0
                                  ? this.ProtoCharacterEffectsRushAttack
                                  : this.ProtoCharacterDefaultEffects,
                publicState,
                privateState);
        }

        protected override void ServerInitializeCharacterMob(ServerInitializeData data)
        {
            base.ServerInitializeCharacterMob(data);

            this.weaponProtoNormalAttack = GetProtoEntity<ItemWeaponMobThumperNormalAttack>();
            this.weaponProtoRushAttack = GetProtoEntity<ItemWeaponMobThumperRushAttack>();
            data.PrivateState.WeaponState.SharedSetWeaponProtoOnly(this.weaponProtoNormalAttack);
            data.PublicState.SharedSetCurrentWeaponProtoOnly(this.weaponProtoNormalAttack);

            ServerMobWeaponHelper.TrySetWeapon(data.GameObject,
                                               this.weaponProtoNormalAttack,
                                               rebuildWeaponsCacheNow: false);
        }

        protected override void ServerUpdateMob(ServerUpdateData data)
        {
            var character = data.GameObject;

            var privateState = character.GetPrivateState<CharacterMobPrivateState>();
            var weaponState = privateState.WeaponState;

            var lastTargetCharacter = privateState.CurrentTargetCharacter;

            var targetCharacter = ServerCharacterAiHelper.GetClosestTargetPlayer(character);
            ServerCharacterAiHelper.CalculateDistanceAndDirectionToEnemy(
                character,
                targetCharacter,
                isRangedWeapon:
                weaponState.ProtoWeapon is IProtoItemWeaponRanged,
                out var distanceToTarget,
                out var directionToEnemyPosition,
                out var directionToEnemyHitbox);

            directionToEnemyPosition = directionToEnemyPosition.Normalized;

            var distanceEnemyTooFar = this.DistanceEnemyTooFar;
            if (ReferenceEquals(targetCharacter, privateState.CurrentAggroCharacter))
            {
                // increase distances if aggro on this character
                distanceEnemyTooFar = this.DistanceEnemyTooFarWhenAggro;
            }

            // not retreating
            var isTargetTooFar = distanceToTarget > distanceEnemyTooFar;
            var movementDirection = distanceToTarget < MinDistanceToTarget
                                    || isTargetTooFar
                                        ? Vector2F.Zero // too close or too far
                                        : directionToEnemyPosition;

            if (isTargetTooFar)
            {
                targetCharacter = null;
            }

            privateState.CurrentTargetCharacter = targetCharacter;

            double rotationAngleRad = character.GetPublicState<CharacterMobPublicState>()
                                               .AppliedInput
                                               .RotationAngleRad;
            ServerCharacterAiHelper.LookOnEnemy(directionToEnemyHitbox, ref rotationAngleRad);

            var isAttacking = false;

            if (!double.IsNaN(distanceToTarget))
            {
                isAttacking = distanceToTarget <= privateState.AttackRange;
            }

            if (!isAttacking)
            {
                weaponState.SharedSetInputIsFiring(false);
            }

            if (targetCharacter is not null)
            {
                if (lastTargetCharacter != targetCharacter)
                {
                    // changed an enemy
                    var protoMob = (IProtoCharacterMob)character.ProtoCharacter;
                    protoMob.ServerPlaySound(character, CharacterSound.Aggression);
                }
            }

            this.ServerUpdateThumperAiState(character,
                                            data.PublicState,
                                            privateState,
                                            data.DeltaTime,
                                            weaponState,
                                            isAttacking,
                                            targetCharacter,
                                            isTargetTooFar,
                                            distanceToTarget,
                                            movementDirection,
                                            rotationAngleRad);

            if (privateState.FinalStatsCache.IsDirty)
            {
                this.SharedRefreshFinalCacheIfNecessary(character);
            }
        }

        /// <summary>
        /// After the rush attack preparation, determine whether there are any obstacles
        /// and try to avoid them.
        /// </summary>
        private static void ServerOnRushAttackStart(ICharacter characterNpc, PublicState publicState)
        {
            var targetCharacter = Server.World.GetGameObjectById<ICharacter>(GameObjectType.Character,
                                                                             publicState.ServerRushAttackTargetId);

            if (targetCharacter is null
                || targetCharacter.IsNpc
                || PlayerCharacter.GetPublicState(targetCharacter).IsDead)
            {
                // don't correct the target direction
                return;
            }

            var originalRushAttackDirection = publicState.ServerRushAttackDirection;
            var rushAttackDirection = originalRushAttackDirection.ToVector2D();
            if (IsGoodRushDirection(characterNpc, rushAttackDirection))
            {
                return;
            }

            // no direct attack direction
            // perform a shorter run 
            publicState.ServerStateTimeRemains *= RushWrongDirectionRangeMultiplier;

            // try to find a better rush direction
            foreach (var angle in RushAttackFallbackOffsetAngles)
            {
                var lastFlipDirection = 0;
                rushAttackDirection = CorrectDirection(originalRushAttackDirection,
                                                       angle: angle,
                                                       ref lastFlipDirection);
                if (IsGoodRushDirection(characterNpc, rushAttackDirection))
                {
                    return;
                }

                rushAttackDirection = CorrectDirection(originalRushAttackDirection,
                                                       angle: angle,
                                                       ref lastFlipDirection);
                if (IsGoodRushDirection(characterNpc, rushAttackDirection))
                {
                    return;
                }
            }

            // cannot do better, let's run this way for now
            return;

            // The rush direction is good if three lines (matching the creature bounding box)
            // could be casted forward and detect no obstacles.
            static bool IsGoodRushDirection(ICharacter characterNpc, Vector2D rushAttackDirection)
            {
                var physicsSpace = characterNpc.PhysicsBody.PhysicsSpace;
                var rustAttackEndPosition = characterNpc.Position
                                            + rushAttackDirection * AttackRushMinDistance;

                using var tempLineTestResults0 = physicsSpace.TestLine(
                    characterNpc.Position,
                    rustAttackEndPosition,
                    CollisionGroup.Default);

                if (!TestDirection(tempLineTestResults0, characterNpc))
                {
                    return false;
                }

                var offset1 = rushAttackDirection.RotateDeg(-90) * MobPhysicsBoundingBoxSize / 2;
                using var tempLineTestResults1 = physicsSpace.TestLine(
                    characterNpc.Position + offset1,
                    rustAttackEndPosition + offset1,
                    CollisionGroup.Default);

                if (!TestDirection(tempLineTestResults1, characterNpc))
                {
                    return false;
                }

                var offset2 = rushAttackDirection.RotateDeg(90) * MobPhysicsBoundingBoxSize / 2;
                using var tempLineTestResults2 = physicsSpace.TestLine(
                    characterNpc.Position + offset2,
                    rustAttackEndPosition + offset2,
                    CollisionGroup.Default);

                if (!TestDirection(tempLineTestResults2, characterNpc))
                {
                    return false;
                }

                return true;

                bool TestDirection(ITempList<TestResult> tempLineTestResults, ICharacter character)
                {
                    foreach (var testResult in tempLineTestResults.AsList())
                    {
                        var testResultPhysicsBody = testResult.PhysicsBody;

                        var attackedProtoTile = testResultPhysicsBody.AssociatedProtoTile;
                        if (attackedProtoTile is not null)
                        {
                            // tile on the way - blocking damage ray
                            return false;
                        }

                        var damagedObject = testResultPhysicsBody.AssociatedWorldObject;
                        if (ReferenceEquals(damagedObject, character))
                        {
                            // ignore collision with self
                            continue;
                        }

                        if (!(damagedObject.ProtoGameObject is IDamageableProtoWorldObject))
                        {
                            // obstacle
                            return false;
                        }

                        if (damagedObject is ICharacter { IsNpc: false }
                            || damagedObject.ProtoGameObject is IProtoVehicle)
                        {
                            // any character or a vehicle
                            return true;
                        }

                        // obstacle
                        return false;
                    }

                    return true;
                }
            }

            Vector2D CorrectDirection(Vector2F direction, float angle, ref int lastFlipDirection)
            {
                int flipDirection;
                if (lastFlipDirection == 0)
                {
                    flipDirection = (RandomHelper.Next(0, 2) == 0
                                         ? -1
                                         : 1);
                }
                else
                {
                    flipDirection = -lastFlipDirection;
                }

                lastFlipDirection = flipDirection;
                rushAttackDirection = (Vector2D)direction.RotateDeg(angle * flipDirection);

                publicState.ServerRushAttackDirection = (Vector2F)rushAttackDirection;
                double rotationAngleRad = 0;
                ServerCharacterAiHelper.LookOnEnemy(publicState.ServerRushAttackDirection, ref rotationAngleRad);
                publicState.ServerRushAttackViewAngle = rotationAngleRad;
                return rushAttackDirection;
            }
        }

        /// <summary>
        /// Thumper AI is a state machine (see State class).
        /// </summary>
        private void ServerUpdateThumperAiState(
            ICharacter character,
            PublicState publicState,
            CharacterMobPrivateState privateState,
            double deltaTime,
            WeaponState weaponState,
            bool isAttacking,
            ICharacter targetCharacter,
            bool isTargetTooFar,
            double distanceToTarget,
            Vector2F movementDirection,
            double rotationAngleRad)
        {
            publicState.ServerStateTimeRemains -= deltaTime;

            switch (publicState.AiState)
            {
                case ThumperAiState.Idle:
                {
                    // can stay indefinitely in this state
                    publicState.ServerStateTimeRemains = 0;

                    ServerMobWeaponHelper.TrySetWeapon(character,
                                                       this.weaponProtoNormalAttack,
                                                       rebuildWeaponsCacheNow: true);

                    // consider a rush attack or simply perform a normal attack
                    if (targetCharacter is null
                        || isTargetTooFar)
                    {
                        // has no target or the target is too far - stay idle
                        this.ServerSetMobInput(character,
                                               movementDirection: default,
                                               publicState.ServerRushAttackViewAngle);
                    }
                    else if (distanceToTarget < AttackRushMinDistance)
                    {
                        // the target is too close - perform a normal attack
                        this.ServerSetMobInput(character, movementDirection, rotationAngleRad);
                        weaponState.SharedSetInputIsFiring(isAttacking);
                    }
                    else
                    {
                        // the target is within a rush attack range
                        publicState.GoToAiState(ThumperAiState.RushAttackPreparation);
                        publicState.ServerRushAttackViewAngle = rotationAngleRad;
                        publicState.ServerRushAttackTargetId = targetCharacter.Id;
                        publicState.ServerRushAttackTargetLastPosition = targetCharacter.Position;
                        publicState.ServerRushAttackTargetVelocity = Vector2D.Zero;
                        publicState.ServerRushAttackDirection
                            = (targetCharacter.Position - character.Position).ToVector2F().Normalized;
                    }

                    break;
                }

                case ThumperAiState.RushAttackPreparation:
                {
                    ServerMobWeaponHelper.TrySetWeapon(character,
                                                       this.weaponProtoRushAttack,
                                                       rebuildWeaponsCacheNow: true);

                    if (movementDirection != default
                        && targetCharacter.Id == publicState.ServerRushAttackTargetId)
                    {
                        // update target direction
                        publicState.ServerRushAttackViewAngle = rotationAngleRad;

                        var oldTargetVelocity = publicState.ServerRushAttackTargetVelocity;
                        var newTargetVelocity =
                            SharedProjectileDirectionHelper.ServerGetTargetVelocity(targetCharacter);

                        // interpolate the target velocity so it's not so easy
                        // to fool the AI by rapidly changing the movement direction
                        newTargetVelocity = new Vector2D(
                            MathHelper.LerpWithDeltaTime(oldTargetVelocity.X,
                                                         newTargetVelocity.X,
                                                         deltaTime,
                                                         TargetPredictionInterpolationRate),
                            MathHelper.LerpWithDeltaTime(oldTargetVelocity.Y,
                                                         newTargetVelocity.Y,
                                                         deltaTime,
                                                         TargetPredictionInterpolationRate));

                        publicState.ServerRushAttackDirection
                            = SharedProjectileDirectionHelper
                              .ServerCalculateInterceptDirectionToTargetCharacter(
                                  character.Position,
                                  targetCharacter.Position,
                                  newTargetVelocity,
                                  StatMoveSpeedRushAttackState).ToVector2F();

                        // draw the planned attack line
                        /*SharedEditorPhysicsDebugger.ServerSendDebugPhysicsTesting(
                            new LineSegmentShape(character.Position,
                                                 character.Position
                                                 + (publicState.ServerRushAttackDirection
                                                    * StateDurationRushAttack
                                                    * StatMoveSpeedRushAttackState),
                                                 CollisionGroups.HitboxMelee));*/

                        this.ServerSetMobInput(character,
                                               movementDirection: default,
                                               publicState.ServerRushAttackViewAngle);
                    }

                    if (publicState.ServerStateTimeRemains > 0)
                    {
                        // still preparing for the rush attack
                        break;
                    }

                    // start the rush attack
                    publicState.GoToAiState(ThumperAiState.RushAttack);
                    privateState.SetFinalStatsCacheIsDirty();
                    ServerOnRushAttackStart(character, publicState);
                    break;
                }

                case ThumperAiState.RushAttack:
                {
                    // perform a bull rush attack!
                    if (publicState.ServerStateTimeRemains > 0)
                    {
                        // rush attack
                        this.ServerSetMobInput(character,
                                               movementDirection: publicState.ServerRushAttackDirection,
                                               publicState.ServerRushAttackViewAngle);

                        weaponState.SharedSetInputIsFiring(isAttacking);
                    }
                    else
                    {
                        // attack finished
                        privateState.SetFinalStatsCacheIsDirty();
                        this.ServerSetMobInput(character,
                                               movementDirection: default,
                                               publicState.ServerRushAttackViewAngle);
                        publicState.GoToAiState(ThumperAiState.RushAttackCooldown);
                    }

                    break;
                }

                case ThumperAiState.RushAttackCooldown:
                {
                    ServerMobWeaponHelper.TrySetWeapon(character,
                                                       this.weaponProtoNormalAttack,
                                                       rebuildWeaponsCacheNow: true);

                    weaponState.SharedSetInputIsFiring(false);

                    if (publicState.ServerStateTimeRemains <= 0)
                    {
                        publicState.ServerStateTimeRemains = 0;
                        publicState.GoToAiState(ThumperAiState.Idle);
                    }

                    break;
                }

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// As this mob is sometimes used as a minion, the boss damage tracker is necessary.
        /// </summary>
        public class PrivateState : CharacterMobPrivateState, ICharacterPrivateStateWithBossDamageTracker
        {
            [TempOnly]
            public ServerBossDamageTracker DamageTracker { get; set; }
        }

        public class PublicState : CharacterMobPublicState
        {
            [SyncToClient]
            [TempOnly]
            public ThumperAiState AiState { get; private set; }

            [TempOnly]
            public Vector2F ServerRushAttackDirection { get; set; }

            [TempOnly]
            public uint ServerRushAttackTargetId { get; set; }

            [TempOnly]
            public Vector2D ServerRushAttackTargetLastPosition { get; set; }

            [TempOnly]
            public Vector2D ServerRushAttackTargetVelocity { get; set; }

            [TempOnly]
            public double ServerRushAttackViewAngle { get; set; }

            [TempOnly]
            public double ServerStateTimeRemains { get; set; }

            public void GoToAiState(ThumperAiState state)
            {
                if (this.AiState == state)
                {
                    return;
                }

                this.AiState = state;
                this.ServerStateTimeRemains
                    = state switch
                    {
                        ThumperAiState.Idle                  => 0,
                        ThumperAiState.RushAttackPreparation => StateDurationRushAttackPreparation,
                        ThumperAiState.RushAttack            => StateDurationRushAttack,
                        ThumperAiState.RushAttackCooldown    => StateDurationRushAttackCooldown,
                        _                                    => throw new ArgumentOutOfRangeException()
                    };
            }
        }
    }
}