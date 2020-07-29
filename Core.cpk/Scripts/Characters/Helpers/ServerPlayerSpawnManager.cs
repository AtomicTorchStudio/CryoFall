namespace AtomicTorch.CBND.CoreMod.Characters
{
    using System;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Items.Tools;
    using AtomicTorch.CBND.CoreMod.Items.Tools.Lights;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
    using AtomicTorch.CBND.CoreMod.Zones;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Zones;
    using AtomicTorch.CBND.GameApi.Extensions;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesServer;
    using AtomicTorch.GameEngine.Common.Primitives;

    /// <summary>
    /// This class manages the players spawn location.
    /// </summary>
    public static class ServerPlayerSpawnManager
    {
        private const double MaxDistanceWhenRespawn = 200;

        private const double MaxDistanceWhenRespawnSqr = MaxDistanceWhenRespawn * MaxDistanceWhenRespawn;

        private const double MinDistanceWhenRespawn = 100;

        private const double MinDistanceWhenRespawnSqr = MinDistanceWhenRespawn * MinDistanceWhenRespawn;

        private const int SpawnInZoneAttempts = 2000;

        private static IProtoZone protoSpawnZone;

        private static IWorldServerService worldService;

        public static void PlacePlayer(ICharacter character, bool isRespawn)
        {
            // please ensure that we don't touch any private properties as they're not initialized at the first spawn request
            // (player placed in the world and only then it's initialized)
            var random = Api.Random;
            Vector2Ushort? spawnPosition = null;

            var spawnZone = protoSpawnZone.ServerZoneInstance;
            if (!spawnZone.IsEmpty)
            {
                // TODO: this could be slow and might require distributing across the multiple frames
                if (isRespawn)
                {
                    spawnPosition = TryFindZoneSpawnPosition(character,
                                                             spawnZone,
                                                             random,
                                                             isRespawn: true);
                }

                spawnPosition ??= TryFindZoneSpawnPosition(character,
                                                           spawnZone,
                                                           random,
                                                           isRespawn: false);
            }

            if (!spawnPosition.HasValue)
            {
                // fallback - spawn in the center of the world
                var worldBounds = worldService.WorldBounds;
                var offset = worldBounds.Offset;
                var size = worldBounds.Size;
                spawnPosition = new Vector2Ushort((ushort)(offset.X + size.X / 2),
                                                  (ushort)(offset.Y + size.Y / 2));
            }

            worldService.SetPosition(character, spawnPosition.Value.ToVector2D());
        }

        public static void ServerAddTorchItemIfNoItems(ICharacter character)
        {
            if (!character.IsInitialized)
            {
                return;
            }

            // spawn torch item for players convenience
            // (yeah, it's possible to farm torches this way, but they are cheap so no good reason for farming them)
            var hotbar = character.SharedGetPlayerContainerHotbar();
            var inventory = character.SharedGetPlayerContainerInventory();

            if (hotbar?.OccupiedSlotsCount == 0
                && inventory?.OccupiedSlotsCount == 0)
            {
                Api.Server.Items.CreateItem<ItemTorch>(hotbar, slotId: 0);
            }
        }

        public static void Setup(IServerConfiguration serverConfiguration)
        {
            worldService = Api.Server.World;
            protoSpawnZone = Api.GetProtoEntity<ZoneSpecialPlayerSpawn>();
            serverConfiguration.SetupPlayerCharacterSpawnCallbackMethod(
                character => SpawnPlayer(character, isRespawn: false));
        }

        public static void SpawnPlayer(ICharacter character, bool isRespawn)
        {
            PlacePlayer(character, isRespawn);

            ServerAddTorchItemIfNoItems(character);
        }

        private static Vector2Ushort? TryFindZoneSpawnPosition(
            ICharacter character,
            IServerZone spawnZone,
            Random random,
            bool isRespawn)
        {
            var characterDeathPosition = Vector2Ushort.Zero;
            if (isRespawn && character.ProtoCharacter is PlayerCharacter)
            {
                var privateState = PlayerCharacter.GetPrivateState(character);
                if (!privateState.LastDeathTime.HasValue)
                {
                    return null;
                }

                characterDeathPosition = privateState.LastDeathPosition;
            }

            var restrictedZone = ZoneSpecialConstructionRestricted.Instance.ServerZoneInstance;

            for (var attempt = 0; attempt < SpawnInZoneAttempts; attempt++)
            {
                var randomPosition = spawnZone.GetRandomPosition(random);

                if (isRespawn)
                {
                    var sqrDistance = randomPosition.TileSqrDistanceTo(characterDeathPosition);
                    if (sqrDistance > MaxDistanceWhenRespawnSqr
                        || sqrDistance < MinDistanceWhenRespawnSqr)
                    {
                        // too close or too far for the respawn
                        continue;
                    }
                }

                if (restrictedZone.IsContainsPosition(randomPosition))
                {
                    // the position is inside the restricted zone
                    continue;
                }

                if (!LandClaimSystem.SharedIsPositionInsideOwnedOrFreeArea(randomPosition, character))
                {
                    // the land is claimed by another player
                    continue;
                }

                if (ServerCharacterSpawnHelper.IsPositionValidForCharacterSpawn(randomPosition.ToVector2D(),
                                                                                isPlayer: true))
                {
                    // valid position found
                    return randomPosition;
                }
            }

            return null;
        }
    }
}