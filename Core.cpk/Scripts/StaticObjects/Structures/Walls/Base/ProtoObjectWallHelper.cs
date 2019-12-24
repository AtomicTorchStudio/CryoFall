namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Walls
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Helpers.Client.Walls;
    using AtomicTorch.CBND.CoreMod.Helpers.Primitives;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.ConstructionSite;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Doors;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public static class ProtoObjectWallHelper
    {
        public static void ClientRefreshRenderer(IStaticWorldObject worldObject)
        {
            var isDestroyedWall = false;
            if (!(worldObject.ProtoGameObject is IProtoObjectWall protoWall))
            {
                if (!(worldObject.ProtoGameObject is ObjectWallDestroyed))
                {
                    return;
                }

                isDestroyedWall = true;
                protoWall = ObjectWallDestroyed.GetPublicState(worldObject)
                                               .OriginalProtoObjectWall;
            }

            var result = SharedGetAtlasTextureChunkPosition(worldObject.OccupiedTile,
                                                            protoWall,
                                                            isConsiderDestroyed: !isDestroyedWall,
                                                            isConsiderConstructionSites: !isDestroyedWall);
            try
            {
                var textureAtlas = ClientGetTextureAtlas(worldObject);
                var primaryTextureResource = textureAtlas.Chunk(
                    (byte)result.Primary.AtlasChunkPosition.X,
                    (byte)result.Primary.AtlasChunkPosition.Y);

                var clientState = worldObject.GetClientState<ObjectWallClientState>();
                clientState.Renderer.TextureResource = primaryTextureResource;
                var destroyedWallYOffset = isDestroyedWall
                                               ? result.Primary.DrawOffsetDestroyed
                                               : result.Primary.DrawOffsetNormal;

                clientState.Renderer.DrawOrderOffsetY = isDestroyedWall
                                                            ? destroyedWallYOffset
                                                            : result.Primary.DrawOffsetNormal;

                // destroy previous overlay renderers
                var overlayRenderers = clientState.RenderersObjectOverlay;
                if (overlayRenderers?.Count > 0)
                {
                    foreach (var renderer in overlayRenderers)
                    {
                        renderer.Destroy();
                    }

                    overlayRenderers.Clear();
                }

                var overlayChunkPreset = result.Overlay;
                if (overlayChunkPreset == null
                    || overlayChunkPreset.Count == 0)
                {
                    // no overlay renderers needed
                    return;
                }

                // add overlay renderers
                foreach (var preset in overlayChunkPreset)
                {
                    var overlayRenderer = Api.Client.Rendering.CreateSpriteRenderer(
                        worldObject,
                        textureAtlas.Chunk((byte)preset.AtlasChunkPosition.X,
                                           (byte)preset.AtlasChunkPosition.Y));

                    if (overlayRenderers == null)
                    {
                        overlayRenderers = new List<IComponentSpriteRenderer>();
                        clientState.RenderersObjectOverlay = overlayRenderers;
                    }

                    overlayRenderers.Add(overlayRenderer);
                    overlayRenderer.DrawOrderOffsetY = isDestroyedWall
                                                           ? destroyedWallYOffset
                                                           : preset.DrawOffsetNormal;
                }
            }
            finally
            {
                result.Dispose();
            }
        }

        public static void ClientSetupBlueprint(IProtoObjectWall protoWall, Tile tile, IClientBlueprint blueprint)
        {
            var textureAtlas = ClientGetTextureAtlas(protoWall);
            var result = SharedGetAtlasTextureChunkPosition(tile,
                                                            protoWall,
                                                            isConsiderDestroyed: false,
                                                            isConsiderConstructionSites: true);

            blueprint.SpriteRenderer.TextureResource = textureAtlas.Chunk((byte)result.Primary.AtlasChunkPosition.X,
                                                                          (byte)result.Primary.AtlasChunkPosition.Y);
            result.Dispose();
        }

        public static void SharedCalculateNeighborsPattern(
            Tile tile,
            IProtoObjectWall protoWall,
            out NeighborsPattern sameTypeNeighbors,
            out NeighborsPattern compatibleTypeNeighbors,
            bool isConsiderDestroyed,
            bool isConsiderConstructionSites)
        {
            sameTypeNeighbors = NeighborsPattern.None;
            compatibleTypeNeighbors = NeighborsPattern.None;

            var eightNeighborTiles = tile.EightNeighborTiles;
            var tileIndex = -1;
            foreach (var neighborTile in eightNeighborTiles)
            {
                tileIndex++;
                if (SharedIsSameWallType(protoWall,
                                         neighborTile,
                                         isConsiderDestroyed))
                {
                    sameTypeNeighbors |= NeighborsPatternDirections.NeighborDirectionSameType[tileIndex];
                }
                else if (SharedIsCompatibleWallType(neighborTile,
                                                    isConsiderDestroyed,
                                                    isConsiderConstructionSites,
                                                    isHorizontal: tileIndex == 3 || tileIndex == 4))
                {
                    sameTypeNeighbors |= NeighborsPatternDirections.NeighborDirectionSameType[tileIndex];
                    compatibleTypeNeighbors |= NeighborsPatternDirections.NeighborDirectionSameType[tileIndex];
                }
            }
        }

        public static bool SharedIsDestroyedWallRequired(Tile tile, IProtoObjectWall protoWall)
        {
            SharedCalculateNeighborsPattern(tile,
                                            protoWall,
                                            out var sameTypeNeighbors,
                                            out var compatibleTypeNeighbors,
                                            isConsiderDestroyed: false,
                                            isConsiderConstructionSites: false);

            // remove corner cases (literally!)
            const NeighborsPattern cornerCases = ~(NeighborsPattern.TopLeft
                                                   | NeighborsPattern.TopRight
                                                   | NeighborsPattern.BottomLeft
                                                   | NeighborsPattern.BottomRight);

            sameTypeNeighbors &= cornerCases;
            compatibleTypeNeighbors &= cornerCases;

            return sameTypeNeighbors != NeighborsPattern.None
                   || compatibleTypeNeighbors != NeighborsPattern.None;
        }

        private static ITextureAtlasResource ClientGetTextureAtlas(IStaticWorldObject worldObject)
        {
            if (worldObject.ProtoGameObject is ObjectWallDestroyed)
            {
                var protoWall = ObjectWallDestroyed.GetPublicState(worldObject)
                                                   .OriginalProtoObjectWall;

                if (protoWall == null)
                {
                    Api.Logger.Important("Incorrect destroyed wall - no protoWall defined: " + worldObject);
                    protoWall = Api.GetProtoEntity<ObjectWallWood>();
                }

                return protoWall.TextureAtlasDestroyed;
            }

            return ClientGetTextureAtlas(worldObject.ProtoStaticWorldObject);
        }

        private static ITextureAtlasResource ClientGetTextureAtlas(IProtoStaticWorldObject protoObject)
        {
            if (protoObject is IProtoObjectWall protoObjectWall)
            {
                return protoObjectWall.TextureAtlasPrimary;
            }

            throw new Exception("Incompatible object prototype: " + protoObject);
        }

        private static WallTextureChunkSelector.WallChunkWithOverlays SharedGetAtlasTextureChunkPosition(
            Tile tile,
            IProtoObjectWall protoWall,
            bool isConsiderDestroyed,
            bool isConsiderConstructionSites)
        {
            SharedCalculateNeighborsPattern(tile,
                                            protoWall,
                                            out var sameTypeNeighbors,
                                            out var compatibleTypeNeighbors,
                                            isConsiderDestroyed,
                                            isConsiderConstructionSites);
            return WallTextureChunkSelector.GetRegion(sameTypeNeighbors, compatibleTypeNeighbors);
        }

        private static bool SharedIsCompatibleDoor(
            IStaticWorldObject worldObject,
            Tile tile,
            bool isConsiderConstructionSites,
            bool isHorizontal)
        {
            if (worldObject.ProtoWorldObject is IProtoObjectDoor
                && isHorizontal == worldObject.GetPublicState<ObjectDoorPublicState>().IsHorizontalDoor)
            {
                return true;
            }

            if (isConsiderConstructionSites
                && ProtoObjectConstructionSite.SharedIsConstructionOf(worldObject, typeof(IProtoObjectDoor))
                && isHorizontal == DoorHelper.IsHorizontalDoorNeeded(tile, checkExistingDoor: true))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Checks if the wall in other tile is compatible with this type.
        /// </summary>
        private static bool SharedIsCompatibleWallType(
            Tile tile,
            bool recognizeDestroyedWalls,
            bool recognizeConstructionSites,
            bool isHorizontal)
        {
            foreach (var worldObject in tile.StaticObjects)
            {
                if (worldObject.IsDestroyed)
                {
                    continue;
                }

                if (worldObject.ProtoWorldObject is IProtoObjectWall
                    || recognizeDestroyedWalls && worldObject.ProtoWorldObject is ObjectWallDestroyed)
                {
                    return true;
                }

                if (recognizeConstructionSites
                    && ProtoObjectConstructionSite.SharedIsConstructionOf(worldObject, typeof(IProtoObjectWall)))
                {
                    return true;
                }

                if (isHorizontal)
                {
                    if (SharedIsCompatibleDoor(worldObject, tile, recognizeConstructionSites, isHorizontal: true))
                    {
                        return true;
                    }
                }

                // Disabled - no need for this as we cover that empty space with the hitboxes
                // and the space is too small to pass through so absence of a physical collider is fine.
                //else if (Api.IsServer)
                //{
                //    // Only on the server side we do the check for the vertical compatible door.
                //    // That's because we don't want to draw the "compatible wall adapters" on the client side.
                //    // It also means that physics is a bit different between the client and the server
                //    // in this aspect but it's barely noticeable.
                //    if (SharedIsCompatibleDoor(worldObject, tile, recognizeConstructionSites, isHorizontal: false))
                //    {
                //        return true;
                //    }
                //}
            }

            return false;
        }

        /// <summary>
        /// Checks if the wall in other tile is compatible with this type.
        /// It checks if the wall in other tile has exactly the same type as the wall in this tile.
        /// </summary>
        private static bool SharedIsSameWallType(IProtoObjectWall protoWall, Tile tile, bool isConsiderDestroyed)
        {
            foreach (var o in tile.StaticObjects)
            {
                if (o.IsDestroyed)
                {
                    continue;
                }

                if (o.ProtoWorldObject == protoWall)
                {
                    return true;
                }

                if (isConsiderDestroyed
                    && o.ProtoWorldObject is ObjectWallDestroyed
                    && ObjectWallDestroyed.GetPublicState(o).OriginalProtoObjectWall == protoWall)
                {
                    // destroyed wall of the same type
                    return true;
                }
            }

            return false;
        }
    }
}