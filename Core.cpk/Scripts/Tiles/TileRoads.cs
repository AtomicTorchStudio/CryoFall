namespace AtomicTorch.CBND.CoreMod.Tiles
{
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;

    public class TileRoads : ProtoTile
    {
        private static readonly TextureResource GroundTexture1
            = new TextureResource("Terrain/Ruins/TileRuins1.jpg",
                                  isTransparent: false);

        public override byte BlendOrder => 0;

        // characters can move faster on the roads (the same speed as for the floors)
        public override double CharacterMoveSpeedMultiplier => 1.15;

        public override GroundSoundMaterial GroundSoundMaterial => GroundSoundMaterial.Solid;

        public override bool IsRestrictingConstruction => true;

        public override TileKind Kind => TileKind.Solid;

        public override string Name => "Roads";

        public override string WorldMapTexturePath
            => "Map/Roads.png";

        protected override void PrepareProtoTile(Settings settings)
        {
            settings.AddGroundTexture(
                new ProtoTileGroundTexture(
                    texture: GroundTexture1,
                    blendMaskTexture: BlendMaskTextureSprayStraightSmooth,
                    noiseSelector: null));
        }
    }
}