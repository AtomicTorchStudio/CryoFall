namespace AtomicTorch.CBND.CoreMod.Tiles
{
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class TileLakeArctic : ProtoTile, IProtoTileCold
    {
        public override byte BlendOrder => byte.MaxValue;

        // Snow is limiting the movement speed a bit.
        public override double CharacterMoveSpeedMultiplier => 0.9;

        public override GroundSoundMaterial GroundSoundMaterial => GroundSoundMaterial.Solid;

        public override bool IsRestrictingConstruction => true;

        public override TileKind Kind => TileKind.Solid;

        public override string Name => Api.GetProtoEntity<TileWaterLake>().Name;

        public override string WorldMapTexturePath
            => "Map/LakeArctic.png";

        protected virtual TextureResource GroundTexture1
            => new("Terrain/LakeArctic/TileLakeArctic1.jpg",
                   isTransparent: false);

        protected override void PrepareProtoTile(Settings settings)
        {
            settings.AmbientSoundProvider = new TileForestAmbientSoundProvider(
                daySoundPresetPlains: new AmbientSoundPreset(new SoundResource("Ambient/BorealPlains")),
                daySoundPresetForest: new AmbientSoundPreset(new SoundResource("Ambient/BorealForest")),
                nightSoundPresetPlains: new AmbientSoundPreset(new SoundResource("Ambient/BorealPlainsNight")),
                nightSoundPresetForest: new AmbientSoundPreset(new SoundResource("Ambient/BorealForestNight")));

            settings.AddGroundTexture(
                new ProtoTileGroundTexture(
                    texture: this.GroundTexture1,
                    blendMaskTexture: BlendMaskTextureSpraySmooth,
                    noiseSelector: null));
        }
    }
}