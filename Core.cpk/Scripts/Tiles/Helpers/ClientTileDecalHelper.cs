namespace AtomicTorch.CBND.CoreMod.Tiles
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.ClientOptions.Video;
    using AtomicTorch.CBND.CoreMod.Noise;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.GameEngine.Common.DataStructures;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ClientTileDecalHelper
    {
        private static readonly IClientStorage SessionStorage;

        private static bool isDecalsEnabled;

        static ClientTileDecalHelper()
        {
            SessionStorage = Api.Client.Storage.GetSessionStorage(
                $"{nameof(ClientTileDecalHelper)}.{nameof(IsDecalsEnabled)}");
            if (!SessionStorage.TryLoad(out isDecalsEnabled))
            {
                isDecalsEnabled = true;
            }
        }

        public static event Action IsDecalsEnabledChanged;

        public static bool IsDecalsEnabled
        {
            get => isDecalsEnabled;
            set
            {
                if (isDecalsEnabled == value)
                {
                    return;
                }

                isDecalsEnabled = value;
                SessionStorage.Save(isDecalsEnabled);
                Api.Client.Rendering.RefreshAllTileRenderers();
                IsDecalsEnabledChanged?.Invoke();
            }
        }

        [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
        public static void CreateDecalRenderers(
            Tile tile,
            ReadOnlyListWrapper<ProtoTileDecal> decals,
            IClientSceneObject sceneObject)
        {
            if (!isDecalsEnabled)
            {
                return;
            }

            var noiseSelectorRangeMultiplier = VideoOptionTerrainDetails.TerrainDecalNoiseSelectorRangeMultiplier;
            if (noiseSelectorRangeMultiplier == 0)
            {
                // decals disabled in options
                return;
            }

            var position = tile.Position;
            foreach (var decal in decals)
            {
                if (IsValidDecal(decal, tile, position, noiseSelectorRangeMultiplier))
                {
                    // create decal renderer
                    sceneObject.AddComponent<ClientComponentTerrainDecalRenderer>()
                               .Setup(decal, position);
                }
            }
        }

        public static void RefreshDecalRenderers(
            Tile tile,
            IClientSceneObject sceneObject)
        {
            var noiseSelectorRangeMultiplier = VideoOptionTerrainDetails.TerrainDecalNoiseSelectorRangeMultiplier;
            if (noiseSelectorRangeMultiplier == 0)
            {
                // decals disabled in options
                return;
            }

            var decalComponents = sceneObject.FindComponents<ClientComponentTerrainDecalRenderer>();
            foreach (var componentDecal in decalComponents)
            {
                if (!IsValidDecal(componentDecal.Decal,
                                  tile,
                                  tile.Position,
                                  noiseSelectorRangeMultiplier))
                {
                    // disable invalid decal
                    // (please note we never enable the disabled decal object as it will look wrong to the players)
                    componentDecal.IsEnabled = false;
                }
            }
        }

        private static bool EnsureAllTilesMatchDecal(
            Tile tile,
            ProtoTileDecal decal,
            double noiseSelectorRangeMultiplier)
        {
            var requiredProtoTile = tile.ProtoTile;
            var requiredHeight = tile.Height;
            var decalWorldSize = decal.Size;
            var hidingSetting = decal.HidingSetting;
            var requiredGroundTextures = decal.RequiredGroundTextures;

            for (var x = 0; x < decalWorldSize.X; x++)
            for (var y = 0; y < decalWorldSize.Y; y++)
            {
                var checkNoiseMatch = decal.RequiresCompleteNoiseSelectorCoverage
                                      || x == 0 && y == 0;

                var tileToCheck = tile.GetNeighborTile(x, y);
                var tileToCheckProto = tileToCheck.ProtoTile;
                if (tileToCheckProto != requiredProtoTile)
                {
                    return false;
                }

                if (!IsMatch(decal.NoiseSelector,
                             noiseSelectorRangeMultiplier,
                             tileToCheck,
                             requiredHeight,
                             checkNoiseMatch,
                             hidingSetting))
                {
                    return false;
                }

                if (requiredGroundTextures is not null)
                {
                    // check constraint for the ground texture
                    if (!IsTileMatchGroundTextures(tileToCheck, requiredGroundTextures)
                        || (decal.RequiresCompleteProtoTileCoverage
                            && tileToCheck.EightNeighborTiles.Any(
                                n => !IsTileMatchGroundTextures(n, requiredGroundTextures))))
                    {
                        return false;
                    }
                }
                else if (decal.RequiresCompleteProtoTileCoverage)
                {
                    // check neighbor tiles to match the desired proto tile
                    if (tileToCheck.EightNeighborTiles.Any(n => n.ProtoTile != requiredProtoTile))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private static bool IsMatch(
            INoiseSelector noiseSelector,
            double noiseSelectorRangeMultiplier,
            Tile tile,
            byte requiredHeight,
            bool checkNoiseMatch,
            DecalHidingSetting hidingSetting)
        {
            if (tile.Height != requiredHeight)
            {
                // different height
                return false;
            }

            if (tile.IsCliffOrSlope)
            {
                // don't place decals on cliffs and slopes
                return false;
            }

            if (checkNoiseMatch
                && !noiseSelector.IsMatch(tile.Position.X, tile.Position.Y, noiseSelectorRangeMultiplier))
            {
                // the tile is not in the noise map
                return false;
            }

            switch (hidingSetting)
            {
                case DecalHidingSetting.Never:
                    // always show decal
                    return true;

                case DecalHidingSetting.AnyObject:
                    // don't show decals if there is any object
                    foreach (var staticWorldObject in tile.StaticObjects)
                    {
                        var kind = staticWorldObject.ProtoStaticWorldObject.Kind;
                        if (kind != StaticObjectKind.SpecialAllowDecals)
                        {
                            return false;
                        }
                    }

                    return true;

                case DecalHidingSetting.StructureOrFloorObject:
                    foreach (var staticWorldObject in tile.StaticObjects)
                    {
                        var kind = staticWorldObject.ProtoStaticWorldObject.Kind;
                        switch (kind)
                        {
                            case StaticObjectKind.Platform:
                            case StaticObjectKind.Structure:
                            case StaticObjectKind.Floor:
                            case StaticObjectKind.FloorDecal:
                                // don't show decals under structures and floors
                                return false;
                        }
                    }

                    return true;

                default:
                    throw new ArgumentOutOfRangeException(nameof(hidingSetting));
            }
        }

        private static bool IsTileMatchGroundTextures(
            Tile tile,
            IReadOnlyList<ProtoTileGroundTexture> requiredGroundTextures)
        {
            var proto = (ProtoTile)tile.ProtoTile;
            var tileGroundTexture = proto.GetGroundTexture(tile.Position, tile.Height);
            if (!requiredGroundTextures.Contains(tileGroundTexture))
            {
                // the decal cannot be added to this ground texture
                return false;
            }

            return true;
        }

        private static bool IsValidDecal(
            ProtoTileDecal decal,
            Tile tile,
            Vector2Ushort position,
            double noiseSelectorRangeMultiplier)
        {
            var interval = decal.Interval;
            var offset = decal.Offset;

            if ((position.X + offset.X) % interval.X != 0
                || (position.Y + offset.Y) % interval.Y != 0)
            {
                return false;
            }

            // ensure that all tiles which will be covered by this decal are matching the conditions
            return EnsureAllTilesMatchDecal(tile, decal, noiseSelectorRangeMultiplier);
        }
    }
}