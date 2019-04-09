namespace AtomicTorch.CBND.CoreMod.Editor.Tools.Base
{
    using System.Collections.Generic;
    using System.Windows;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;

    [RemoteAuthorizeServerOperator]
    public abstract class BaseEditorTool : ProtoEntity
    {
        public static readonly IReadOnlyList<EditorToolItemFilter> DefaultFiltersReadOnlyList;

        public static readonly ITextureResource TextureFilterAll;

        private static readonly IReadOnlyList<BaseEditorToolItem> EmptyItemsReadOnlyList =
            new List<BaseEditorToolItem>(0).AsReadOnly();

        protected readonly string ToolTexturesPath;

        static BaseEditorTool()
        {
            TextureFilterAll = new TextureResource("Editor/DefaultFilterIcon");
            DefaultFiltersReadOnlyList =
                new List<EditorToolItemFilter>
                {
                    new EditorToolItemFilter("All", TextureFilterAll)
                }.AsReadOnly();
        }

        protected BaseEditorTool()
        {
            var toolName = this.GetType().Name.Replace("Editor", string.Empty);
            this.ToolTexturesPath = "Editor/" + toolName + "/";
            this.Icon = new TextureResource(this.ToolTexturesPath + "ToolIcon.png");
        }

        public virtual IReadOnlyList<EditorToolItemFilter> AbstractFilters => DefaultFiltersReadOnlyList;

        public virtual IReadOnlyList<BaseEditorToolItem> AbstractItems => EmptyItemsReadOnlyList;

        public bool HasSettings { get; private set; }

        public TextureResource Icon { get; }

        public abstract int Order { get; }

        public abstract BaseEditorActiveTool Activate(BaseEditorToolItem item);

        public virtual FrameworkElement CreateSettingsControl()
        {
            return null;
        }

        protected override void PrepareProto()
        {
            base.PrepareProto();

            var methodInfo = this.GetType().GetMethod(nameof(this.CreateSettingsControl));
            var isCreateSettingsMethodOverriden = methodInfo.DeclaringType != typeof(BaseEditorTool);
            this.HasSettings = isCreateSettingsMethodOverriden;
        }
    }

    public abstract class BaseEditorTool<TItem> : BaseEditorTool
        where TItem : BaseEditorToolItem
    {
        private IReadOnlyList<EditorToolItemFilter<TItem>> filters;

        private IReadOnlyList<TItem> items;

        protected BaseEditorTool()
        {
        }

        public sealed override IReadOnlyList<EditorToolItemFilter> AbstractFilters => this.Filters;

        public sealed override IReadOnlyList<BaseEditorToolItem> AbstractItems => this.Items;

        public IReadOnlyList<EditorToolItemFilter<TItem>> Filters
        {
            get
            {
                if (this.filters == null)
                {
                    var filters = new List<EditorToolItemFilter<TItem>>();
                    // add default filter (all items allowed)
                    filters.Add(new EditorToolItemFilter<TItem>("All", TextureFilterAll, item => true));
                    this.SetupFilters(filters);
                    Api.Assert(filters.Count > 0, "There is should be at least one filter");
                    this.filters = filters;
                }

                return this.filters;
            }
        }

        public IReadOnlyList<TItem> Items
        {
            get
            {
                if (this.items == null)
                {
                    var items = new List<TItem>();
                    this.SetupItems(items);
                    this.items = items;
                }

                return this.items;
            }
        }

        public sealed override BaseEditorActiveTool Activate(BaseEditorToolItem item)
        {
            return this.Activate((TItem)item);
        }

        public abstract BaseEditorActiveTool Activate(TItem item);

        protected abstract void SetupFilters(List<EditorToolItemFilter<TItem>> filters);

        protected abstract void SetupItems(List<TItem> items);
    }
}