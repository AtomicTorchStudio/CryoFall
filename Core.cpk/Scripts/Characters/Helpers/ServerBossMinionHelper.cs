namespace AtomicTorch.CBND.CoreMod.Characters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Extensions;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Helpers;
    using AtomicTorch.GameEngine.Common.Primitives;

    public static class ServerBossMinionHelper
    {
        public static void ServerProcessMinions(
            ICharacter characterBoss,
            IProtoCharacterMob protoMinion,
            List<ICharacter> minionsList,
            out int spawnedMinionsCount,
            double despawnDistanceSqr)
        {
            var bossPosition = characterBoss.Position;
            spawnedMinionsCount = 0;

            for (var index = minionsList.Count - 1; index >= 0; index--)
            {
                var minion = minionsList[index];
                if (minion.GetPublicState<CharacterMobPublicState>().IsDead)
                {
                    //Logger.Dev("The minion is dead: " + minion);
                    minionsList.RemoveAt(index);
                    continue;
                }

                if (minion.Position.DistanceSquaredTo(bossPosition)
                    >= despawnDistanceSqr)
                {
                    //Logger.Dev("The minion is too far, despawn it: " + minion);
                    protoMinion.ServerSetSpawnState(minion, MobSpawnState.Despawning);
                    minionsList.RemoveAt(index);
                    continue;
                }

                spawnedMinionsCount++;
            }
        }

        public static void ServerSpawnMinions(
            ICharacter characterBoss,
            Vector2D characterBossCenterPosition,
            IProtoCharacterMob protoMinion,
            List<ICharacter> minionsList,
            double spawnCheckDistanceSqr,
            ServerBossDamageTracker bossDamageTracker,
            double minionsPerPlayer,
            int minionsTotalMin,
            int minionsTotalMax,
            int? minionsSpawnPerIterationLimit,
            double baseMinionsNumber,
            double spawnNoObstaclesCircleRadius,
            double spawnDistanceMin,
            double spawnDistanceMax)
        {
            // calculate how many minions required
            var minionsRequired = baseMinionsNumber;
            using var tempListCharacters = Api.Shared.GetTempList<ICharacter>();
            Api.Server.World.GetScopedByPlayers(characterBoss, tempListCharacters);

            foreach (var playerCharacter in tempListCharacters.AsList())
            {
                if (playerCharacter.ProtoGameObject is PlayerCharacterSpectator)
                {
                    continue;
                }

                if (playerCharacter.Position.DistanceSquaredTo(characterBossCenterPosition)
                    <= spawnCheckDistanceSqr)
                {
                    minionsRequired += minionsPerPlayer;
                }
            }

            minionsRequired = MathHelper.Clamp(minionsRequired,
                                               minionsTotalMin,
                                               minionsTotalMax);

            if (minionsSpawnPerIterationLimit.HasValue)
            {
                minionsRequired = Math.Min(minionsRequired, minionsSpawnPerIterationLimit.Value);
            }

            ServerProcessMinions(characterBoss,
                                 protoMinion,
                                 minionsList,
                                 out var spawnedMinionsCount,
                                 despawnDistanceSqr: spawnCheckDistanceSqr);

            //Logger.Dev($"Minions required: {minionsRequired} minions have: {minionsHave}");
            minionsRequired -= spawnedMinionsCount;
            if (minionsRequired <= 0)
            {
                return;
            }

            // spawn minions
            var attemptsRemains = 300;
            var physicsSpace = characterBoss.PhysicsBody.PhysicsSpace;

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
                var spawnPosition = new Vector2D(
                    characterBossCenterPosition.X + spawnDistance * Math.Cos(angle),
                    characterBossCenterPosition.Y + spawnDistance * Math.Sin(angle));
                if (ServerTrySpawnMinion(spawnPosition) is { } spawnedMinion)
                {
                    // spawned successfully!
                    minionsRequired--;
                    minionsList.Add(spawnedMinion);
                }
            }

            ICharacter ServerTrySpawnMinion(Vector2D spawnPosition)
            {
                if (physicsSpace.TestCircle(spawnPosition,
                                            spawnNoObstaclesCircleRadius,
                                            CollisionGroups.Default,
                                            sendDebugEvent: true).EnumerateAndDispose().Any())
                {
                    // obstacles nearby
                    return null;
                }

                var spawnedCharacter = Api.Server.Characters.SpawnCharacter(protoMinion, spawnPosition);
                if (spawnedCharacter is null)
                {
                    return null;
                }

                // write this boss' damage tracker into the minion character
                // so any damage dealt to it will be counted in the winners ranking
                var privateState = spawnedCharacter.GetPrivateState<ICharacterPrivateStateWithBossDamageTracker>();
                privateState.DamageTracker = bossDamageTracker;

                // start spawn animation
                if (spawnedCharacter.ProtoGameObject is IProtoCharacterMob protoCharacterMob)
                {
                    protoCharacterMob.ServerSetSpawnState(spawnedCharacter,
                                                          MobSpawnState.Spawning);
                }

                return spawnedCharacter;
            }
        }
    }
}