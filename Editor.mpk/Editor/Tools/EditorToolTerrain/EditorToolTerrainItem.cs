namespace AtomicTorch.CBND.CoreMod.Editor.Tools.EditorToolTerrain
{
    using AtomicTorch.CBND.CoreMod.Editor.Tools.Base;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;

    public class EditorToolTerrainItem : BaseEditorToolItem
    {
        public readonly IProtoTile ProtoTile;

        public EditorToolTerrainItem(IProtoTile protoTile)
            : base(protoTile?.Name ?? "(don't change the terrain sprite)",
                   id: string.Empty,
                   displayShortName: protoTile != null)
        {
            this.ProtoTile = protoTile;
        }

        public override ITextureResource Icon => this.ProtoTile?.EditorIconTexture
                                                 ?? new TextureResource("Editor/ToolTerrain/IconDoNotChange");
    }
}