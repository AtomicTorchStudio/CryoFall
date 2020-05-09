namespace AtomicTorch.CBND.CoreMod.Tiles
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Windows.Controls;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Helpers.Primitives;
    using AtomicTorch.CBND.CoreMod.Noise;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Floors;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.Physics;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.DataStructures;
    using AtomicTorch.GameEngine.Common.Primitives;

    /// <summary>
    /// Implements ground tile with atlas (tile texture should be bigger than 1x1 world unit).
    /// </summary>
    public abstract class ProtoTile : ProtoEntity, IProtoTile
    {
        public static readonly TextureAtlasResource TileCliffHeightEdgeMaskAtlas = new TextureAtlasResource(
            "Terrain/Masks/MaskHeightEdge.png",
            columns: 4,
            rows: 1,
            isTransparent: true);

        internal static readonly TextureResource BlendMaskTextureGeneric2Smooth
            = new TextureResource("Terrain/Masks/MaskGeneric2");

        internal static readonly TextureResource BlendMaskTextureSprayRough
            = new TextureResource("Terrain/Masks/MaskSprayRough");

        internal static readonly TextureResource BlendMaskTextureSpraySmooth
            = new TextureResource("Terrain/Masks/MaskSpraySmooth");

        internal static readonly TextureResource BlendMaskTextureSprayStraightRough
            = new TextureResource("Terrain/Masks/MaskSprayStraightRough");

        internal static readonly TextureResource BlendMaskTextureSprayStraightSmooth
            = new TextureResource("Terrain/Masks/MaskSprayStraightSmooth");

        private static readonly ITextureResource WorldMapTextureCliff
            = SharedCreateMapTexture("Map/Cliff.png");

        private ReadOnlyListWrapper<ProtoTileDecal> decals;

        private ReadOnlyListWrapper<ProtoTileGroundTexture> groundTextures;

        protected ProtoTile()
        {
            var typeName = this.GetType().Name;
            var name = typeName;
            // ReSharper disable once CanExtractXamlLocalizableStringCSharp
            const string prefix = "Tile";
            if (!name.StartsWith(prefix, StringComparison.Ordinal))
            {
                throw new Exception($"Tile class name must start with \"{prefix}\": {typeName}");
            }

            this.ShortId = name.Substring(prefix.Length);
        }

        public TileAmbientSoundProvider AmbientSoundProvider { get; private set; }

        /// <param name="blendOrder">
        /// The blending order of the ground tile. If this is lower than the neighbor tile of another group tile proto, then
        /// the rendering for this tile will use blending with neighbor tile.
        /// </param>
        public abstract byte BlendOrder { get; }

        /// <summary>
        /// Characters can move faster or slower over this tile (applies only if there is no floor).
        /// </summary>
        public virtual double CharacterMoveSpeedMultiplier => 1.0;

        public virtual TextureAtlasResource CliffAtlas { get; }
            = new TextureAtlasResource(
                "Terrain/Cliffs/TerrainCliffs.png",
                columns: 6,
                rows: 4,
                isTransparent: true);

        /// <summary>
        /// Gets the texture resource used as icon in Editor toolbar.
        /// </summary>
        public ITextureResource EditorIconTexture { get; private set; }

        public abstract GroundSoundMaterial GroundSoundMaterial { get; }

        public virtual bool IsRestrictingConstruction => false;

        public abstract TileKind Kind { get; }

        public byte SessionIndex
        {
            get;
            // do not change this, required by the engine
#if GAME
			set;
#endif
        }

        public override string ShortId { get; }

        public ITextureResource WorldMapTexture { get; private set; }

        public abstract string WorldMapTexturePath { get; }

        protected virtual ITextureResource TextureWaterWorldPlaceholder { get; }

        public static double SharedGetTileMoveSpeedMultiplier(in Tile tile)
        {
            var moveSpeedMultiplier = ((ProtoTile)tile.ProtoTile).CharacterMoveSpeedMultiplier;

            foreach (var staticWorldObject in tile.StaticObjects)
            {
                if (staticWorldObject.ProtoGameObject is IProtoObjectMovementSurface protoObjectMovementSurface
                    && moveSpeedMultiplier < protoObjectMovementSurface.CharacterMoveSpeedMultiplier)
                {
                    moveSpeedMultiplier = protoObjectMovementSurface.CharacterMoveSpeedMultiplier;
                }
            }

            return moveSpeedMultiplier;
        }

        public virtual bool ClientIsBlendingWith(ProtoTile protoTile)
        {
            return true;
        }

        public void ClientRefreshDecals(Tile tile, IClientSceneObject sceneObject)
        {
            ClientTileDecalHelper.RefreshDecalRenderers(tile, sceneObject);
        }

        /// <summary>
        /// The return value will be cached during script prepare.
        /// </summary>
        /// <param name="tile">Tile.</param>
        /// <param name="sceneObject">Scene object (you can attach custom renderers to it).</param>
        /// <returns>
        /// Texture resource (could be chunk of a texture atlas). If null, no rendering will be created for this ground
        /// tile.
        /// </returns>
        public ITextureResource ClientSetupRendering(Tile tile, IClientSceneObject sceneObject)
        {
            try
            {
                return this.ClientSetupTileRendering(tile, sceneObject);
            }
            catch (Exception ex)
            {
                this.ReportException(ex);
                return null;
            }
        }

        public virtual void CreatePhysics(Tile tile, IPhysicsBody physicsBody)
        {
            const double tileColliderSlopeWidth = 0.25;
            const double tileColliderBottomWidth = 0.2;
            const double rangedHitboxOffset = 0.75;

            var tileKind = this.Kind;
            if (tileKind == TileKind.Placeholder)
            {
                return;
            }

            if (tileKind == TileKind.Water)
            {
                if (Api.IsEditor
                    && Api.Shared.IsDebug)
                {
                    // in debug version of Editor we don't want to create an excessive load
                    // with water tile physics
                    return;
                }

                // full cell blocking
                foreach (var neighborTile in tile.EightNeighborTiles)
                {
                    if (neighborTile.ProtoTile.Kind == TileKind.Water)
                    {
                        continue;
                    }

                    // has a non-water tile nearby - need to create physics tile to block passage here
                    // however, first we need to ensure this tile doesn't have a platform
                    var isPlatformFound = false;
                    foreach (var staticObject in tile.StaticObjects)
                    {
                        if (staticObject.ProtoStaticWorldObject.Kind == StaticObjectKind.Platform)
                        {
                            isPlatformFound = true;
                            break;
                        }
                    }

                    if (isPlatformFound)
                    {
                        continue;
                    }

                    physicsBody.AddShapeRectangle((1, 1));
                    return;
                }

                // this tile doesn't has neighbor non-water tile so we can skip creating physics for it
                return;
            }

            if (tileKind == TileKind.Solid
                && !tile.IsCliffOrSlope)
            {
                // no blocking
                return;
            }

            var neighborTileLeft = tile.NeighborTileLeft;
            var neighborTileRight = tile.NeighborTileRight;

            if (tile.IsSlope)
            {
                // The tile is a slope. Determine if it left or right slope and create according colliders.
                if (neighborTileLeft.IsCliff)
                {
                    if (IsServer
                        && neighborTileRight.IsCliff)
                    {
                        // an issue found - 1-tile slope!
                        // fix this by making left tile slope too
                        Server.World.SetTileData(neighborTileLeft.Position,
                                                 neighborTileLeft.ProtoTile,
                                                 neighborTileLeft.Height,
                                                 isSlope: true,
                                                 isCliff: false);
                        Server.World.FixMapTilesRecentlyModified();
                        return;
                    }

                    // left collider
                    physicsBody.AddShapeRectangle((tileColliderSlopeWidth, 1));
                }

                if (neighborTileRight.IsCliff)
                {
                    // right collider
                    physicsBody.AddShapeRectangle(
                        (tileColliderSlopeWidth, 1),
                        offset: (1d - tileColliderSlopeWidth, 0));
                }

                return;
            }

            // if we get here - the tile is 100% cliff
            // let's test it against neighbor tiles to determine what colliders we need to create
            var tileHeight = tile.Height;
            if (tile.NeighborTileUp.Height > tileHeight)
            {
                // cliff against top tile with higher height - full cell always
                physicsBody.AddShapeRectangle(size: (1, 1));
                AddHitbox(size: (1, 1 - rangedHitboxOffset),
                          offset: (0, rangedHitboxOffset));

                if (neighborTileLeft.Height > tileHeight)
                {
                    AddHitbox(size: (0.5, 1));
                }

                if (neighborTileRight.Height > tileHeight)
                {
                    AddHitbox(size: (0.5, 1),
                              offset: (0.5, 0));
                }

                return;
            }

            var cliffAgainst = NeighborsPattern.None;
            if (tile.NeighborTileDown.Height > tileHeight)
            {
                cliffAgainst |= NeighborsPattern.Bottom;
            }

            if (neighborTileLeft.Height > tileHeight)
            {
                cliffAgainst |= NeighborsPattern.Left;
            }

            if (tile.NeighborTileDownLeft.Height > tileHeight)
            {
                cliffAgainst |= NeighborsPattern.BottomLeft;
            }

            if (tile.NeighborTileUpLeft.Height > tileHeight)
            {
                cliffAgainst |= NeighborsPattern.TopLeft;
            }

            if (neighborTileRight.Height > tileHeight)
            {
                cliffAgainst |= NeighborsPattern.Right;
            }

            if (tile.NeighborTileDownRight.Height > tileHeight)
            {
                cliffAgainst |= NeighborsPattern.BottomRight;
            }

            if (tile.NeighborTileUpRight.Height > tileHeight)
            {
                cliffAgainst |= NeighborsPattern.TopRight;
            }

            if ((cliffAgainst & NeighborsPattern.Left) != 0)
            {
                if ((cliffAgainst & NeighborsPattern.Right) != 0)
                {
                    // both left and right - full cell
                    physicsBody.AddShapeRectangle(size: (1, 1));
                    AddHitbox(size: (1, 1),
                              offset: (0, rangedHitboxOffset));
                    return;
                }

                // left side
                physicsBody.AddShapeRectangle(size: (0.5, 1));
                AddHitbox(size: (0.5, 1));
            }
            else if ((cliffAgainst & NeighborsPattern.TopLeft) != 0)
            {
                physicsBody.AddShapeRectangle(size: (0.5, 1),
                                              offset: (0, 0));

                AddHitbox(size: (0.5, 1),
                          offset: (0, rangedHitboxOffset));
            }

            if ((cliffAgainst & NeighborsPattern.Right) != 0)
            {
                physicsBody.AddShapeRectangle(size: (0.5, 1),
                                              offset: (0.5, 0));
                AddHitbox(size: (0.5, 1),
                          offset: (0.5, 0));
            }
            else if ((cliffAgainst & NeighborsPattern.TopRight) != 0)
            {
                physicsBody.AddShapeRectangle(size: (0.5, 1),
                                              offset: (0.5, 0));

                AddHitbox(size: (0.5, 1),
                          offset: (0.5, rangedHitboxOffset));
            }

            if ((cliffAgainst & NeighborsPattern.BottomLeft) != 0
                && (cliffAgainst & NeighborsPattern.BottomRight) != 0)
            {
                cliffAgainst |= NeighborsPattern.Bottom;
            }

            if ((cliffAgainst & NeighborsPattern.Bottom) != 0)
            {
                physicsBody.AddShapeRectangle(size: (1, tileColliderBottomWidth));
                AddHitbox(size: (1, tileColliderBottomWidth));
                return;
            }

            if ((cliffAgainst & NeighborsPattern.BottomLeft) != 0
                && (cliffAgainst & NeighborsPattern.Left) == 0)
            {
                physicsBody.AddShapeRectangle(size: (0.5, tileColliderBottomWidth));
                AddHitbox(size: (0.5, tileColliderBottomWidth));
            }

            if ((cliffAgainst & NeighborsPattern.BottomRight) != 0
                && (cliffAgainst & NeighborsPattern.Right) == 0)
            {
                physicsBody.AddShapeRectangle(size: (0.5, tileColliderBottomWidth),
                                              offset: (0.5, 0));
                AddHitbox(size: (0.5, tileColliderBottomWidth),
                          offset: (0.5, 0));
            }

            void AddHitbox(Vector2D size, Vector2D? offset = null)
            {
                // add both melee and ranged hitboxes
                physicsBody
                    .AddShapeRectangle(size, offset, CollisionGroups.HitboxMelee)
                    .AddShapeRectangle(size, offset, CollisionGroups.HitboxRanged);
            }
        }

        public virtual ProtoTileGroundTexture GetGroundTexture(Vector2Ushort tilePosition)
        {
            if (!ClientTileBlendHelper.IsBlendingEnabled)
            {
                // return default texture set
                return this.groundTextures[0];
            }

            // go through all texture sets in reverse order and select the first one which is matched by the noise selector
            for (var index = this.groundTextures.Count - 1; index >= 1; index--)
            {
                var set = this.groundTextures[index];
                if (set.NoiseSelector.IsMatch(tilePosition.X, tilePosition.Y, rangeMultiplier: 1))
                {
                    return set;
                }
            }

            // return default texture set
            return this.groundTextures[0];
        }

        public ITextureResource GetWorldMapTexture(Tile tile)
        {
            if (tile.IsCliff)
            {
                return WorldMapTextureCliff;
            }

            if (this.Kind == TileKind.Water)
            {
                return ((IProtoTileWater)this).WorldMapTexture;

                ;
            }

            // solid ground
            return this.WorldMapTexture;
        }

        public virtual void ServerFixHeight(Tile tile)
        {
            var properHeight = tile.Height;

            foreach (var neighborTile in tile.EightNeighborTiles)
            {
                if (neighborTile.IsOutOfBounds)
                {
                    continue;
                }

                var neighborHeight = neighborTile.Height;
                if (properHeight <= neighborHeight)
                {
                    // consider only neighbor tiles with height lower than the current tile height
                    continue;
                }

                var heightDifference = neighborHeight - properHeight;
                if (heightDifference == 1)
                {
                    continue;
                }

                // found a neighbor tile which is more than one unit height lower than current tile
                properHeight = (byte)(neighborHeight + 1);
            }

            if (properHeight != tile.Height)
            {
                // need to fix tile height
                //Api.Logger.WriteDev($"Fixing tile height: {tile.Position} {tile.Height} -> {properHeight}");
                Server.World.SetTileData(
                    tile.Position,
                    null,
                    properHeight,
                    isCliff: tile.IsCliff,
                    isSlope: tile.IsSlope);
            }
        }

        public void ServerRefresh(Tile tile)
        {
            if (tile.ProtoTile is TilePlaceholder)
            {
                return;
            }

            var tileHeight = tile.Height;
            var isShouldBeCliff = false;
            var isNeighborsFixRequired = false;
            foreach (var neighborTile in tile.EightNeighborTiles)
            {
                if (neighborTile.IsOutOfBounds)
                {
                    continue;
                }

                var neighborHeight = neighborTile.Height;
                if (tileHeight >= neighborHeight)
                {
                    // consider only neighbor tiles with height higher than the current tile height
                    continue;
                }

                if (neighborHeight - tileHeight > 1)
                {
                    // incorrect tile height found - difference more than one
                    // add to the tiles height fix list
                    isNeighborsFixRequired = true;
                    Server.World.ScheduleFixTileHeight(neighborTile.Position);
                }

                isShouldBeCliff = true;
            }

            if (isNeighborsFixRequired)
            {
                // don't do anything - this method will be invoked again for this tile after fixing neighbors
                return;
            }

            var isCliffOrSlope = tile.IsCliffOrSlope;
            if (isShouldBeCliff == isCliffOrSlope)
            {
                // this is a correct tile
                return;
            }

            if (!isShouldBeCliff)
            {
                // should not be a cliff or a slope
                Server.World.SetTileData(
                    tilePosition: tile.Position,
                    protoTile: null,
                    tileHeight: tile.Height,
                    isSlope: false,
                    isCliff: false);
            }
            else
            {
                // should be a cliff
                Server.World.SetTileData(
                    tilePosition: tile.Position,
                    protoTile: null,
                    tileHeight: tile.Height,
                    isSlope: false,
                    isCliff: true);
            }
        }

        public override string ToString()
        {
            // ReSharper disable once CanExtractXamlLocalizableStringCSharp
            return $"Proto Tile \"{this.ShortId}\"";
        }

        protected virtual ITextureResource ClientSetupTileRendering(Tile tile, IClientSceneObject sceneObject)
        {
            this.ClientRenderTileDebugInfo(tile, sceneObject);
            if (this.Kind == TileKind.Water
                && ClientTileBlendHelper.IsBlendingEnabled)
            {
                ClientTileWaterHelper.CreateWaterRenderer(sceneObject, (IProtoTileWater)tile.ProtoTile);
            }

            ClientTileCliffsRendererHelper.CreateCliffsRenderersIfNeeded(tile, sceneObject);
            ClientTileBlendHelper.CreateTileBlendRenderers(tile, this, sceneObject);
            ClientTileDecalHelper.CreateDecalRenderers(tile, this.decals, sceneObject);

            if (this.Kind == TileKind.Water)
            {
                if (!ClientTileBlendHelper.IsBlendingEnabled)
                {
                    // when blending is disabled we don't want to render a super expensive animated water
                    return this.TextureWaterWorldPlaceholder;
                }

                return null;
            }

            return this.GetGroundTexture(tile.Position).Texture;
        }

        /// <summary>
        /// Gets the texture resource used as icon in Editor toolbar (executed once - for caching).
        /// </summary>
        /// <returns>Icon texture resource.</returns>
        protected virtual ITextureResource GetEditorIconTexture()
        {
            // TODO: return only 256x256 chunk of this texture! Consider using a procedural texture for that
            return this.groundTextures[0].Texture;
        }

        /// <summary>
        /// Prepares prototype - invoked after all scripts are loaded, so you can access other scripting
        /// entities by using <see cref="ProtoEntity.GetProtoEntity{TProtoEntity}" /> and
        /// <see cref="ProtoEntity.FindProtoEntities{TProtoEntity}" /> methods.
        /// </summary>
        protected sealed override void PrepareProto()
        {
            base.PrepareProto();

            if (IsServer)
            {
                return;
            }

            var settings = new Settings();
            this.PrepareProtoTile(settings);

            this.WorldMapTexture = SharedCreateMapTexture(this.WorldMapTexturePath);

            this.groundTextures =
                new ReadOnlyListWrapper<ProtoTileGroundTexture>(settings.GroundTextures, ensureNotChanged: false);
            this.decals = new ReadOnlyListWrapper<ProtoTileDecal>(settings.Decals, ensureNotChanged: false);

            this.EditorIconTexture = this.GetEditorIconTexture();

            // prepare ground textures
            foreach (var groundTexture in this.groundTextures)
            {
                groundTexture.Prepare(this);
            }

            // prepare tile decals
            foreach (var decal in this.decals)
            {
                decal.Prepare(this);
            }

            this.AmbientSoundProvider = settings.AmbientSoundProvider
                                        ?? new TileAmbientSoundProvider(default);
        }

        protected abstract void PrepareProtoTile(Settings settings);

        private static TextureResource SharedCreateMapTexture(string worldMapTexturePath)
        {
            var worldMapTexture = worldMapTexturePath != null
                                      ? new TextureResource(worldMapTexturePath,
                                                            isTransparent: false,
                                                            qualityOffset: -100)
                                      : TextureResource.NoTexture;
            return worldMapTexture;
        }

        [SuppressMessage("ReSharper", "CanExtractXamlLocalizableStringCSharp")]
        [Conditional("TILE_DEBUG")]
        private void ClientRenderTileDebugInfo(Tile tile, IClientSceneObject sceneObject)
        {
            var debugText = "H" + tile.Height;
            if (tile.IsSlope)
            {
                debugText += "S";
            }
            else if (tile.IsCliff)
            {
                debugText += "C";
            }

            Api.Client.UI.AttachControl(
                sceneObject,
                new TextBlock()
                {
                    Text = debugText,
                    Foreground = Brushes.White
                },
                positionOffset: (0.5, 0.5),
                isFocusable: false);
        }

        protected class Settings
        {
            internal readonly List<ProtoTileDecal> Decals = new List<ProtoTileDecal>();

            internal readonly List<ProtoTileGroundTexture> GroundTextures = new List<ProtoTileGroundTexture>();

            public TileAmbientSoundProvider AmbientSoundProvider { get; set; }

            public Settings AddDecal(ProtoTileDecal decal)
            {
                this.Decals.Add(decal);
                return this;
            }

            public void AddDecalDoubleWithOffset(
                List<ITextureResource> textures,
                Vector2Ushort size,
                INoiseSelector noiseSelector,
                Vector2Ushort? interval = null,
                DrawOrder drawOrder = DrawOrder.GroundDecals,
                bool requiresCompleteNoiseSelectorCoverage = false,
                bool requiresCompleteProtoTileCoverage = false,
                bool canFlipHorizontally = true)
            {
                this.AddDecal(
                    new ProtoTileDecal(textures,
                                       size: size,
                                       interval: interval,
                                       drawOrder: drawOrder,
                                       requiresCompleteNoiseSelectorCoverage: requiresCompleteNoiseSelectorCoverage,
                                       requiresCompleteProtoTileCoverage: requiresCompleteProtoTileCoverage,
                                       noiseSelector: noiseSelector,
                                       canFlipHorizontally: canFlipHorizontally));

                // add the same decal but with a little offset (to make a more dense diagonal placement)
                this.AddDecal(
                    new ProtoTileDecal(textures,
                                       size: size,
                                       interval: interval,
                                       drawOrder: drawOrder,
                                       offset: (1, 1),
                                       requiresCompleteNoiseSelectorCoverage: requiresCompleteNoiseSelectorCoverage,
                                       requiresCompleteProtoTileCoverage: requiresCompleteProtoTileCoverage,
                                       noiseSelector: noiseSelector,
                                       canFlipHorizontally: canFlipHorizontally));
            }

            public Settings AddGroundTexture(ProtoTileGroundTexture groundTexture)
            {
                this.GroundTextures.Add(groundTexture);
                return this;
            }
        }
    }
}