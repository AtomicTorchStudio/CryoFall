namespace AtomicTorch.CBND.CoreMod.Helpers.Client.Floors
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Helpers.Primitives;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient.Rendering;
    using AtomicTorch.GameEngine.Common.Extensions;
    using JetBrains.Annotations;

    /// <summary>
    /// This class has method to generate atlas texture for walls.
    /// It knows all possible combinations of walls and creates atlas with prerendered sprites for all of them.
    /// </summary>
    public class FloorTextureComposer
    {
        private const byte PrimaryAtlasColumnsCount = 8;

        public static readonly IReadOnlyDictionary<NeighborsPattern, FloorChunkPreset> FloorChunkPresets;

        private static readonly TextureAtlasSize AtlasSize;

        private static readonly Lazy<FloorTextureComposer> LazyInstance
            = new(() => new FloorTextureComposer());

        public readonly ushort AtlasTextureHeight;

        public readonly ushort AtlasTextureWidth;

        // It's safe to use SpriteQualitySizeMultiplier here
        // as this value is set only once and cannot be changed without restarting.
        private readonly int TileTextureSize =
            (ushort)Math.Round(ScriptingConstants.TileSizeRealPixels * Api.Client.Rendering.SpriteQualitySizeMultiplier,
                               MidpointRounding.AwayFromZero);

        static FloorTextureComposer()
        {
            var patternsForLayers = FloorSpritePatterns.CreatePatternsForPrimaryLayers();
            var allCombinations = GenerateNeighborsCombinations();

            // All possible combinations are known now.
            // Let's check all known combinations against all known patterns and setup atlas chunks.
            var wallChunkTypes = GenerateAtlas(
                allCombinations,
                patternsForLayers,
                PrimaryAtlasColumnsCount,
                out var atlasSize);
            AtlasSize = atlasSize;
            FloorChunkPresets = wallChunkTypes;
        }

        private FloorTextureComposer()
        {
            var tileSize = this.TileTextureSize;
            this.AtlasTextureWidth = (ushort)(AtlasSize.ColumnsCount * tileSize);
            this.AtlasTextureHeight = (ushort)(AtlasSize.RowsCount * tileSize);
        }

        public static FloorTextureComposer Instance => LazyInstance.Value;

        public static ITextureAtlasResource CreateProceduralTexture(
            [NotNull] string name,
            [NotNull] TextureAtlasResource sourceTextureAtlas)
        {
            var proceduralTextureResource = new ProceduralTexture(
                name,
                isTransparent: true,
                isUseCache: true,
                generateTextureCallback:
                request => Instance.GenerateProceduralTexture(name, sourceTextureAtlas, request),
                dependsOn: new[] { sourceTextureAtlas });

            var columnsCount = AtlasSize.ColumnsCount;
            var rowsCount = AtlasSize.RowsCount;
            return new TextureAtlasResource(
                proceduralTextureResource,
                columnsCount,
                rowsCount);
        }

        public async Task<ITextureResource> GenerateProceduralTexture(
            [NotNull] string textureName,
            [NotNull] TextureAtlasResource sourceTextureAtlas,
            ProceduralTextureRequest request)
        {
            var result = await this.GenerateProceduralTextureInternal(
                             textureName,
                             sourceTextureAtlas,
                             request,
                             isAtlas: true);
            return (IGeneratedTexture2DAtlas)result;
        }

        public Task<IGeneratedTexture2D> GenerateProceduralTextureNonAtlas(
            [NotNull] string textureName,
            [NotNull] TextureAtlasResource wallSpriteAtlas,
            ProceduralTextureRequest request)
        {
            return this.GenerateProceduralTextureInternal(
                textureName,
                wallSpriteAtlas,
                request,
                isAtlas: false);
        }

        private static int CalculateLayersHashCode(FloorSourceChunk[] newChunkLayers)
        {
            var sum = 0;
            foreach (var c in newChunkLayers)
            {
                sum += c.GetHashCode();
            }

            return sum;
        }

        private static IEnumerable<NeighborsPattern> EnumerateCombinations(
            List<NeighborsPattern> list,
            int startingIndex)
        {
            if (startingIndex == list.Count)
            {
                // no elements
                yield break;
            }

            var result = list[startingIndex];
            yield return result;

            foreach (var secondaryPermutation in EnumerateCombinations(list, startingIndex + 1))
            {
                yield return secondaryPermutation;
                yield return result | secondaryPermutation;
            }
        }

        private static Dictionary<NeighborsPattern, FloorChunkPreset> GenerateAtlas(
            List<NeighborsPattern> allCombinations,
            List<FloorPatternLayer> patternsForLayers,
            byte columnsCount,
            out TextureAtlasSize atlasSize)
        {
            var selectedChunks = new List<FloorChunkPreset>();
            var atlasPosition = 0;

            var layers = new List<FloorPatternLayer>(8);
            foreach (var variant in allCombinations)
            {
                foreach (var patternLayer in patternsForLayers)
                {
                    if (patternLayer.IsPass(variant))
                    {
                        layers.Add(patternLayer);
                    }
                }

                if (layers.Count == 0)
                {
                    continue;
                }

                var newChunkLayers = layers.SelectMany(l => l.SourceChunks).ToArray();
                var newChunkLayersHashCode = CalculateLayersHashCode(newChunkLayers);

                FloorChunkPreset wallChunkPreset = null;

                foreach (var chunk in selectedChunks)
                {
                    if (chunk.Layers is not null
                        && chunk.LayersHashCode == newChunkLayersHashCode
                        && chunk.Layers.SequenceEqual(newChunkLayers))
                    {
                        // the same layers are used - reuse its target address
                        wallChunkPreset = new FloorChunkPreset(
                            variant,
                            chunk.TargetColumn,
                            chunk.TargetRow,
                            null,
                            newChunkLayersHashCode,
                            linkedLayersFromOtherChunk: chunk.Layers);
                        break;
                    }
                }

                if (wallChunkPreset is null)
                {
                    // new chunk preset
                    wallChunkPreset =
                        new FloorChunkPreset(
                            variant,
                            (byte)(atlasPosition % columnsCount),
                            (byte)(atlasPosition / columnsCount),
                            newChunkLayers,
                            newChunkLayersHashCode,
                            linkedLayersFromOtherChunk: null);
                    atlasPosition++;
                }

                selectedChunks.Add(wallChunkPreset);
                layers.Clear();
            }

            var wallChunkTypes = selectedChunks.ToDictionary(p => p.Pattern);

            // Atlas chunks are ready - calculate atlas size.
            var rowsCount = (byte)(1 + atlasPosition / columnsCount);
            atlasSize = new TextureAtlasSize(columnsCount, rowsCount);

            foreach (var selectedChunk in selectedChunks)
            {
                if (selectedChunk.TargetRow >= rowsCount)
                {
                    Api.Logger.Error($"Target row position exceeded: {selectedChunk.TargetRow}>={rowsCount}");
                }

                if (selectedChunk.TargetColumn >= columnsCount)
                {
                    Api.Logger.Error($"Target column position exceeded: {selectedChunk.TargetColumn}>={columnsCount}");
                }
            }

            Api.Logger.Important(
                string.Format(
                    "Floor combinations calculated, total: {0} all combinations, {1} selected combinations, with unique atlas chunks {2} combinations.",
                    allCombinations.Count,
                    selectedChunks.Count,
                    selectedChunks.Count(c => c.Layers is not null)));
            return wallChunkTypes;
        }

        private static List<NeighborsPattern> GenerateNeighborsCombinations()
        {
            var primaryDirectionsCombinations = new List<NeighborsPattern>()
            {
                NeighborsPattern.Left,
                NeighborsPattern.Top,
                NeighborsPattern.Right,
                NeighborsPattern.Bottom
            };
            // Generate primary directions combinations - left/top/right/bottom and all their possible 15 combinations.
            primaryDirectionsCombinations = EnumerateCombinations(primaryDirectionsCombinations, 0).ToList();

            // Extra combinations are combinations with filled diagonal cells nearby primary directions.
            // For example:
            // D + D
            // + + -
            // D - -
            // This algorithm will generate combinations of all 'D' cells with all '+' cells.
            var allExtraCombinations = new List<NeighborsPattern>();
            var localExtraDirections = new List<NeighborsPattern>(8);

            foreach (var pattern in primaryDirectionsCombinations)
            {
                if ((pattern & NeighborsPattern.Left) != 0)
                {
                    localExtraDirections.Add(NeighborsPattern.TopLeft);
                    localExtraDirections.Add(NeighborsPattern.BottomLeft);
                }

                if ((pattern & NeighborsPattern.Top) != 0)
                {
                    localExtraDirections.AddIfNotContains(NeighborsPattern.TopLeft);
                    localExtraDirections.Add(NeighborsPattern.TopRight);
                }

                if ((pattern & NeighborsPattern.Right) != 0)
                {
                    localExtraDirections.AddIfNotContains(NeighborsPattern.TopRight);
                    localExtraDirections.Add(NeighborsPattern.BottomRight);
                }

                if ((pattern & NeighborsPattern.Bottom) != 0)
                {
                    localExtraDirections.AddIfNotContains(NeighborsPattern.BottomLeft);
                    localExtraDirections.AddIfNotContains(NeighborsPattern.BottomRight);
                }

                if (localExtraDirections.Count == 0)
                {
                    // no extra variants
                    continue;
                }

                foreach (var combination in EnumerateCombinations(localExtraDirections, 0))
                {
                    allExtraCombinations.Add(pattern | combination);
                }

                localExtraDirections.Clear();
            }

            var allCombinations = primaryDirectionsCombinations.Concat(allExtraCombinations.Distinct()).ToList();
            allCombinations.Add(NeighborsPattern.None);
            return allCombinations;
        }

        private async Task<IGeneratedTexture2D> GenerateProceduralTextureInternal(
            string textureName,
            TextureAtlasResource sourceTextureAtlas,
            ProceduralTextureRequest request,
            bool isAtlas)
        {
            var renderingTag = string.Format("Procedural texture \"{0}\" camera \"{1}\"",
                                             textureName,
                                             sourceTextureAtlas.TextureResource);
            var client = Api.Client;
            var cameraObject = client.Scene.CreateSceneObject(renderingTag);
            var camera = client.Rendering.CreateCamera(cameraObject,
                                                       renderingTag,
                                                       -10);

            var wallChunkTypes = FloorChunkPresets;
            var atlasSize = AtlasSize;
            var textureWidth = this.AtlasTextureWidth;
            var textureHeight = this.AtlasTextureHeight;

            var rendering = Api.Client.Rendering;
            var renderTexture = rendering.CreateRenderTexture(renderingTag, textureWidth, textureHeight);
            camera.RenderTarget = renderTexture;
            // TODO: we cannot use Colors.Transparent because RGB=FFFFFF in that case.
            camera.ClearColor = Color.FromArgb(0, 0, 0, 0);
            camera.SetOrthographicProjection(textureWidth, textureHeight);
            camera.TextureFilter = TextureFilter.Point;

            foreach (var variant in wallChunkTypes.Values)
            {
                if (variant.Layers is null)
                {
                    // reusing layers from another chunk
                    continue;
                }

                var targetRow = variant.TargetRow;
                var targetColumn = variant.TargetColumn;

                if (targetRow >= atlasSize.RowsCount)
                {
                    Api.Logger.Error(
                        $"Floor chunk target row is exceed rows count: {targetRow} >= {atlasSize.RowsCount}");
                }

                if (targetColumn >= atlasSize.ColumnsCount)
                {
                    Api.Logger.Error(
                        $"Floor chunk target column is exceed columns count: {targetColumn} >= {atlasSize.ColumnsCount}");
                }

                foreach (var layer in variant.Layers)
                {
                    rendering.CreateSpriteRenderer(
                        cameraObject,
                        sourceTextureAtlas.Chunk(layer.Column, layer.Row),
                        positionOffset: (this.TileTextureSize * targetColumn, -this.TileTextureSize * targetRow),
                        // draw down
                        spritePivotPoint: (0, 1),
                        renderingTag: renderingTag);
                }
            }

            await camera.DrawAsync();
            cameraObject.Destroy();

            request.ThrowIfCancelled();

            IGeneratedTexture2D generatedTexture;

            if (isAtlas)
            {
                generatedTexture = await renderTexture.SaveToTextureAtlas(atlasSize, isTransparent: true);
                //Api.Logger.Write($"Texture atlas generated: {renderingTag} atlas size: {atlasSize}");
            }
            else
            {
                generatedTexture = await renderTexture.SaveToTexture(isTransparent: true);
            }

            renderTexture.Dispose();
            request.ThrowIfCancelled();
            return generatedTexture;
        }
    }
}