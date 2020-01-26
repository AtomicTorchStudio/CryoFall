namespace AtomicTorch.CBND.CoreMod.Helpers.Server
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Helpers;
    using AtomicTorch.GameEngine.Common.Primitives;

    public static class ServerMobSpawnHelper
    {
        public static void ServerTrySpawnMobsCustom(
            IProtoCharacter protoMob,
            int countToSpawn,
            RectangleInt excludeBounds,
            int maxSpawnDistanceFromExcludeBounds,
            double noObstaclesCheckRadius,
            int maxAttempts)
        {
            using var tempList = Api.Shared.GetTempList<ICharacter>();
            ServerTrySpawnMobsCustom(protoMob,
                                     spawnedCollection: tempList.AsList(),
                                     countToSpawn,
                                     excludeBounds,
                                     maxSpawnDistanceFromExcludeBounds,
                                     noObstaclesCheckRadius,
                                     maxAttempts);
        }

        public static void ServerTrySpawnMobsCustom(
            IProtoCharacter protoMob,
            ICollection<ICharacter> spawnedCollection,
            int countToSpawn,
            RectangleInt excludeBounds,
            int maxSpawnDistanceFromExcludeBounds,
            double noObstaclesCheckRadius,
            int maxAttempts)
        {
            var spawnBounds = excludeBounds.Inflate(maxSpawnDistanceFromExcludeBounds,
                                                    maxSpawnDistanceFromExcludeBounds);
            var physicsSpace = Api.Server.World.GetPhysicsSpace();

            while (maxAttempts-- > 0)
            {
                var position = new Vector2D(spawnBounds.Left + RandomHelper.NextDouble() * spawnBounds.Width,
                                            spawnBounds.Bottom + RandomHelper.NextDouble() * spawnBounds.Height);
                if (IsTooClose(position))
                {
                    continue;
                }

                var character = ServerTrySpawnMob(position);
                if (character == null)
                {
                    // cannot spawn there
                    continue;
                }

                spawnedCollection.Add(character);

                countToSpawn--;
                if (countToSpawn == 0)
                {
                    return;
                }
            }

            bool IsTooClose(in Vector2D position)
                => position.X >= excludeBounds.X
                   && position.Y >= excludeBounds.Y
                   && position.X < excludeBounds.X + excludeBounds.Width
                   && position.Y < excludeBounds.Y + excludeBounds.Height;

            ICharacter ServerTrySpawnMob(Vector2D worldPosition)
            {
                foreach (var _ in physicsSpace.TestCircle(worldPosition,
                                                          radius: noObstaclesCheckRadius,
                                                          CollisionGroups.Default,
                                                          sendDebugEvent: false).EnumerateAndDispose())
                {
                    // position is not valid for spawning
                    return null;
                }

                if (!ServerCharacterSpawnHelper.IsValidSpawnTile(
                        Api.Server.World.GetTile(worldPosition.ToVector2Ushort()),
                        checkNeighborTiles: true))
                {
                    return null;
                }

                return Api.Server.Characters.SpawnCharacter(protoMob,
                                                            worldPosition);
            }
        }
    }
}