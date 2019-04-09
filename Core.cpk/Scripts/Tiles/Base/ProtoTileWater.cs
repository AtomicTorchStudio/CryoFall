namespace AtomicTorch.CBND.CoreMod.Tiles
{
    using System.Collections.Generic;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Primitives;

    public abstract class ProtoTileWater : ProtoTile, IProtoTileWater
    {
        private static readonly TextureAtlasResource BlendMaskTextureAtlas
            = new TextureAtlasResource(
                "Terrain/Water/MaskWater.png",
                columns: 4,
                rows: 1,
                isTransparent: false);

        private static readonly IReadOnlyList<EffectResource> EffectForMaskCounts
            = new[]
            {
                new EffectResource("Terrain/WaterTileBlendOneMaskLayer.fx"),
                new EffectResource("Terrain/WaterTileBlendTwoMaskLayers.fx"),
                new EffectResource("Terrain/WaterTileBlendThreeMaskLayers.fx"),
                new EffectResource("Terrain/WaterTileBlendFourMaskLayers.fx")
            };

        private static readonly EffectResource WaterEffectResource
            = new EffectResource("Terrain/WaterTile.fx");

        // ReSharper disable once CollectionNeverQueried.Local
        private readonly List<RenderingMaterial> allMaterials = new List<RenderingMaterial>();

        private readonly Dictionary<TileBlendSides, RenderingMaterial> cachedBlendMaskMaterials
            = new Dictionary<TileBlendSides, RenderingMaterial>();

        private RenderingMaterial waterPrimaryMaterial;

        public override GroundSoundMaterial GroundSoundMaterial => GroundSoundMaterial.Liquid;

        public sealed override TileKind Kind => TileKind.Water;

        public abstract TextureResource UnderwaterGroundTextureAtlas { get; }

        protected virtual float ShoreMaskSpeed => 0.2f;

        protected virtual float WaterAmplitude => 0.075f;

        protected abstract Color WaterColor { get; }

        protected virtual TextureResource WaterSufraceTexture { get; }
            = new TextureResource("Terrain/Water/WaterSurface.png");

        public RenderingMaterial ClientGetWaterBlendMaterial(ClientTileBlendHelper.BlendLayer blendLayer)
        {
            var cacheKey = blendLayer.BlendSides;

            if (this.cachedBlendMaskMaterials.TryGetValue(cacheKey, out var material))
            {
                return material;
            }

            var masks = new List<TileMaskSet>();
            ClientTileBlendHelper.CalculateMasks(blendLayer.BlendSides, masks);
            masks = ClientTileBlendHelper.DetectMasksInnerCorners(masks);
            Api.Assert(masks.Count > 0, "No masks?!");

            var effect = EffectForMaskCounts[masks.Count - 1];
            material = RenderingMaterial.Create(effect);
            this.SetupMaterial(material);

            var effectParameters = material.EffectParameters;
            effectParameters.Set("MaskTextureArray", BlendMaskTextureAtlas);

            for (var i = 0; i < masks.Count; i++)
            {
                var mask = masks[i];
                var flip = (mask.IsFlipHorizontally ? 1f : 0,
                            mask.IsFlipVertically ? 1f : 0);
                effectParameters.Set("Mask" + (i + 1) + "ArraySlice", mask.TextureMaskArraySlice)
                                .Set("Mask" + (i + 1) + "Flip", flip);
            }

            material.EffectResource = effect;
            this.cachedBlendMaskMaterials[cacheKey] = material;
            this.allMaterials.Add(material);
            return material;
        }

        public RenderingMaterial ClientGetWaterPrimaryMaterial()
        {
            return this.waterPrimaryMaterial;
        }

        protected override ITextureResource GetEditorIconTexture()
        {
            return this.TextureWaterWorldPlaceholder;
        }

        protected override void PrepareProtoTile(Settings settings)
        {
            if (IsServer)
            {
                return;
            }

            // Please note: this is a texture used for the ground UNDER the water.
            // The water sprite itself is assigned at ClientTileWaterHelper class.
            settings.GroundTextures.Add(
                new ProtoTileGroundTexture(
                    texture: this.UnderwaterGroundTextureAtlas,
                    blendMaskTexture: BlendMaskTextureSpraySmooth,
                    noiseSelector: null));

            this.waterPrimaryMaterial = RenderingMaterial.Create(WaterEffectResource);
            this.SetupMaterial(this.waterPrimaryMaterial);

            this.allMaterials.Add(this.waterPrimaryMaterial);
        }

        private void SetupMaterial(RenderingMaterial renderingMaterial)
        {
            renderingMaterial.EffectParameters
                             .Set("WaterTexture", this.WaterSufraceTexture)
                             .Set("WaterOpacity", this.WaterColor.A / (float)byte.MaxValue)
                             .Set("WaterColor",
                                  // ReSharper disable once RedundantNameQualifier
                                  new Vector3(
                                      this.WaterColor.R / (float)byte.MaxValue,
                                      this.WaterColor.G / (float)byte.MaxValue,
                                      this.WaterColor.B / (float)byte.MaxValue))
                             .Set("ShoreMaskSpeed", this.ShoreMaskSpeed);
            //.Set("WaterAmplitude", this.WaterAmplitude);
        }
    }
}