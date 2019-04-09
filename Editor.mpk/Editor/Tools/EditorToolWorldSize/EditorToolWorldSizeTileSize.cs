namespace AtomicTorch.CBND.CoreMod.Editor.Tools.EditorToolWorldSize
{
    public struct EditorToolWorldSizeTileSize
    {
        public EditorToolWorldSizeTileSize(ushort value)
        {
            this.Value = value;
        }

        public string Description => this.Value + " tiles";

        public ushort Value { get; }

        public override string ToString()
        {
            return this.Description;
        }
    }
}