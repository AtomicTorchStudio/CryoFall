namespace AtomicTorch.CBND.CoreMod.Tiles
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Noise;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ProtoTileDecal
    {
        public readonly DrawOrder DrawOrder;

        public readonly DecalHidingSetting HidingSetting;

        public readonly Vector2Ushort Interval;

        public readonly INoiseSelector NoiseSelector;

        public readonly Vector2Ushort Offset;

        public readonly IReadOnlyList<ProtoTileGroundTexture> RequiredGroundTextures;

        public readonly bool RequiresCompleteNoiseSelectorCoverage;

        public readonly Vector2Ushort Size;

        public readonly IReadOnlyList<ITextureResource> TextureResources;

        public ProtoTileDecal(
            IReadOnlyList<ITextureResource> textureResources,
            Vector2Ushort size,
            INoiseSelector noiseSelector,
            Vector2Ushort? interval = null,
            DecalHidingSetting hidingSetting = DecalHidingSetting.StructureOrFloorObject,
            DrawOrder drawOrder = DrawOrder.GroundDecals,
            Vector2Ushort offset = default,
            bool requiresCompleteNoiseSelectorCoverage = false,
            bool canFlipHorizontally = true,
            bool requiresCompleteProtoTileCoverage = false,
            IReadOnlyList<ProtoTileGroundTexture> requiredGroundTextures = null)
        {
            if (size.X > ScriptingConstants.MaxTerrainDecalWidthOrHeight
                || size.Y > ScriptingConstants.MaxTerrainDecalWidthOrHeight)
            {
                throw new Exception("Max decal size is 2x2");
            }

            this.Size = size;
            this.Offset = offset;
            this.RequiresCompleteNoiseSelectorCoverage = requiresCompleteNoiseSelectorCoverage;
            this.NoiseSelector = noiseSelector;
            this.TextureResources = textureResources;
            this.DrawOrder = drawOrder;
            this.HidingSetting = hidingSetting;
            this.RequiredGroundTextures = requiredGroundTextures?.Count > 0
                                              ? requiredGroundTextures
                                              : null;

            this.Interval = new Vector2Ushort((ushort)(this.Size.X + (interval?.X ?? 0)),
                                              (ushort)(this.Size.Y + (interval?.Y ?? 0)));

            this.CanFlipHorizontally = canFlipHorizontally;
            this.RequiresCompleteProtoTileCoverage = requiresCompleteProtoTileCoverage;
        }

        public ProtoTileDecal(
            string localTextureFilePath,
            Vector2Ushort size,
            INoiseSelector noiseSelector,
            Vector2Ushort? interval = null,
            DecalHidingSetting hidingSetting = DecalHidingSetting.StructureOrFloorObject,
            DrawOrder drawOrder = DrawOrder.GroundDecals,
            Vector2Ushort offset = default,
            bool requiresCompleteNoiseSelectorCoverage = false,
            bool canFlipHorizontally = true,
            bool requiresCompleteProtoTileCoverage = false,
            IReadOnlyList<ProtoTileGroundTexture> requiredGroundTextures = null)
            : this(CollectTextures(localTextureFilePath),
                   size,
                   noiseSelector,
                   interval,
                   hidingSetting,
                   drawOrder,
                   offset,
                   requiresCompleteNoiseSelectorCoverage,
                   canFlipHorizontally,
                   requiresCompleteProtoTileCoverage,
                   requiredGroundTextures)
        {
        }

        public bool CanFlipHorizontally { get; }

        public ProtoTile ProtoTile { get; private set; }

        public bool RequiresCompleteProtoTileCoverage { get; }

        public static List<ITextureResource> CollectTextures(params string[] localTextureFilePath)
        {
            var list = new List<ITextureResource>();
            foreach (var path in localTextureFilePath)
            {
                CollectTexturesInternal(path, list);
            }

            return list;
        }

        public ITextureResource GetTexture(Vector2Ushort tilePosition, out DrawMode drawMode)
        {
            var textureIndex = PositionalRandom.Get(
                tilePosition,
                minInclusive: 0,
                maxExclusive: this.TextureResources.Count,
                seed: 792396596);

            if (this.CanFlipHorizontally)
            {
                drawMode = PositionalRandom.Get(
                               tilePosition,
                               minInclusive: 0,
                               maxExclusive: 2,
                               seed: 628275376)
                           == 0
                               ? DrawMode.FlipHorizontally
                               : DrawMode.Default;
            }
            else
            {
                drawMode = DrawMode.Default;
            }

            return this.TextureResources[textureIndex];
        }

        public void Prepare(ProtoTile protoTile)
        {
            this.ProtoTile = protoTile;
        }

        private static void CollectTexturesInternal(string localTextureFilePath, List<ITextureResource> list)
        {
            using var tempFilesList = Api.Shared.FindFilesWithTrailingNumbers(
                ContentPaths.Textures + localTextureFilePath);
            if (tempFilesList.Count == 0)
            {
                throw new Exception("No decal textures found: " + localTextureFilePath);
            }

            foreach (var file in tempFilesList.AsList())
            {
                list.Add(new TextureResource(file));
            }
        }
    }
}