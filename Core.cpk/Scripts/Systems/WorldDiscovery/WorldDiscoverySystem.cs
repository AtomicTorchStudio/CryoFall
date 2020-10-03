namespace AtomicTorch.CBND.CoreMod.Systems.WorldDiscovery
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Bootstrappers;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Map.Data;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.GameEngine.Common.Extensions;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class WorldDiscoverySystem : ProtoSystem<WorldDiscoverySystem>
    {
        private const string DatabaseKeyPlayerDiscoveredChunkTiles = "PlayerDiscoveredChunkTiles";

        private int clientLastRequestIndex;

        private Dictionary<ICharacter, HashSet<Vector2Ushort>> serverPlayerDiscoveredChunkTiles;

        public override string Name => "World discovery system";

        /// <summary>
        /// Returns true if the location is used for the player corpses.
        /// </summary>
        public static bool IsCorpseIsland(Vector2Ushort chunkPosition, BoundsUshort worldBounds)
        {
            if (Api.IsEditor)
            {
                return false;
            }

            var chunkPositionX = chunkPosition.X - worldBounds.Offset.X;
            var chunkPositionY = chunkPosition.Y - worldBounds.Offset.Y;
            // bottom right corner is the corpse island
            return chunkPositionX > worldBounds.Size.X - ScriptingConstants.WorldChunkSize * 6
                   && chunkPositionY < ScriptingConstants.WorldChunkSize * 6;
        }

        public async void ClientInitializeDiscoverySystem()
        {
            if (Client.Characters.IsCurrentPlayerCharacterSpectator)
            {
                return;
            }

            var clientRequestIndex = ++this.clientLastRequestIndex;

            Client.World.ClearDiscoveredWorldChunks();

            // request discovered tiles
            var resultDiscoveredTiles = await this.CallServer(_ => _.ServerRemote_GetDiscoveredTiles());
            if (clientRequestIndex != this.clientLastRequestIndex)
            {
                return;
            }

            if (resultDiscoveredTiles is not null)
            {
                //Logger.WriteDev("Client received from server all the discovered tiles for current character: "
                //                + resultDiscoveredTiles.GetJoinedString(Environment.NewLine));
                Client.World.AddDiscoveredWorldChunks(
                    WorldMapController.OrderChunksByProximityToPlayer(resultDiscoveredTiles));
            }
        }

        public void ServerDiscoverWorldChunks(ICharacter character, IReadOnlyList<Vector2Ushort> chunkTilePositions)
        {
            if (!this.serverPlayerDiscoveredChunkTiles.TryGetValue(character, out var hashSet))
            {
                hashSet = new HashSet<Vector2Ushort>();
                this.serverPlayerDiscoveredChunkTiles[character] = hashSet;
            }

            var world = Api.Server.World;
            var worldBounds = world.WorldBounds;

            List<Vector2Ushort> tempList = null;
            foreach (var chunkPosition in chunkTilePositions)
            {
                if (IsCorpseIsland(chunkPosition, worldBounds))
                {
                    // undiscoverable area
                    continue;
                }

                if (!hashSet.Add(chunkPosition))
                {
                    continue;
                }

                // new chunk discovered!
                tempList ??= new List<Vector2Ushort>();

                tempList.Add(chunkPosition);
            }

            if (tempList?.Count > 0
                && character.ServerIsOnline)
            {
                // send to client (it will automatically request the according tiles chunk if doesn't have it in the cache)
                this.CallClient(character, _ => _.ClientRemote_ChunksDiscovered(tempList));
            }
        }

        protected override void PrepareSystem()
        {
            if (IsClient)
            {
                BootstrapperClientGame.InitCallback += _ => this.ClientInitializeDiscoverySystem();
                return;
            }

            Server.World.ValidatePlayerDiscoveredWorldChunk = this.ServerValidatePlayerDiscoveredWorldChunk;

            // below is the Server-side code only
            Server.World.PlayerCharacterObservedChunkTiles += this.ServerPlayerCharacterObservedTilesHandler;
            Server.World.WorldBoundsChanged += this.ServerWorldBoundsChangedHandler;

            this.ServerLoadSystem();
        }

        [RemoteCallSettings(DeliveryMode.ReliableUnordered)]
        private void ClientRemote_ChunksDiscovered(List<Vector2Ushort> discoveredChunks)
        {
            //Logger.WriteDev("Client received from server the new discovered tiles for current character: "
            //                + discoveredChunks.GetJoinedString(Environment.NewLine));

            Client.World.AddDiscoveredWorldChunks(
                WorldMapController.OrderChunksByProximityToPlayer(discoveredChunks));
        }

        private void ServerLoadSystem()
        {
            if (Api.Server.Database.TryGet(
                nameof(WorldDiscoverySystem),
                DatabaseKeyPlayerDiscoveredChunkTiles,
                out this.serverPlayerDiscoveredChunkTiles))
            {
                Logger.Important(
                    "Observed chunk tiles system loaded - characters count: "
                    + Environment.NewLine
                    + this.serverPlayerDiscoveredChunkTiles.Count
                    /*this.serverPlayerDiscoveredChunkTiles.Select(c => $"{c.Key} - {c.Value.Count} chunks")
                              .GetJoinedString(Environment.NewLine)*/);
                return;
            }

            // the list is not stored, create a new one
            this.serverPlayerDiscoveredChunkTiles = new Dictionary<ICharacter, HashSet<Vector2Ushort>>();
            Api.Server.Database.Set(
                nameof(WorldDiscoverySystem),
                DatabaseKeyPlayerDiscoveredChunkTiles,
                this.serverPlayerDiscoveredChunkTiles);
        }

        private void ServerPlayerCharacterObservedTilesHandler(
            ICharacter character,
            IReadOnlyList<Vector2Ushort> chunkTilePositions)
        {
            //Logger.WriteDev($"Character observed tiles: {character} tiles: {Environment.NewLine}"
            //                + chunkTilePositions.GetJoinedString(Environment.NewLine));
            this.ServerDiscoverWorldChunks(character, chunkTilePositions);
        }

        [RemoteCallSettings(timeInterval: RemoteCallSettingsAttribute.MaxTimeInterval)]
        private HashSet<Vector2Ushort> ServerRemote_GetDiscoveredTiles()
        {
            return this.serverPlayerDiscoveredChunkTiles.Find(ServerRemoteContext.Character);
        }

        private bool ServerValidatePlayerDiscoveredWorldChunk(ICharacter character, Vector2Ushort chunkStartPosition)
        {
            return this.serverPlayerDiscoveredChunkTiles.TryGetValue(character, out var hashSet)
                   && hashSet.Contains(chunkStartPosition);
        }

        private void ServerWorldBoundsChangedHandler()
        {
            const string key = nameof(WorldDiscoverySystem);
            Server.Database.Remove(key, DatabaseKeyPlayerDiscoveredChunkTiles);
            this.ServerLoadSystem();
        }
    }
}