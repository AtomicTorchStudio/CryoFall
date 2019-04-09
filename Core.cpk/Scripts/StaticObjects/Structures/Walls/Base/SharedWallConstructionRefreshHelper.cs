namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Walls
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.ConstructionSite;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Doors;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;

    internal static class SharedWallConstructionRefreshHelper
    {
        private static bool isRefreshing;

        /// <summary>
        /// Find neighbor objects of type wall or construction site (for wall) and refresh their renderers
        /// according to their neighbors.
        /// </summary>
        public static void SharedRefreshNeighborObjects(Tile tile, bool isDestroy)
        {
            if (isRefreshing)
            {
                // don't allow recursive refreshing
                return;
            }

            try
            {
                isRefreshing = true;

                foreach (var neighborTile in tile.EightNeighborTiles)
                {
                    foreach (var obj in neighborTile.StaticObjects)
                    {
                        var protoWorldObject = obj.ProtoWorldObject;
                        switch (protoWorldObject)
                        {
                            case IProtoObjectWall _:
                            case IProtoObjectDoor _:
                            case ObjectWallDestroyed _:
                                RefreshWorldObject();
                                break;

                            case ProtoObjectConstructionSite _:
                            {
                                var constructionProto = ProtoObjectConstructionSite.GetPublicState(obj)
                                                                                   .ConstructionProto;
                                switch (constructionProto)
                                {
                                    case IProtoObjectWall _:
                                    case IProtoObjectDoor _:
                                        RefreshWorldObject();
                                        break;
                                }

                                break;
                            }
                        }

                        // helper local function
                        void RefreshWorldObject()
                        {
                            if (Api.IsClient)
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
            finally
            {
                isRefreshing = false;
            }
        }

        public static void SharedRefreshNeighborObjects(IStaticWorldObject staticWorldObject, bool isDestroy)
        {
            var protoWorldObject = staticWorldObject.ProtoStaticWorldObject;
            switch (protoWorldObject)
            {
                case IProtoObjectWall _:
                case IProtoObjectDoor _:
                case ObjectWallDestroyed _:
                    SharedRefreshNeighborObjects(staticWorldObject.OccupiedTile, isDestroy);
                    break;

                case ProtoObjectConstructionSite _:
                {
                    var constructionProto = ProtoObjectConstructionSite.GetPublicState(staticWorldObject)
                                                                       .ConstructionProto;
                    switch (constructionProto)
                    {
                        case IProtoObjectWall _:
                        case IProtoObjectDoor _:
                            SharedRefreshNeighborObjects(staticWorldObject.OccupiedTile, isDestroy);
                            break;
                    }

                    break;
                }
            }
        }
    }
}