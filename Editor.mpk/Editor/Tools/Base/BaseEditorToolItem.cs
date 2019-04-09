namespace AtomicTorch.CBND.CoreMod.Editor.Tools.Base
{
    using AtomicTorch.CBND.GameApi.Resources;

    public abstract class BaseEditorToolItem
    {
        protected BaseEditorToolItem(string name, string id)
        {
            this.Name = name;
            this.Id = id;
        }

        public abstract ITextureResource Icon { get; }

        public string Id { get; }

        public string Name { get; }
    }
}