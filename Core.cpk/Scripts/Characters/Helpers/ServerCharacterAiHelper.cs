namespace AtomicTorch.CBND.CoreMod.Characters
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Physics;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesServer;
    using AtomicTorch.GameEngine.Common.Primitives;

    public static class ServerCharacterAiHelper
    {
        private static readonly IWorldServerService ServerWorldService
            = Api.IsServer
                  ? Api.Server.World
                  : null;

        private static readonly List<ICharacter> TempListPlayersInView
            = new List<ICharacter>();

        public static ICharacter GetClosestPlayer(ICharacter characterNpc)
        {
            var npcTile = characterNpc.Tile;
            var tileHeight = npcTile.Height;

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
                    if (playerCharacterTile.Height != tileHeight)
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
            double distanceRetreat,
            double distanceEnemyTooClose,
            double distanceEnemyTooFar,
            out Vector2F movementDirection,
            out double rotationAngleRad)
        {
            var characterNpcPrivateState = characterNpc.GetPrivateState<CharacterMobPrivateState>();
            var enemyCharacter = GetClosestPlayer(characterNpc);

            CalculateDistanceAndDirectionToEnemy(characterNpc,
                                                 enemyCharacter,
                                                 out var distanceToEnemy,
                                                 out var directionToEnemy);

            if (enemyCharacter == characterNpcPrivateState.CurrentAgroCharacter)
            {
                // increase distances if agro on this character
                distanceRetreat *= 3;
                distanceEnemyTooFar *= 3;
            }

            if (isRetreating)
            {
                movementDirection = directionToEnemy = directionToEnemy * -1;
                if (distanceToEnemy > distanceRetreat)
                {
                    // retreat completed
                    movementDirection = Vector2F.Zero;
                }
            }
            else
            {
                movementDirection = distanceToEnemy < distanceEnemyTooClose
                                    || distanceToEnemy > distanceEnemyTooFar
                                        ? Vector2F.Zero // too close or too far
                                        : directionToEnemy;
            }

            rotationAngleRad = characterNpc.GetPublicState<CharacterMobPublicState>()
                                           .AppliedInput
                                           .RotationAngleRad;
            LookOnEnemy(directionToEnemy, ref rotationAngleRad);

            var isFiring = !isRetreating
                           && !double.IsNaN(distanceToEnemy)
                           && distanceToEnemy <= characterNpcPrivateState.AttackRange;
            characterNpcPrivateState.WeaponState.SharedSetInputIsFiring(isFiring);
        }

        public static void ProcessRetreatingAi(
            ICharacter characterNpc,
            double distanceRetreat,
            out Vector2F movementDirection,
            out double rotationAngleRad)
        {
            var characterNpcPrivateState = characterNpc.GetPrivateState<CharacterMobPrivateState>();
            var enemyCharacter = GetClosestPlayer(
                characterNpc);

            if (enemyCharacter == characterNpcPrivateState.CurrentAgroCharacter)
            {
                // increase distances if agro on this character
                distanceRetreat *= 3;
            }

            CalculateDistanceAndDirectionToEnemy(characterNpc,
                                                 enemyCharacter,
                                                 out var distanceToEnemy,
                                                 out var directionToEnemy);

            movementDirection = directionToEnemy = directionToEnemy * -1;
            if (distanceToEnemy > distanceRetreat)
            {
                // retreat completed
                movementDirection = Vector2F.Zero;
            }

            rotationAngleRad = characterNpc.GetPublicState<CharacterMobPublicState>()
                                           .AppliedInput
                                           .RotationAngleRad;
            LookOnEnemy(directionToEnemy, ref rotationAngleRad);
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
            foreach (var test in obstaclesOnTheWay)
            {
                var testPhysicsBody = test.PhysicsBody;
                if (testPhysicsBody.AssociatedProtoTile != null)
                {
                    // obstacle tile on the way
                    return true;
                }

                var testWorldObject = testPhysicsBody.AssociatedWorldObject;
                if (testWorldObject == npc
                    || testWorldObject == player)
                {
                    // not an obstacle - it's one of the characters
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
            out Vector2F directionToEnemy)
        {
            if (enemyCharacter == null)
            {
                distanceToEnemy = double.NaN;
                directionToEnemy = Vector2F.Zero;
                return;
            }

            var deltaPos = enemyCharacter.Position - characterNpc.Position;
            distanceToEnemy = deltaPos.Length;
            directionToEnemy = (Vector2F)deltaPos;
        }

        private static void LookOnEnemy(Vector2F directionToEnemy, ref double rotationAngleRad)
        {
            if (directionToEnemy != Vector2F.Zero)
            {
                rotationAngleRad = Math.Abs(
                    Math.Atan2(directionToEnemy.Y, directionToEnemy.X) + 2 * Math.PI);
            }
        }
    }
}