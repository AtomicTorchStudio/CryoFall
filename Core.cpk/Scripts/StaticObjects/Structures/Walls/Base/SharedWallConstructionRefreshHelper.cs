namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Walls
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.ConstructionSite;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Doors;
    using AtomicTorch.CBND.CoreMod.Triggers;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Primitives;
    using static GameApi.Scripting.Api;

    /// <summary>
    /// Find neighbor walls or construction sites (for walls)
    /// and re-initialize them
    /// (so they take into account new/updated neighbor for their rendering and physics body).
    /// </summary>
    public static class SharedWallConstructionRefreshHelper
    {
        private static readonly HashSet<Tile> Queue = new(TileComparer.Instance);

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

            var protoWorldObject = staticWorldObject.ProtoStaticWorldObject;
            switch (protoWorldObject)
            {
                case IProtoObjectWall:
                case IProtoObjectDoor:
                case ObjectWallDestroyed:
                    SharedRefreshNeighborObjects(staticWorldObject.OccupiedTile, isDestroy);
                    break;

                case ProtoObjectConstructionSite:
                {
                    var constructionProto = ProtoObjectConstructionSite.GetPublicState(staticWorldObject)
                                                                       .ConstructionProto;
                    switch (constructionProto)
                    {
                        case IProtoObjectWall:
                        case IProtoObjectDoor:
                            SharedRefreshNeighborObjects(staticWorldObject.OccupiedTile, isDestroy);
                            break;
                    }

                    break;
                }
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

                if (IsClient && Client.World.WorldBounds.Size == Vector2Ushort.Zero
                    || IsServer && Server.World.WorldBounds.Size == Vector2Ushort.Zero)
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
                        Logger.Exception(ex);
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
                        var protoWorldObject = obj.ProtoWorldObject;
                        switch (protoWorldObject)
                        {
                            case IProtoObjectWall:
                            case IProtoObjectDoor:
                            case ObjectWallDestroyed:
                                RefreshWorldObject();
                                break;

                            case ProtoObjectConstructionSite:
                            {
                                var constructionProto = ProtoObjectConstructionSite.GetPublicState(obj)
                                    .ConstructionProto;
                                switch (constructionProto)
                                {
                                    case IProtoObjectWall:
                                    case IProtoObjectDoor:
                                        RefreshWorldObject();
                                        break;
                                }

                                break;
                            }
                        }

                        // helper local function
                        void RefreshWorldObject()
                        {
                            if (IsClient)
                            {
                                obj.ClientInitialize();
                            }
                            else
                            {
                                obj.ServerInitialize();
                            }
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
            public static readonly TileComparer Instance = new();

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