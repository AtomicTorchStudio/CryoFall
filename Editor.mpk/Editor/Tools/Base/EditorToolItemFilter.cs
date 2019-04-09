namespace AtomicTorch.CBND.CoreMod.Editor.Tools.Base
{
    using System;
    using AtomicTorch.CBND.GameApi.Resources;

    public class EditorToolItemFilter
    {
        public EditorToolItemFilter(string name, ITextureResource icon)
        {
            this.Name = name;
            this.Icon = icon;
        }

        public ITextureResource Icon { get; }

        public string Name { get; }

        public virtual bool FilterItem(BaseEditorToolItem toolItem)
        {
            return true;
        }
    }

    public class EditorToolItemFilter<TItem> : EditorToolItemFilter
        where TItem : BaseEditorToolItem
    {
        private readonly Func<TItem, bool> filterFunc;

        public EditorToolItemFilter(string name, ITextureResource icon, Func<TItem, bool> filterFunc)
            : base(name, icon)
        {
            this.filterFunc = filterFunc;
        }

        public override bool FilterItem(BaseEditorToolItem toolItem)
        {
            return this.filterFunc((TItem)toolItem);
        }
    }
}