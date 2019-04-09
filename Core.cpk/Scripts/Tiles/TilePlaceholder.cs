namespace AtomicTorch.CBND.CoreMod.Tiles
{
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;

    /// <summary>
    /// Placeholder tile - if no tile type provided for a tile, it will be provided
    /// Please DO NOT rename/remove this, it's required by the game engine.
    /// </summary>
    public sealed class TilePlaceholder : ProtoTile
    {
        public override byte BlendOrder => 0;

        public override GroundSoundMaterial GroundSoundMaterial => GroundSoundMaterial.Solid;

        public override TileKind Kind => TileKind.Placeholder;

        [NotLocalizable]
        public override string Name => "Placeholder";

        public override string WorldMapTexturePath => null;

        //protected override TextureAtlasResource AtlasTexture
        //    => new TextureAtlasResource(TextureResource.NoTexture, 1, 1);

        protected override ITextureResource ClientSetupTileRendering(Tile tile, IClientSceneObject sceneObject)
        {
            // will be rendered as "absent" magenta texture
            return null;
        }

        protected override ITextureResource GetEditorIconTexture()
        {
            // not placeable with editor
            return null;
        }

        protected override void PrepareProtoTile(Settings settings)
        {
            settings.GroundTextures.Add(
                new ProtoTileGroundTexture(
                    texture: TextureResource.NoTexture,
                    blendMaskTexture: new TextureResource("Terrain/TileMaskGeneric"),
                    noiseSelector: null));
        }
    }
}