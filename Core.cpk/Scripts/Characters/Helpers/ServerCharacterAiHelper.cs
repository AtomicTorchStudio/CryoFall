namespace AtomicTorch.CBND.CoreMod.Characters
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Vehicles;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Physics;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesServer;
    using AtomicTorch.GameEngine.Common.Helpers;
    using AtomicTorch.GameEngine.Common.Primitives;

    public static class ServerCharacterAiHelper
    {
        private const double FleeSoundRepeatInterval = 3;

        private static readonly IWorldServerService ServerWorldService
            = Api.IsServer
                  ? Api.Server.World
                  : null;

        private static readonly List<ICharacter> TempListPlayersInView
            = new List<ICharacter>();

        public static ICharacter GetClosestPlayer(ICharacter characterNpc)
        {
            byte? tileHeight = null;

            try
            {
                var playersInView = TempListPlayersInView;
                ServerWorldService.GetCharactersInView(characterNpc,
                                                       playersInView,
                                                       onlyPlayerCharacters: true);
                if (playersInView.Count == 0)
                {
                    return null;
                }

                ICharacter enemy = null;
                foreach (var player in playersInView)
                {
                    if (player.GetPublicState<ICharacterPublicState>().IsDead)
                    {
                        // do not pay attention to dead characters
                        continue;
                    }

                    if (player.ProtoCharacter.GetType() != typeof(PlayerCharacter))
                    {
                        // don't react on non-player prototype (spectator?)
                        continue;
                    }

                    var playerCharacterTile = player.Tile;
                    if (!tileHeight.HasValue)
                    {
                        var npcTile = characterNpc.Tile;
                        tileHeight = npcTile.Height;
                    }

                    if (playerCharacterTile.Height != tileHeight.Value)
                    {
                        // attack only on the same height characters
                        // unless there is a direct line of sight between the NPC and the target
                        if (AnyObstaclesBetween(characterNpc, player))
                        {
                            continue;
                        }
                    }

                    enemy = player;
                    break;
                }

                return enemy;
            }
            finally
            {
                TempListPlayersInView.Clear();
            }
        }

        public static void ProcessAggressiveAi(
            ICharacter characterNpc,
            bool isRetreating,
            bool isRetreatingForHeavyVehicles,
            double distanceRetreat,
            double distanceEnemyTooClose,
            double distanceEnemyTooFar,
            out Vector2F movementDirection,
            out double rotationAngleRad)
        {
            var characterNpcPrivateState = characterNpc.GetPrivateState<CharacterMobPrivateState>();
            var lastTargetCharacter = characterNpcPrivateState.CurrentTargetCharacter;
            var wasRetreating = characterNpcPrivateState.IsRetreating;

            var targetCharacter = GetClosestPlayer(characterNpc);
            CalculateDistanceAndDirectionToEnemy(characterNpc,
                                                 targetCharacter,
                                                 out var distanceToTarget,
                                                 out var directionToEnemyPosition,
                                                 out var directionToEnemyHitbox);
            if (isRetreatingForHeavyVehicles
                && !(targetCharacter is null)
                && !targetCharacter.IsNpc)
            {
                // determine if the enemy player riding a heavy vehicle
                var protoVehicle = PlayerCharacter.GetPublicState(targetCharacter).CurrentVehicle?.ProtoGameObject
                                       as IProtoVehicle;
                if (protoVehicle?.IsHeavyVehicle ?? false)
                {
                    // retreat from heavy vehicle
                    distanceRetreat = Math.Max(7, distanceRetreat);
                    isRetreating = true;
                }
            }

            if (targetCharacter == characterNpcPrivateState.CurrentAgroCharacter)
            {
                // increase distances if agro on this character
                distanceRetreat *= 3;
                distanceEnemyTooFar *= 3;
            }

            if (isRetreating)
            {
                if ((double.IsNaN(distanceToTarget)
                     || distanceToTarget > characterNpcPrivateState.AttackRange)
                    && characterNpcPrivateState.WeaponState.CooldownSecondsRemains <= 0)
                {
                    // look away when retreating
                    // and the enemy is not within the attack range
                    // and not attacked recently (so the attack cooldown (and so the animation) is finished)
                    directionToEnemyHitbox *= -1;
                }

                if (distanceToTarget <= distanceRetreat)
                {
                    // retreat
                    movementDirection = directionToEnemyPosition * -1;
                }
                else
                {
                    // retreat completed
                    movementDirection = Vector2F.Zero;
                    targetCharacter = null;
                    isRetreating = false;
                }
            }
            else // not retreating
            {
                var isTargetTooFar = distanceToTarget > distanceEnemyTooFar;
                movementDirection = distanceToTarget < distanceEnemyTooClose
                                    || isTargetTooFar
                                        ? Vector2F.Zero // too close or too far
                                        : directionToEnemyPosition;

                if (isTargetTooFar)
                {
                    targetCharacter = null;
                }
            }

            characterNpcPrivateState.IsRetreating = isRetreating;
            characterNpcPrivateState.CurrentTargetCharacter = targetCharacter;

            rotationAngleRad = characterNpc.GetPublicState<CharacterMobPublicState>()
                                           .AppliedInput
                                           .RotationAngleRad;
            LookOnEnemy(directionToEnemyHitbox, ref rotationAngleRad);

            var isAttacking = !double.IsNaN(distanceToTarget)
                              && distanceToTarget <= characterNpcPrivateState.AttackRange;

            if (isRetreating && isAttacking)
            {
                // don't attack every time when retreating
                isAttacking = RandomHelper.Next(0, 6) == 0;
            }

            characterNpcPrivateState.WeaponState.SharedSetInputIsFiring(isAttacking);

            if (targetCharacter is null)
            {
                return;
            }

            if (isRetreating)
            {
                TryPlayFleeSound(characterNpc, characterNpcPrivateState);
                return;
            }

            // not retreating
            if (wasRetreating 
                || lastTargetCharacter != targetCharacter)
            {
                // changed an enemy
                var protoMob = (IProtoCharacterMob)characterNpc.ProtoCharacter;
                protoMob.ServerPlaySound(characterNpc, CharacterSound.Aggression);
            }
        }

        public static void ProcessRetreatingAi(
            ICharacter characterNpc,
            double distanceRetreat,
            out Vector2F movementDirection,
            out double rotationAngleRad)
        {
            var characterNpcPrivateState = characterNpc.GetPrivateState<CharacterMobPrivateState>();
            var targetCharacter = GetClosestPlayer(characterNpc);

            if (targetCharacter == characterNpcPrivateState.CurrentAgroCharacter)
            {
                // increase distances if agro on this character
                distanceRetreat *= 3;
            }

            CalculateDistanceAndDirectionToEnemy(characterNpc,
                                                 targetCharacter,
                                                 out var distanceToEnemy,
                                                 out var directionToEnemy,
                                                 directionToEnemyHitbox: out _);
            directionToEnemy *= -1;

            var isRetreating = distanceToEnemy <= distanceRetreat;
            if (isRetreating)
            {
                // retreat
                movementDirection = directionToEnemy;
            }
            else
            {
                // retreat completed
                movementDirection = Vector2F.Zero;
            }

            characterNpcPrivateState.IsRetreating = isRetreating;

            rotationAngleRad = characterNpc.GetPublicState<CharacterMobPublicState>()
                                           .AppliedInput
                                           .RotationAngleRad;
            // look away from the enemy
            LookOnEnemy(directionToEnemy, ref rotationAngleRad);

            if (isRetreating)
            {
                TryPlayFleeSound(characterNpc, characterNpcPrivateState);
            }
        }

        private static bool AnyObstaclesBetween(ICharacter npc, ICharacter player)
        {
            var physicsSpace = npc.PhysicsBody.PhysicsSpace;
            var npcCharacterCenter = npc.Position + npc.PhysicsBody.CenterOffset;
            var playerCharacterCenter = player.Position + player.PhysicsBody.CenterOffset;

            using var obstaclesOnTheWay = physicsSpace.TestLine(
                npcCharacterCenter,
                playerCharacterCenter,
                CollisionGroup.GetDefault(),
                sendDebugEvent: false);
            foreach (var test in obstaclesOnTheWay.AsList())
            {
                var testPhysicsBody = test.PhysicsBody;
                if (testPhysicsBody.AssociatedProtoTile != null)
                {
                    // obstacle tile on the way
                    return true;
                }

                var testWorldObject = testPhysicsBody.AssociatedWorldObject;
                if (testWorldObject == npc
                    || testWorldObject == player
                    || testWorldObject.ProtoGameObject is IProtoVehicle)
                {
                    // not an obstacle - it's one of the characters or vehicle
                    continue;
                }

                // obstacle object on the way
                return true;
            }

            return false;
        }

        private static void CalculateDistanceAndDirectionToEnemy(
            ICharacter characterNpc,
            ICharacter enemyCharacter,
            out double distanceToEnemy,
            out Vector2F directionToEnemyPosition,
            out Vector2F directionToEnemyHitbox)
        {
            if (enemyCharacter == null)
            {
                distanceToEnemy = double.NaN;
                directionToEnemyPosition = directionToEnemyHitbox = Vector2F.Zero;
                return;
            }

            var deltaPos = enemyCharacter.Position - characterNpc.Position;
            distanceToEnemy = deltaPos.Length;
            directionToEnemyPosition = (Vector2F)deltaPos;

            directionToEnemyHitbox = new Vector2F(directionToEnemyPosition.X,
                                                  directionToEnemyPosition.Y
                                                  + enemyCharacter.ProtoCharacter.CharacterWorldWeaponOffsetMelee
                                                  - characterNpc.ProtoCharacter.CharacterWorldWeaponOffsetMelee);
        }

        private static void LookOnEnemy(Vector2F directionToEnemyHitbox, ref double rotationAngleRad)
        {
            if (directionToEnemyHitbox != Vector2F.Zero)
            {
                rotationAngleRad = Math.Abs(
                    Math.Atan2(directionToEnemyHitbox.Y, directionToEnemyHitbox.X) + 2 * Math.PI);
            }
        }

        private static void TryPlayFleeSound(ICharacter characterNpc, CharacterMobPrivateState characterNpcPrivateState)
        {
            var serverTime = Api.Server.Game.FrameTime;
            var timeSinceLastFleeSound = serverTime - characterNpcPrivateState.LastFleeSoundTime;
            if (timeSinceLastFleeSound < FleeSoundRepeatInterval)
            {
                // already played the flee sound recently
                return;
            }

            if (timeSinceLastFleeSound < 2 * FleeSoundRepeatInterval)
            {
                // played the flee sound quite recently
                // we don't want to play it like it's a cuckoo clock (exactly every X seconds)
                // so let's apply some randomization here
                if (RandomHelper.Next(0, 5) != 0)
                {
                    return;
                }
            }

            characterNpcPrivateState.LastFleeSoundTime = serverTime;
            var protoMob = (IProtoCharacterMob)characterNpc.ProtoCharacter;
            protoMob.ServerPlaySound(characterNpc, CharacterSound.Flee);
        }
    }
}