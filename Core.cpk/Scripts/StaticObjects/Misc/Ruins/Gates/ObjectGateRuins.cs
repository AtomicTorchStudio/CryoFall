namespace AtomicTorch.CBND.CoreMod.StaticObjects.Misc.Ruins.Gates
{
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;

    public abstract class ObjectGateRuins : ProtoObjectGateRuins
    {
        protected ObjectGateRuins()
        {
            var folderPath = SharedGetRelativeFolderPath(this.GetType(), typeof(ProtoStaticWorldObject<,,>));
            var texturePath = $"StaticObjects/{folderPath}/{nameof(ObjectGateRuins)}";

            this.AtlasTextureHorizontal = new TextureAtlasResource(
                texturePath + "Horizontal",
                columns: 6,
                rows: 1,
                isTransparent: true);

            this.TextureBaseHorizontal = new TextureResource(
                texturePath + "HorizontalBase",
                isTransparent: true);

            this.AtlasTextureVertical = new TextureAtlasResource(
                texturePath + "Vertical",
                columns: 8,
                rows: 1,
                isTransparent: true);
        }

        public override bool IsInteractableObject => false;

        [NotLocalizable]
        public override string Name => "Ruins gate"; // this object is not available for players to build or interact

        public override ObjectMaterial ObjectMaterial => ObjectMaterial.Metal;

        public override SoundResource SoundResourceDoorEnd { get; }
            = new("Objects/Misc/ObjectGateRuins/End");

        public override SoundResource SoundResourceDoorProcess { get; }
            = SoundResource.NoSound;

        public override SoundResource SoundResourceDoorStart { get; }
            = new("Objects/Misc/ObjectGateRuins/Start");

        protected override bool ServerCheckIsDoorShouldBeOpened(
            IStaticWorldObject worldObject,
            ObjectGateRuinsPrivateState privateState)
        {
            return Server.Game.FrameTime <= privateState.OpenedUntil;
        }
    }
}