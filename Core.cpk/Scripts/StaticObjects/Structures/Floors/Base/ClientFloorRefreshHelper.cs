namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Floors
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Walls;
    using AtomicTorch.CBND.CoreMod.Triggers;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Primitives;

    /// <summary>
    /// Find neighbor floor and re-initialize them
    /// (so they take into account new/updated neighbor for their rendering and physics body).
    /// </summary>
    public static class ClientFloorRefreshHelper
    {
        private static readonly HashSet<Tile> Queue = new HashSet<Tile>(TileComparer.Instance);

        private static bool isProcessingQueueNow;

        public static void SharedRefreshNeighborObjects(Tile tile, bool isDestroy)
        {
            if (isProcessingQueueNow)
            {
                // don't allow recursive refreshing
                return;
            }

            Queue.Add(tile);
        }

        public static void SharedRefreshNeighborObjects(IStaticWorldObject staticWorldObject, bool isDestroy)
        {
            if (isProcessingQueueNow)
            {
                // don't allow recursive refreshing
                return;
            }

            if (staticWorldObject.ProtoStaticWorldObject is IProtoObjectFloor)
            {
                SharedRefreshNeighborObjects(staticWorldObject.OccupiedTile, isDestroy);
            }
        }

        private static void SharedProcessQueue()
        {
            if (Queue.Count == 0)
            {
                return;
            }

            try
            {
                isProcessingQueueNow = true;

                if (Api.IsClient && Api.Client.World.WorldBounds.Size == Vector2Ushort.Zero
                    || Api.IsServer && Api.Server.World.WorldBounds.Size == Vector2Ushort.Zero)
                {
                    return;
                }

                foreach (var tile in Queue)
                {
                    try
                    {
                        SharedProcessTileNeighbors(in tile);
                    }
                    catch (Exception ex)
                    {
                        Api.Logger.Exception(ex);
                    }
                }
            }
            finally
            {
                Queue.Clear();
                isProcessingQueueNow = false;
            }

            static void SharedProcessTileNeighbors(in Tile tile)
            {
                foreach (var neighborTile in tile.EightNeighborTiles)
                {
                    foreach (var obj in neighborTile.StaticObjects)
                    {
                        if (obj.ProtoWorldObject is IProtoObjectFloor protoFloor)
                        {
                            protoFloor.ClientRefreshRenderer(obj);
                        }
                    }
                }
            }
        }

        private class Bootstrapper : BaseBootstrapper
        {
            public override void ClientInitialize()
            {
                ClientUpdateHelper.UpdateCallback += SharedProcessQueue;
                Client.World.WorldBoundsChanged += () => Queue.Clear();
            }

            public override void ServerInitialize(IServerConfiguration serverConfiguration)
            {
                TriggerEveryFrame.ServerRegister(SharedProcessQueue,
                                                 nameof(SharedWallConstructionRefreshHelper) + ".Process");
                Server.World.WorldBoundsChanged += () => Queue.Clear();
            }
        }

        private class TileComparer : IEqualityComparer<Tile>
        {
            public static readonly TileComparer Instance = new TileComparer();

            public bool Equals(Tile a, Tile b)
            {
                return a.Position.Equals(b.Position);
            }

            public int GetHashCode(Tile tile)
            {
                return tile.Position.GetHashCode();
            }
        }
    }
}