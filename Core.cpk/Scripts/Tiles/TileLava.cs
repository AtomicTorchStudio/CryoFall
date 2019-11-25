namespace AtomicTorch.CBND.CoreMod.Tiles
{
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Rendering.Lighting;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class TileLava : ProtoTileWater
    {
        public override byte BlendOrder => byte.MaxValue;

        public override GroundSoundMaterial GroundSoundMaterial => GroundSoundMaterial.Mud;

        public override string Name => "Lava";

        // used for blending only (if lava on the same height level which is not intended)
        public override TextureResource UnderwaterGroundTextureAtlas { get; }
            = new TextureResource("Terrain/LakeShore/TileLakeshoreSand2.jpg",
                                  isTransparent: false);

        public override string WorldMapTexturePath
            => "Map/Lava.png";

        protected override ITextureResource TextureWaterWorldPlaceholder { get; }
            = new TextureResource("Terrain/Lava/TileLavaPlaceholder",
                                  isTransparent: false);

        protected override float WaterAmplitude => 0.02f;

        protected override Color WaterColor => Color.FromArgb(255, 255, 255, 255);

        protected override float WaterColorMix => 0;

        protected override float WaterDiffractionFrequency => 4.0f;

        protected override float WaterDiffractionSpeed => 0.22f;

        protected override float WaterSpeed => 0.3f;

        protected override TextureResource WaterSufraceTexture { get; }
            = new TextureResource("Terrain/Lava/TileLava1.jpg");

        protected override ITextureResource ClientSetupTileRendering(Tile tile, IClientSceneObject sceneObject)
        {
            var position = tile.Position;
            if (position.X % 2 == 0
                && position.Y % 2 == 0)
            {
                // add light source
                ClientLighting.CreateLightSourceSpot(
                    sceneObject,
                    color: LightColors.Lava,
                    size: (12, 12 * 1.5),
                    positionOffset: (0.5, 0.5));
            }

            return base.ClientSetupTileRendering(tile, sceneObject);
        }

        protected override void PrepareProtoTile(Settings settings)
        {
            base.PrepareProtoTile(settings);

            settings.AmbientSoundProvider = new TileAmbientSoundProvider(
                new AmbientSoundPreset(new SoundResource("Ambient/Lava"),
                                       suppressionCoef: 1,
                                       isSupressingMusic: true,
                                       isUsingAmbientVolume: false));
        }
    }
}